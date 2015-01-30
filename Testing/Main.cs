using System;
using ApiAiSDK;
using ApiAiSDK.model;
using fastJSON;

namespace Testing
{
		class MainClass
		{
				public static void Main (string[] args)
				{
					Console.WriteLine(System.Environment.Version);
								
					const string SUBSCRIPTION_KEY = "cb9693af-85ce-4fbf-844a-5563722fc27f";
					const string ACCESS_TOKEN = "9586504322be4f8ba31cfdebc40eb76f";
					
					var config = new AIConfiguration(SUBSCRIPTION_KEY,ACCESS_TOKEN, "en");
					config.DebugMode = true;
					
					var apiAi = new ApiAi(config);

					AIResponse response = apiAi.requestText("hello");
					
					if (response != null) {
						Console.WriteLine(response.Result.resolvedQuery);
						Console.WriteLine(fastJSON.JSON.ToJSON(response, new JSONParameters { 
							UseExtensions = false
						}));
					}
					else{
						Console.WriteLine("Response is null");
					}
				}
		}
}
