package ai.api.unityhelper;

public class RecognitionResultObject {

    private boolean ready;
    private String result;

    public boolean isReady() {
        return ready;
    }

    public void setReady(boolean ready) {
        this.ready = ready;
    }

    public String getResult() {
        return result;
    }

    public void setResult(String result) {
        this.result = result;

        setReady(true);
    }
}
