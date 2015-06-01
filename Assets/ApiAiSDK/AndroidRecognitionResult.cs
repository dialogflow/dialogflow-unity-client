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
using Newtonsoft.Json;

namespace ApiAiSDK.Unity.Android
{

#if UNITY_ANDROID
    [JsonObject]
	public class AndroidRecognitionResult
	{
		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("errorMessage")]
		public string ErrorMessage { get; set; }

		[JsonProperty("recognitionResults")]
		public string[] RecognitionResults { get; set; }

		[JsonProperty("confidence")]
		public float[] Confidence{ get; set; }

		public bool IsError {
			get {
				if (Status == "success") {
					return false;
				}

				return true;
			}
		}

		public AndroidRecognitionResult()
		{
		}
	}
#endif

}

