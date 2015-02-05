
using System;
using fastJSON;

namespace ApiAiSDK.model
{
	[Serializable]
	public class Result
	{

		[JsonProperty("speech")]
		public String speech{ get; set; }
	
		[JsonProperty("action")]
		public String action{ get; set; }

		[JsonProperty("resolvedQuery")]
		public String resolvedQuery{ get; set; }

		public Result ()
		{
		}
	}
}


