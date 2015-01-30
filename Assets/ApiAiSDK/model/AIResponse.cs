using UnityEngine;
using System;
using System.Collections;
using fastJSON;

namespace ApiAiSDK.model
{
[Serializable]
public class AIResponse
{
		[JsonProperty("id")]
		public string Id { get; set; }
	
		[JsonProperty("timestamp")]
		public DateTime Timestamp{ get; set; }

		[JsonProperty("result")]
		public Result Result{ get; set; }
	
		[JsonProperty("status")]
		public Status Status{ get; set; }
}

}