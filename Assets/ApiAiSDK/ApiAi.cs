using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.IO;
using ApiAiSDK.model;

namespace ApiAiSDK
{
	public class ApiAi
	{
		private AIConfiguration config;
		private AIDataService dataService;

		public ApiAi (AIConfiguration config)
		{
			this.config = config;

			dataService = new AIDataService (config);
		}

		public AIResponse textRequest (string text)
		{
			return dataService.Request (new AIRequest (text));
		}

		public AIResponse voiceRequest(Stream voiceStream){
			return dataService.VoiceRequest(voiceStream);
		}

		public AIResponse voiceRequest(float[] samples)
		{
			var trimmedSamples = TrimSilence (samples);
			
			if (trimmedSamples != null) {
				
				var pcm16 = ConvertIeeeToPcm16 (trimmedSamples);
				var bytes = ConvertArrayShortToBytes (pcm16);

				var voiceStream = new MemoryStream (bytes);
				voiceStream.Seek (0, SeekOrigin.Begin);
				
				var aiResponse = voiceRequest (voiceStream);
				return aiResponse;

			}

			return null;
		}


		private float[] TrimSilence(float[] samples)
		{
			if(samples == null){
				return null;
			}

			float min = 0.000001f;
			
			int startIndex = 0;
			int endIndex = samples.Length;
			
			for (int i = 0; i < samples.Length; i++) {
				
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
	}
}