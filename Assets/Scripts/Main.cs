using SherpaOnnxUnity;
using System;
using System.Collections.Generic;
using System.IO;
using uMicrophoneWebGL;
using UnityEngine;
using UnityEngine.UI; 

public class Main : MonoBehaviour
{
    private bool isEnh = true;
    GtcrnStream gtcrnStream;
    public MicrophoneWebGL microphoneWebGL;
    public AudioSource audioSource;
    public OnlineHotwords speechRecognition;
    public ModelMatcha speechSynthesis;
    public Text text;

    OllamaSharpUnity ollama;
    Dictionary<int, TtsData> audioDic = new Dictionary<int, TtsData>();
      
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        if (speechRecognition != null)
        {
            speechRecognition.onResult += OnResult;
            speechRecognition.onResultEnd += OnResultEnd;
        }
        ollama = new OllamaSharpUnity("http://localhost:11434", "qwen3:0.6b", OnWord, OnSentence);

        string modelPath = Application.streamingAssetsPath + "/gtcrn/gtcrn_simple.onnx";
        gtcrnStream = new GtcrnStream(modelPath);

        microphoneWebGL = GetComponent<MicrophoneWebGL>();
        microphoneWebGL.dataEvent.AddListener(OnData);
        microphoneWebGL.Begin(256); 
    }

    float[] enhancedOutput = new float[256];
    int count;
    private void OnData(float[] data)
    {
        if (speechRecognition.initDone && speechSynthesis.initDone)
        {
            if (isEnh)
            {
                count = gtcrnStream.ProcessAudio(data, data.Length, out enhancedOutput);
                if (count > 0)
                {
                    if (speechRecognition != null)
                    {
                        speechRecognition.RecognizeOnline(16000, enhancedOutput);
                    }
                }
            }
            else
            {
                if (speechRecognition != null)
                {
                    speechRecognition.RecognizeOnline(16000, data);
                }
            }
        }
    }

    private void OnResult(string result)
    {
        Loom.QueueOnMainThread(() =>
        {
            //Debug.Log(result);
            //text.text = result;
        });
    }

    private void OnResultEnd(string result)
    {
        audioDic.Clear();
        audioEnd = true;
        nowIndex = 0;
        audioIndex = 0;

        ollama.Interrupt();

        Loom.QueueOnMainThread(() =>
        {
            if (audioSource != null &&
           audioSource.clip != null
           && audioSource.isPlaying)
            {
                Destroy(audioSource.clip);
                audioSource.Stop();
            }
            Debug.Log(result);
            text.text = "";
            text.text = result;
        });

        Loom.RunAsync(() =>
        {
            ollama.RequestAsync(result);
        });

        //Task task = new Task(() =>
        //{
        //    ollama.RequestAsync(result);
        //});
        //task.Start();
    }

    private void OnWord(string word)
    {
        //Debug.Log($"{word}");
    }

    public int audioIndex = 0;
    private void OnSentence(string sentence)
    {
        Loom.RunAsync(() =>
        {
            speechSynthesis.Generate(sentence, audioIndex, OnGenerate);
            audioIndex++;
        });
    }

    void OnGenerate(string audioPath, int index, string msg)
    {
        Loom.QueueOnMainThread(() =>
        {
            //Debug.Log("OnSentence:" + sentence);
            TtsData data = new TtsData();
            data.audioPath = audioPath;
            data.content = msg;
            audioDic.Add(index, data);
            PlayTtsData();
        });
    }

    bool audioEnd = false;
    private void Update()
    {
        if (audioSource != null &&
            audioSource.clip != null
            && audioSource.isPlaying
            && audioSource.time >= audioSource.clip.length - 0.1f)
        {
            if (!audioEnd)
            {
                audioEnd = true;
                Debug.Log("当前音频播放完毕");
                if (nowIndex == audioDic.Count)
                {
                    allAudioEnd?.Invoke();
                    Debug.Log("所有音频播放完毕");
                }
                Invoke("PlayTtsData", 0.5f);
            }
        }
    }

    public Action allAudioEnd;
    int nowIndex = 0;

    void PlayTtsData()
    {
        if (audioSource.clip == null || !audioSource.isPlaying)
        {
            if (audioDic.ContainsKey(nowIndex))
            {
                audioEnd = false;
                TtsData data = audioDic[nowIndex];
                nowIndex++;
                if (data.audioPath == null)
                {
                    nowIndex++;
                    PlayTtsData();
                }
                else
                {
                    text.text = data.content;
                    audioSource.clip = GetAudioClip(data.audioPath);
                    audioSource.Play();
                }
            }
        }
    }

    AudioClip GetAudioClip(string audioPath)
    {
        byte[] bytes = File.ReadAllBytes(audioPath);
        byte[] result = new byte[bytes.Length - 44];
        Buffer.BlockCopy(bytes, 44, result, 0, bytes.Length - 44);
        float[] data = Util.BytesToFloat(result);
        AudioClip audioClip = AudioClip.Create("tts", data.Length, 1, speechSynthesis.sampleRate, false);
        audioClip.SetData(data, 0);
        return audioClip;
    }

    private void OnApplicationQuit()
    {

    }

    private void OnDestroy()
    { 
        if (ollama != null)
        {
            ollama.Stop();
        }
        if (gtcrnStream != null)
        {
            gtcrnStream.Dispose();
        }
    }
}

public struct TtsData
{
    public string content;
    public string audioPath;
}