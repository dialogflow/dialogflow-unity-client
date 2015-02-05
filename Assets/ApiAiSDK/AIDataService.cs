using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.IO;
using fastJSON;
using ApiAiSDK.model;

namespace ApiAiSDK
{
	public class AIDataService
	{
		private AIConfiguration config;

		public AIDataService (AIConfiguration config)
		{
			this.config = config;
		}

		public AIResponse Request(AIRequest request)
		{

			request.Language = config.Language;
			request.Timezone = TimeZone.CurrentTimeZone.StandardName;

			try {

				//WORKAROUND for http://stackoverflow.com/questions/3285489/mono-problems-with-cert-and-mozroots
				ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => {
					return true; };

				var httpRequest = (HttpWebRequest)WebRequest.Create (config.RequestUrl);
				httpRequest.Method = "POST";
				httpRequest.ContentType = "application/json; charset=utf-8";
				httpRequest.Accept = "application/json";
						
				httpRequest.Headers.Add ("Authorization", "Bearer " + config.ClientAccessToken);
				httpRequest.Headers.Add ("ocp-apim-subscription-key", config.SubscriptionKey);
						
				var jsonParams = new JSONParameters { 
				UseExtensions = false,
				EnableAnonymousTypes = true,
				SerializeNullValues = false,
				};
			
				var jsonRequest = fastJSON.JSON.ToJSON (request, jsonParams);
				Console.WriteLine ("Request: " + jsonRequest);
				Debug.Log ("Request: " + jsonRequest);

				using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream())) {
					streamWriter.Write (jsonRequest);
					streamWriter.Close ();
				}

				var httpResponse = httpRequest.GetResponse () as HttpWebResponse;
				using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
					var result = streamReader.ReadToEnd ();
					Console.WriteLine ("Result: " + result);
					return fastJSON.JSON.ToObject<AIResponse> (result);
				}

			} catch (Exception e) {
				//Debug.LogException(e);
				Console.WriteLine (e);
			}

			return null;
		}

		public AIResponse VoiceRequest(Stream voiceStream)
		{
			var request = new AIRequest ();
			request.Language = config.Language;
			request.Timezone = TimeZone.CurrentTimeZone.StandardName;

			try {

				//WORKAROUND for http://stackoverflow.com/questions/3285489/mono-problems-with-cert-and-mozroots
				ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => {
					return true; };
				
				var httpRequest = (HttpWebRequest)WebRequest.Create (config.RequestUrl);
				httpRequest.Method = "POST";
				httpRequest.Accept = "application/json";
				
				httpRequest.Headers.Add ("Authorization", "Bearer " + config.ClientAccessToken);
				httpRequest.Headers.Add ("ocp-apim-subscription-key", config.SubscriptionKey);

				var jsonParams = new JSONParameters { 
					UseExtensions = false,
					EnableAnonymousTypes = true,
					SerializeNullValues = false,
				};
				
				var jsonRequest = fastJSON.JSON.ToJSON (request, jsonParams);

				Console.WriteLine ("Request: " + jsonRequest);
				Debug.Log ("Request: " + jsonRequest);

				var multipartClient = new MultipartHttpClient(httpRequest);
				multipartClient.connect();

				multipartClient.addStringPart("request", jsonRequest);
				multipartClient.addFilePart("voiceData", "voice.wav", voiceStream);

				multipartClient.finish();

				var textJson = multipartClient.getResponse();
				return fastJSON.JSON.ToObject<AIResponse>(textJson);

			} catch (Exception e) {
				//Debug.LogException(e);
				Console.WriteLine (e);
			}

			return null;
		}

	}
}
