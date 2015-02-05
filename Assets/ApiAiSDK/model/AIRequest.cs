using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using fastJSON;

namespace ApiAiSDK.model
{
	[Serializable]
	public class AIRequest : QuestionMetadata
	{
		[JsonProperty("query")]
		public string[] Query { get; set; }
	
		[JsonProperty("confidence")]
		public float[] Confidence { get; set; }
	
		[JsonProperty("contexts")]
		public List<String> Contexts { get; set; }
	
		[JsonProperty("resetContexts")]
		public bool ResetContexts { get; set; }

		public AIRequest ()
		{
		}

		public AIRequest (string text)
		{
			Query = new string[] { text };
			Confidence = new float[] { 1.0f };
		}

	}
}
