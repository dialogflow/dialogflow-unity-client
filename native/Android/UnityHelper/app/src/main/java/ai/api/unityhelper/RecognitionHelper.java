/**
 * Copyright 2017 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package ai.api.unityhelper;

import android.annotation.TargetApi;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.speech.RecognitionListener;
import android.speech.RecognizerIntent;
import android.speech.SpeechRecognizer;
import android.util.Log;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;


public class RecognitionHelper {

    private static final String TAG = RecognitionHelper.class.getName();

    private SpeechRecognizer speechRecognizer;
    private final Object speechRecognizerLock = new Object();
    private volatile boolean recognitionActive = false;
    private Context context;
    private Handler handler;

    private RecognitionResultObject resultObject;

    private final Map<Integer, String> errorMessages = new HashMap<Integer, String>();

    {
        errorMessages.put(1, "Network operation timed out.");
        errorMessages.put(2, "Other network related errors.");
        errorMessages.put(3, "Audio recording error.");
        errorMessages.put(4, "Server sends error status.");
        errorMessages.put(5, "Other client side errors.");
        errorMessages.put(6, "No speech input.");
        errorMessages.put(7, "No recognition result matched.");
        errorMessages.put(8, "RecognitionService busy.");
        errorMessages.put(9, "Insufficient permissions.");
    }

    public RecognitionHelper() {
    }

    public void initialize(Context context){
        this.context = context;
        handler = new Handler(context.getMainLooper());
    }

    public RecognitionResultObject recognize(final String lang) {
        try {
            if (!recognitionActive) {
                resultObject = new RecognitionResultObject();
                startListening(lang);
            }

            return resultObject;
        } catch (Exception e) {

            resultObject = null;

            JSONObject result = new JSONObject();

            try {
                result.put("status", "error");
                result.put("errorMessage", e.getMessage());
            } catch (JSONException jsonEx) {
                Log.e(TAG, jsonEx.getMessage(), jsonEx);
            }
            RecognitionResultObject errorResultObject = new RecognitionResultObject();
            errorResultObject.setResult(result.toString());
            return errorResultObject;
        }

    }

    public void clean(){
        resultObject = null;
    }

    /**
     *
     * @param lang recognition language
     */
    private void startListening(final String lang) {
        if (!recognitionActive) {

            final Intent sttIntent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
            sttIntent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL,
                    RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);

            final String language = lang.replace('-', '_');

            sttIntent.putExtra(RecognizerIntent.EXTRA_LANGUAGE, language);
            sttIntent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_PREFERENCE, language);

            // WORKAROUND for https://code.google.com/p/android/issues/detail?id=75347
            // TODO Must be removed after fix in Android
            sttIntent.putExtra("android.speech.extra.EXTRA_ADDITIONAL_LANGUAGES", new String[]{});

            runInUiThread(new Runnable() {
                @Override
                public void run() {
                    initializeRecognizer();

                    speechRecognizer.startListening(sttIntent);
                    recognitionActive = true;
                }
            });

        } else {
            Log.w(TAG, "Trying to start recognition while another recognition active");
        }
    }

    protected void onError(final String errorMessage) {
        JSONObject resultJson = new JSONObject();

        try {
            resultJson.put("status", "error");
            resultJson.put("errorMessage", errorMessage);
        } catch (JSONException je) {
            Log.e(TAG, je.getMessage(), je);
        }

        String resultJsonString = resultJson.toString();
        if (resultObject != null) {
            resultObject.setResult(resultJsonString);
        }
    }

    protected void onResults(final Bundle results) {

        JSONObject resultJson = new JSONObject();

        try {

            resultJson.put("status", "success");

            final ArrayList<String> recognitionResults = results
                    .getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION);

            if (recognitionResults == null || recognitionResults.size() == 0) {
                resultJson.put("recognitionResults", new JSONArray());
            } else {
                resultJson.put("recognitionResults", new JSONArray(recognitionResults));

                float[] rates = null;

                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.ICE_CREAM_SANDWICH) {
                    rates = results.getFloatArray(SpeechRecognizer.CONFIDENCE_SCORES);
                    if (rates != null && rates.length > 0) {
                        final JSONArray ratesArray = new JSONArray();
                        for (int i = 0; i < rates.length; i++) {
                            ratesArray.put(rates[i]);
                        }
                        resultJson.put("confidence", ratesArray);
                    }
                }
            }


        } catch (JSONException je) {
            Log.e(TAG, je.getMessage(), je);
        }

        clearRecognizer();
        String resultJsonString = resultJson.toString();
        if (resultObject != null) {
            resultObject.setResult(resultJsonString);
        }
    }

    protected void initializeRecognizer() {
        synchronized (speechRecognizerLock) {
            if (speechRecognizer != null) {
                speechRecognizer.destroy();
                speechRecognizer = null;
            }

            final ComponentName googleRecognizerComponent = RecognizerChecker.findGoogleRecognizer(context);

            if (googleRecognizerComponent == null) {
                speechRecognizer = SpeechRecognizer.createSpeechRecognizer(context);
            } else {
                speechRecognizer = SpeechRecognizer.createSpeechRecognizer(context, googleRecognizerComponent);
            }

            speechRecognizer.setRecognitionListener(new InternalRecognitionListener());
        }
    }

    protected void clearRecognizer() {
        if (speechRecognizer != null) {
            synchronized (speechRecognizerLock) {
                if (speechRecognizer != null) {
                    speechRecognizer.destroy();
                    speechRecognizer = null;
                }
            }
        }
    }

    private void runInUiThread(final Runnable runnable) {
        handler.post(runnable);
    }

    private class InternalRecognitionListener implements RecognitionListener {

        @Override
        public void onReadyForSpeech(final Bundle params) {

        }

        @Override
        public void onBeginningOfSpeech() {

        }

        @Override
        public void onRmsChanged(final float rmsdB) {

        }

        @Override
        public void onBufferReceived(final byte[] buffer) {

        }

        @Override
        public void onEndOfSpeech() {

        }

        @Override
        public void onError(final int error) {
            recognitionActive = false;

            String aiError;
            if (errorMessages.containsKey(error)) {
                final String description = errorMessages.get(error);
                aiError = "Speech recognition engine error: " + description;
            } else {
                aiError = "Speech recognition engine error: " + error;
            }
            RecognitionHelper.this.onError(aiError);
        }

        @TargetApi(14)
        @Override
        public void onResults(final Bundle results) {
            if (recognitionActive) {
                recognitionActive = false;

                RecognitionHelper.this.onResults(results);
            }
        }

        @Override
        public void onPartialResults(final Bundle partialResults) {

        }

        @Override
        public void onEvent(final int eventType, final Bundle params) {

        }
    }

}
