using SherpaOnnxUnity;
using uMicrophoneWebGL;
using UnityEngine;

public class Main : MonoBehaviour
{
    public SpeechRecognition speechRecognition;
    MicrophoneWebGL microphoneWebGL;
    OllamaSharpUnity ollama;

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
        ollama = new OllamaSharpUnity("http://localhost:11434", "qwen2.5:1.5b", OnWord, OnSentence);
    }

    private void OnData(float[] data)
    {
        if (speechRecognition != null)
        {
            speechRecognition.RecognizeOnline(16000, data);
        }
    }

    private void OnResult(string result)
    {
        Debug.Log(result);
    }

    private void OnResultEnd(string result)
    {
        Debug.Log(result);
        ollama.RequestAsync(result);
    }

    private void OnWord(string word)
    {
        Debug.Log($"{word}");
    }

    private void OnSentence(string sentence)
    {
        Debug.Log(sentence);
    }

    private void OnDestroy()
    {
        if (ollama != null)
        {
            ollama.Stop();
        }
    }
}