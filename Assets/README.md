# api-ai-unity

The API.AI Unity Plugin makes it easy to integrate the API.AI natural language processing API (http://api.ai) into your Unity project. API.AI allows using voice commands and integration with dialog scenarios defined for a particular agent in API.AI.

Library provides simple programming interface for making text and voice requests to the API.AI service. 

## Getting started

### Create helper module

* Add new script to the Assets folder (ApiAiModule, for example) 
* Your new module should looks like
    ```csharp
    using UnityEngine;
    using System.Collections;

    public class ApiAiModule : MonoBehaviour {

        // Use this for initialization
        void Start () {
        
        }
        
        // Update is called once per frame
        void Update () {
        
        }
    }
    ```

* First, add API.AI usings
    
    ```csharp
    using ApiAiSDK;
    using ApiAiSDK.Model;
    using ApiAiSDK.Unity;
    ```

* Add private field to your module to keep reference to the SDK object

    ```csharp
        private ApiAiUnity apiAiUnity;
    ```

* On the start of your module ApiAiUnity object must be initialized. Required data for initialization is client access token from your development console on the api.ai website and one of supported languages

    ```csharp
    // Use this for initialization
    void Start()
    {
        const string ACCESS_TOKEN = "your_access_token";

        var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);

        apiAiUnity = new ApiAiUnity();
        apiAiUnity.Initialize(config);

        apiAiUnity.OnResult += HandleOnResult;
        apiAiUnity.OnError += HandleOnError;
    }
    ```

* `OnError` and `OnResult` events used for processing service results. So, handling functions must look like

    ```csharp
    void HandleOnResult(object sender, AIResponseEventArgs e)
    {
        var aiResponse = e.Response;
        if (aiResponse != null) {
            // get data from aiResponse
        } else {
            Debug.LogError("Response is null");
        }
    }

    void HandleOnError(object sender, AIErrorEventArgs e)
    {
        Debug.LogException(e.Exception);
    }
    ```

### Usage

ApiAi Unity SDK let you to perform the following actions:
1. Start listening process and then send voice data to the api.ai service for recognition and processing
2. Send simple text request to the api.ai service
3. Use integrated Android recognition engine for recognition and send recognized text to the api.ai service for processing

#### Using Speaktoit recognition

To use Speaktoit voice recognition service you need to provide ApiAiUnity object with valid `AudioSource` object. It can be usually recieved using  `GetComponent<AudioSource>()` function.
Temporary limitation of this case is if you using Speaktoit recognition you need to stop listening manually. So, use this code snippets to start and stop listening.

```csharp
public void StartListening()
{
    try {
        var aud = GetComponent<AudioSource>();
        apiAiUnity.StartListening(aud);
    } catch (Exception ex) {
        Debug.LogException(ex);
    }
}

public void StopListening()
{
    try {
        apiAiUnity.StopListening();
    } catch (Exception ex) {
        Debug.LogException(ex);
    }
}
```

After start/stop listening you will receive api.ai result in the `OnResult` handler.

**Note**: In some cases Unity application must get Sound Recording priviledges for using Microphone. To do this, change your helper module Start function in the following way 

```csharp
IEnumerator Start()
{
    // check access to the Microphone
    yield return Application.RequestUserAuthorization (UserAuthorization.Microphone);
    if (!Application.HasUserAuthorization(UserAuthorization.Microphone)) {
        throw new NotSupportedException ("Microphone using not authorized");
    }

    ... // apiAiUnity initialization...
}
```

#### Simple text requests

Usage of text requests is very simple, all you need is text query.

```csharp
public void SendText()
{
    var text = "hello";
    try {
        var response = apiAiUnity.TextRequest(text);
        if (response != null) {
            // process response
        } else {
            Debug.LogError("Response is null");
        }
    } catch (Exception ex) {
        Debug.LogException(ex);
    }
}
```

**Note**, what you will receive api.ai result immediatly, not in the `OnResult` handler.

#### Using native Android recognition

This case only applicable for the Android Unity applications. You can check if the application is running on the Android platform using this simple code snippet

```csharp
if (Application.platform == RuntimePlatform.Android) {
    // you can use Android recognition here
}
```

Because of native recognition uses Unity-to-Native bridge, you need add following code to the script `Update` method. This code used for checking recognition results from native layer, because of callbacks is not supported in this case.

```csharp
if (apiAiUnity != null) {
    apiAiUnity.Update();
}
```

To start recognition process use simple call of the `StartNativeRecognition` method. 

```csharp
public void StartNativeRecognition(){
    try {
        apiAiUnity.StartNativeRecognition();
    } catch (Exception ex) {
        Debug.LogException (ex);
    }
}
```

You don't need to call `StopListening`, because of Android End-of-Speech detection used in this case.
