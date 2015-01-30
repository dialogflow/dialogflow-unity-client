using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;
using fastJSON;
using ApiAiSDK;
using ApiAiSDK.model;

public class ApiAiModule : MonoBehaviour {

	public Text AnswerTextField{ get; set; }

	private ApiAi apiAi;

	// Use this for initialization
	void Start () {

		const string SUBSCRIPTION_KEY = "cb9693af-85ce-4fbf-844a-5563722fc27f";
		const string ACCESS_TOKEN = "9586504322be4f8ba31cfdebc40eb76f";

		var config = new AIConfiguration(SUBSCRIPTION_KEY,ACCESS_TOKEN, "en");
		config.DebugMode = true;

		apiAi = new ApiAi(config);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void StartListening(){
		Debug.Log ("StartListening");

		if (AnswerTextField != null) {
			AnswerTextField.text = "Listening...";
		}
	}
	
	public void StopListening(){
		Debug.Log ("StopListening");

		if (AnswerTextField != null) {
			AnswerTextField.text = "";
		}
	}

	public void SendText(string text){
		Debug.Log(text);

		Debug.Log(System.Environment.Version);

		AIResponse response = apiAi.requestText(text);

		if (response != null) {
			Debug.Log(response.Result.resolvedQuery);
			var outText = fastJSON.JSON.ToJSON(response, new JSONParameters { 
				UseExtensions = false,  
				SerializeNullValues = false,
				EnableAnonymousTypes = true
			});

			Debug.Log(outText);

			AnswerTextField.text = outText;

		}
		else{
			Debug.LogError("Response is null");
		}

	}
}
