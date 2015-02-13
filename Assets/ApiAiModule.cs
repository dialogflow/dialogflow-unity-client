using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using fastJSON;
using ApiAiSDK;
using ApiAiSDK.model;

public class ApiAiModule : MonoBehaviour
{

	public Text answerTextField;
	private ApiAi apiAi;
	private AudioSource aud;
	public AudioClip listeningSound;
	private volatile bool recording = false;
	BinaryWriter writer;
	FileStream fs;
	int blocksWrote;
	int counter;

	// Use this for initialization
	IEnumerator Start()
	{

		yield return Application.RequestUserAuthorization (UserAuthorization.Microphone);
		if (!Application.HasUserAuthorization (UserAuthorization.Microphone)) {
			throw new NotSupportedException ("Microphone using not authorized");
		} 

		const string SUBSCRIPTION_KEY = "cb9693af-85ce-4fbf-844a-5563722fc27f";
		const string ACCESS_TOKEN = "3485a96fb27744db83e78b8c4bc9e7b7";

		var config = new AIConfiguration (SUBSCRIPTION_KEY, ACCESS_TOKEN, "en");
		config.DebugMode = false;

		apiAi = new ApiAi (config);
	}
	
	// Update is called once per frame
	void Update()
	{
	
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

		foreach (var mic in Microphone.devices) {
			int min, max;
			Microphone.GetDeviceCaps (mic, out min, out max);
			Debug.Log (mic + ": " + min + " - " + max);
		}



		aud = GetComponent<AudioSource> ();

		aud.PlayOneShot (listeningSound);

		//aud.clip = Microphone.Start (null, true, 20, 16000);

		recording = true;

	}
	
	public void StopListening()
	{
		Debug.Log ("StopListening");


		recording = false;


		if (answerTextField != null) {
			answerTextField.text = "";
		}

		try {
			Microphone.End (null);

			var samples = new float[aud.clip.samples];
			aud.clip.GetData (samples, 0);

			Debug.Log ("samples: " + samples.Length);

			Debug.Log ("channels:" + aud.clip.channels);
			Debug.Log ("freq:" + aud.clip.frequency);

		
						
			var aiResponse = apiAi.voiceRequest (samples);

			if (aiResponse != null) {
				Debug.Log (aiResponse.Result.resolvedQuery);
				var outText = fastJSON.JSON.ToJSON (aiResponse, 
							new JSONParameters { 
								UseExtensions = false,  
								SerializeNullValues = false,
								EnableAnonymousTypes = true
							});
						
				Debug.Log (outText);
						
				answerTextField.text = outText;
						
			} else {
				Debug.LogError ("Response is null");
			}

		} catch (Exception ex) {
			Debug.LogException (ex);
		}
	}

//	public void OnAudioFilterRead(float[] data,int channels){
//		Debug.Log("!");
//		counter++;
//		if(recording && writer != null && data != null){
//			var byteArray = new byte[data.Length * 4];
//			Buffer.BlockCopy(data, 0, byteArray, 0, byteArray.Length);
//			writer.Write(byteArray);
//			blocksWrote++;
//		}
//	}

	private void StartSoundFile()
	{
		fs = new FileStream ("recorded_sound.raw", FileMode.Create);
		writer = new BinaryWriter (fs);

	}

	private void StopSoundFile()
	{
		fs.Close ();
		writer.Close ();
		fs = null;
		writer = null;

		Debug.Log ("wrote/counter:" + blocksWrote + " " + counter);
	}
	
	public void SendText(string text)
	{
		Debug.Log (text);

		Debug.Log (System.Environment.Version);

		AIResponse response = apiAi.textRequest (text);

		if (response != null) {
			Debug.Log (response.Result.resolvedQuery);
			var outText = fastJSON.JSON.ToJSON (response, new JSONParameters { 
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
}
