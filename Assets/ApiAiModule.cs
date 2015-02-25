//
// API.AI Unity SDK Sample
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

using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using fastJSON;
using ApiAiSDK;
using ApiAiSDK.Model;
using ApiAiSDK.Unity;

public class ApiAiModule : MonoBehaviour
{

	public Text answerTextField;
	public Text inputTextField;
	private ApiAiUnity apiAiUnity;
	private AudioSource aud;
	public AudioClip listeningSound;
	BinaryWriter writer;
	FileStream fs;
	int blocksWrote;
	int counter;

	// Use this for initialization
	IEnumerator Start()
	{
		// check access to the Microphone
		yield return Application.RequestUserAuthorization (UserAuthorization.Microphone);
		if (!Application.HasUserAuthorization (UserAuthorization.Microphone)) {
			throw new NotSupportedException ("Microphone using not authorized");
		}

		const string SUBSCRIPTION_KEY = "cb9693af-85ce-4fbf-844a-5563722fc27f";
		const string ACCESS_TOKEN = "3485a96fb27744db83e78b8c4bc9e7b7";

		var config = new AIConfiguration (SUBSCRIPTION_KEY, ACCESS_TOKEN, SupportedLanguage.English);

		apiAiUnity = new ApiAiUnity ();
		apiAiUnity.Initialize (config);

		apiAiUnity.OnError += HandleOnError;
		apiAiUnity.OnResult += HandleOnResult;
	}

	void HandleOnResult(object sender, AIResponseEventArgs e)
	{
		var aiResponse = e.Response;
		if (aiResponse != null) {
			Debug.Log (aiResponse.Result.ResolvedQuery);
			var outText = fastJSON.JSON.ToJSON (aiResponse,
			                                   new JSONParameters
			                                   {
													UseExtensions = false,
													SerializeNullValues = false,
													EnableAnonymousTypes = true
											   });
			
			Debug.Log (outText);
			
			answerTextField.text = outText;
			
		} else {
			Debug.LogError ("Response is null");
		}
	}
	
	void HandleOnError(object sender, AIErrorEventArgs e)
	{
		Debug.LogException (e.Exception);
		answerTextField.text = e.Exception.Message;
	}
	
	// Update is called once per frame
	void Update()
	{
		if (apiAiUnity != null) {
			apiAiUnity.Update();
		}
	}
	
	public void PluginInit()
	{
		
	}
	
	public void StartListening()
	{
		Debug.Log ("StartListening");
			
		if (answerTextField != null) {
			answerTextField.text = "Listening...";
		}
			
		aud = GetComponent<AudioSource> ();
		apiAiUnity.StartListening (aud);
	
	}
	
	public void StopListening()
	{
		try {
			Debug.Log ("StopListening");

			if (answerTextField != null) {
				answerTextField.text = "";
			}
			
			apiAiUnity.StopListening ();
		} catch (Exception ex) {
			Debug.LogException (ex);
		}
	}
	
	public void SendText()
	{
		var text = inputTextField.text;

		Debug.Log (text);

		AIResponse response = apiAiUnity.TextRequest (text);

		if (response != null) {
			Debug.Log ("Resolved query: " + response.Result.ResolvedQuery);
			var outText = fastJSON.JSON.ToJSON (response, new JSONParameters
            {
                UseExtensions = false,
                SerializeNullValues = false,
                EnableAnonymousTypes = true
            });

			Debug.Log ("Result: " + outText);

			answerTextField.text = outText;
		} else {
			Debug.LogError ("Response is null");
		}

	}

	public void StartNativeRecognition(){
		try {
			apiAiUnity.StartNativeRecognition();
		} catch (Exception ex) {
			Debug.LogException (ex);
		}
	}
}
