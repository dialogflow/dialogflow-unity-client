using UnityEngine;
using System.Collections;
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

		public AIResponse requestText (string text)
		{
			return dataService.Request (new AIRequest (text));
		}

	}
}