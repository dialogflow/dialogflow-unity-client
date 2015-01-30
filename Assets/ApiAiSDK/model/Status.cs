
using System;
using fastJSON;

namespace ApiAiSDK.model
{
[Serializable]
public class Status
{

		[JsonProperty("code")]
		public int code{ get; set; }
	
		[JsonProperty("errorType")]
		public string errorType{ get; set; }
	
		[JsonProperty("errorDetails")]
		public string errorDetails{ get; set; }
	
		[JsonProperty("errorID")]
		public string errorID{ get; set; }

		public Status ()
		{
		}
}
}

