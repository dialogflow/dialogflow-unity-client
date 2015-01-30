using System.Collections;
using ApiAiSDK.model;

namespace ApiAiSDK{

public class AIConfiguration {

	private const string SERVICE_PROD_URL = "https://api.api.ai/v1/";
	private const string SERVICE_DEV_URL = "https://dev.api.ai/api/";

	public string SubscriptionKey{ get; private set; }
	public string ClientAccessToken{ get; private set; }

	public string Language{get;set;}

	public bool DebugMode { get; set; }
	
	public AIConfiguration (string subscriptionKey, string clientAccessToken, string language)
	{
		this.SubscriptionKey = subscriptionKey;
		this.ClientAccessToken = clientAccessToken;
		this.Language = language;

		DebugMode = false;
	}

	public string RequestUrl {
		get {
			if (DebugMode) {
				return SERVICE_DEV_URL + "query";
			}
			else {
				return SERVICE_PROD_URL + "query";
			}
		}
	}

}
}