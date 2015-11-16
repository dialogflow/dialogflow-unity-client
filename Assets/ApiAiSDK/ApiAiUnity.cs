//
// API.AI Unity SDK - Unity libraries for API.AI
// =================================================
//
// Copyright (C) 2015 by Speaktoit, Inc. (https://www.speaktoit.com)
// https://www.api.ai
//
// ***********************************************************************************************************************
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
// an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
//
// ***********************************************************************************************************************

using System;
using UnityEngine;
using ApiAiSDK;
using ApiAiSDK.Model;
using System.Threading;
using System.Linq;
using System.ComponentModel;

#if UNITY_ANDROID
using ApiAiSDK.Unity.Android;
#endif

namespace ApiAiSDK.Unity
{	
	public class ApiAiUnity
	{
		private ApiAi apiAi;
		private AIConfiguration config;
		private AudioSource audioSource;
		private volatile bool recordingActive;
		private readonly object thisLock = new object();

#if UNITY_ANDROID
		private ResultWrapper androidResultWrapper;
		private AndroidRecognizer androidRecognizer;
#endif

		public event EventHandler<AIResponseEventArgs> OnResult;
		public event EventHandler<AIErrorEventArgs> OnError;
		public event EventHandler<EventArgs> OnListeningStarted;
		public event EventHandler<EventArgs> OnListeningFinished;

		public ApiAiUnity()
		{

		}

		public void Initialize(AIConfiguration config)
		{
			this.config = config;

			apiAi = new ApiAi(this.config);

#if UNITY_ANDROID

			if(Application.platform == RuntimePlatform.Android)
			{
				InitializeAndroid();
			}

#endif

		}
			
#if UNITY_ANDROID
		private void InitializeAndroid(){
			androidRecognizer = new AndroidRecognizer();
			androidRecognizer.Initialize();
		}
#endif

		public void Update()
		{

#if UNITY_ANDROID
			if (androidResultWrapper != null) {
				UpdateAndroidResult();
			}
#endif

		}

#if UNITY_ANDROID
		private void UpdateAndroidResult(){
            Debug.Log("UpdateAndroidResult");
            var wrapper = androidResultWrapper;
			if (wrapper.IsReady) {
				var recognitionResult = wrapper.GetResult();
				androidResultWrapper = null;
				androidRecognizer.Clean();
				
				if (recognitionResult.IsError) {
					FireOnError(new Exception(recognitionResult.ErrorMessage));
				} else {
					var request = new AIRequest {
						Query = recognitionResult.RecognitionResults,
						Confidence = recognitionResult.Confidence
					};
					
					var aiResponse = apiAi.TextRequest(request);
					ProcessResult(aiResponse);
				}
			}
		}
#endif

		public void StartListening(AudioSource audioSource)
		{
			lock (thisLock) {
				if (!recordingActive) {
					this.audioSource = audioSource;
					StartRecording();
				} else {
					Debug.LogWarning("Can't start new recording session while another recording session active");
				}
			}
		}

		public void StartNativeRecognition(){
			if (Application.platform != RuntimePlatform.Android) {
				throw new InvalidOperationException("Now only Android supported");
			}

#if UNITY_ANDROID
			if (androidResultWrapper == null) {
				androidResultWrapper = androidRecognizer.Recognize(config.Language.code);
			}
#endif
		}

		public void StopListening()
		{
			if (recordingActive) {

				float[] samples = null;
	
				lock (thisLock) {
					if (recordingActive) {
						StopRecording();
						samples = new float[audioSource.clip.samples];
						audioSource.clip.GetData(samples, 0);
						audioSource = null;
					}
				}

                new Thread(StartVoiceRequest).Start(samples);
			}
		}

        private void StartVoiceRequest(object parameter){
            float[] samples = (float[])parameter;
            if (samples != null) {
                try {
                    var aiResponse = apiAi.VoiceRequest(samples);
                    ProcessResult(aiResponse);  
                } catch (Exception ex) {
                    FireOnError(ex);
                }
            }
        }

		private void ProcessResult(AIResponse aiResponse)
		{
			if (aiResponse != null) {
				FireOnResult(aiResponse);
			} else {
				FireOnError(new Exception("API.AI Service returns null"));
			}
		}

		private void StartRecording()
		{
			audioSource.clip = Microphone.Start(null, true, 20, 16000);
			recordingActive = true;
			FireOnListeningStarted();
		}

		private void StopRecording()
		{
			Microphone.End(null);
			recordingActive = false;
			FireOnListeningFinished();
		}

		private void FireOnResult(AIResponse aiResponse){
			var onResult = OnResult;
			if (onResult != null) {
				onResult(this, new AIResponseEventArgs(aiResponse));
			}
		}

		private void FireOnError(Exception e){
			var onError = OnError;
			if (onError != null) {
				onError(this, new AIErrorEventArgs(e));
			}
		}

		private void FireOnListeningStarted(){
			var onListeningStarted = OnListeningStarted;
			if (onListeningStarted != null) {
				onListeningStarted(this, EventArgs.Empty);
			}
		}

		private void FireOnListeningFinished(){
			var onListeningFinished = OnListeningFinished;
			if (onListeningFinished != null) {
				onListeningFinished(this, EventArgs.Empty);
			}
		}

		public AIResponse TextRequest(string request)
		{
			return apiAi.TextRequest(request);
		}

		private void ResetContexts()
		{
			// TODO
		}

	}

	public class AIResponseEventArgs : EventArgs
	{
		private readonly AIResponse response;
		
		public AIResponse Response {
			get {
				return response;
			}
		}
		
		public AIResponseEventArgs(AIResponse response)
		{
			this.response = response;
		}
	}
	
	public class AIErrorEventArgs : EventArgs
	{
		
		private readonly Exception exception;
		
		public Exception Exception {
			get {
				return exception;
			}
		}
		
		public AIErrorEventArgs(Exception ex)
		{
			exception = ex;
		}
	}
}

