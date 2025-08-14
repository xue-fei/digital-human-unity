using SherpaOnnxUnity;
using uMicrophoneWebGL;
using UnityEngine;

public class Main : MonoBehaviour
{
    public SpeechRecognition speechRecognition;
    MicrophoneWebGL microphoneWebGL;

    // Start is called before the first frame update
    void Start()
    {
        microphoneWebGL = GetComponent<MicrophoneWebGL>();
        microphoneWebGL.dataEvent.AddListener(OnData);
        if (speechRecognition != null)
        {
            speechRecognition.onResult += OnResult;
            speechRecognition.onResultEnd += OnResultEnd;
        }
    }

    void OnData(float[] data)
    {
        if (speechRecognition != null)
        {
            speechRecognition.RecognizeOnline(16000, data);
        }
    }

    public void OnResult(string result)
    {
        Debug.Log(result);
    }

    public void OnResultEnd(string result)
    {
        Debug.Log(result);
    }
}