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

	public Text AnswerTextField{ get; set; }

	private ApiAi apiAi;
	private AudioSource aud;
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

	public void StartListening()
	{
		Debug.Log ("StartListening");

		if (AnswerTextField != null) {
			AnswerTextField.text = "Listening...";
		}

		foreach (var mic in Microphone.devices) {
			int min, max;
			Microphone.GetDeviceCaps (mic, out min, out max);
			Debug.Log (mic + ": " + min + " - " + max);
		}

		aud = GetComponent<AudioSource> ();
		aud.clip = Microphone.Start (null, true, 20, 16000);

		recording = true;

	}
	
	public void StopListening()
	{
		Debug.Log ("StopListening");


		recording = false;


		if (AnswerTextField != null) {
			AnswerTextField.text = "";
		}

		try {
			Microphone.End (null);

			var samples = new float[aud.clip.samples];
			aud.clip.GetData (samples, 0);

			Debug.Log ("samples: " + samples.Length);

			Debug.Log ("channels:" + aud.clip.channels);
			Debug.Log ("freq:" + aud.clip.frequency);

//			Debug.Log("f: " + samples[10] + " " + samples[1000] + " " + samples[2319]
//			          + " " + samples[55777] + " " + samples[112554]);

			var trimmedSamples = TrimSilence (samples);

			if (trimmedSamples != null) {

				var pcm16 = ConvertIeeeToPcm16 (trimmedSamples);
				var bytes = ConvertArrayShortToBytes (pcm16);
				
				//				StartSoundFile ();
//				
//

//				writer.Write (bytes);
//
//				StopSoundFile ();

				Debug.Log ("size: " + bytes.Length);

				var voiceStream = new MemoryStream (bytes);
				voiceStream.Seek (0, SeekOrigin.Begin);
						
				var aiResponse = apiAi.voiceRequest (voiceStream);

				if (aiResponse != null) {
					Debug.Log (aiResponse.Result.resolvedQuery);
					var outText = fastJSON.JSON.ToJSON (aiResponse, 
							new JSONParameters { 
								UseExtensions = false,  
								SerializeNullValues = false,
								EnableAnonymousTypes = true
							});
						
					Debug.Log (outText);
						
					AnswerTextField.text = outText;
						
				} else {
					Debug.LogError ("Response is null");
				}

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

	private float[] TrimSilence(float[] samples)
	{
		//return null;

		float min = 0.000001f;

		int startIndex = 0;
		int endIndex = samples.Length;

		for (int i = 0; i < samples.Length; i++) {

			//Debug.Log ("f: " + samples [i]);

			if (Math.Abs (samples [i]) > min) {
				startIndex = i;
				break;
			}
		}

		Debug.Log ("startIndex: " + startIndex);
		

		for (int i = samples.Length - 1; i > 0; i--) {
			if (Math.Abs (samples [i]) > min) {
				endIndex = i;
				break;
			}
		}

		Debug.Log ("endIndex: " + endIndex);
		

		if (endIndex <= startIndex) {
			return null;
		}

		var result = new float[endIndex - startIndex];
		Array.Copy (samples, startIndex, result, 0, endIndex - startIndex);
		return result;

	}
		
	public static byte[] ConvertArrayShortToBytes(short[] array)
	{
		Debug.Log ("ConvertArrayShortToBytes: " + array.Length);

		byte[] numArray = new byte[array.Length * 2];
		Buffer.BlockCopy ((Array)array, 0, (Array)numArray, 0, numArray.Length);
		return numArray;
	}

	public static short[] ConvertIeeeToPcm16(float[] source)
	{
		Debug.Log ("ConvertIeeeToPcm16: " + source.Length);

		short[] resultBuffer = new short[source.Length];
		for (int i = 0; i < source.Length; i++) {
			float f = source [i] * 32768f;

			if ((double)f > (double)short.MaxValue)
				f = (float)short.MaxValue;
			else if ((double)f < (double)short.MinValue)
				f = (float)short.MinValue;
			resultBuffer [i] = Convert.ToInt16 (f);
		}

		return resultBuffer;
	}
	
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

			AnswerTextField.text = outText;

		} else {
			Debug.LogError ("Response is null");
		}

	}
}
