using UnityEngine;
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

	}
}