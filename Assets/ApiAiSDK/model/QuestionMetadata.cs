using UnityEngine;
using System;
using System.Collections;
using fastJSON;

namespace ApiAiSDK.model
{
	[Serializable]
	public class QuestionMetadata
	{
		[JsonProperty("timezone")]
		public string Timezone { get; set; }

		[JsonProperty("lang")]
		public string Language { get; set; }

		[JsonProperty("sessionId")]
		public string SessionId { get; set; }
	}
}
