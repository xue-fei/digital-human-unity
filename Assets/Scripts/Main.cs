using SherpaOnnxUnity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using uMicrophoneWebGL;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    DtlnaecProcessor dtlnaecProcessor;
    public MicrophoneWebGL microphoneWebGL;
    public AudioSource audioSource;
    public OnlineHotwords speechRecognition;
    public ModelMatcha speechSynthesis;
    public Text text;

    OllamaSharpUnity ollama;
    Dictionary<int, TtsData> audioDic = new Dictionary<int, TtsData>();

    static bool isPlay = false;

    List<float> mic = new List<float>();
    List<float> lpb = new List<float>();
    List<float> output = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        // 设置音频 2声道 16000 Best latency
        AudioConfiguration config = AudioSettings.GetConfiguration();
        config.sampleRate = 16000;
        config.speakerMode = AudioSpeakerMode.Stereo;
        config.dspBufferSize = 256;
        AudioSettings.Reset(config);

        if (speechRecognition != null)
        {
            speechRecognition.onResult += OnResult;
            speechRecognition.onResultEnd += OnResultEnd;
        }
        ollama = new OllamaSharpUnity("http://localhost:11434", "qwen3:0.6b", OnWord, OnSentence);

        dtlnaecProcessor = new DtlnaecProcessor();
        dtlnaecProcessor.Initialize(Application.streamingAssetsPath + "/dtlnaec/dtln_aec_128_1.onnx",
            Application.streamingAssetsPath + "/dtlnaec/dtln_aec_128_2.onnx");

        microphoneWebGL = GetComponent<MicrophoneWebGL>();
        microphoneWebGL.dataEvent.AddListener(OnData);
        microphoneWebGL.Begin(128);

        isPlay = true;
    }

    float[] temp = new float[128];
    float[] processedFrame;
    private void OnData(float[] data)
    {
        if (speechRecognition.initDone && speechSynthesis.initDone)
        {
#if UNITY_EDITOR
            mic.AddRange(data);
#endif
            if (farQueue.Count >= 128)
            {
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i] = farQueue.Dequeue();
                }
            }
            processedFrame = dtlnaecProcessor.ProcessFrame(data, temp);
#if UNITY_EDITOR
            output.AddRange(processedFrame);
#endif

            if (speechRecognition != null)
            {
                speechRecognition.RecognizeOnline(16000, processedFrame);
            }
        }
    }

    Queue<float> farQueue = new Queue<float>();
    float[] tempData = new float[256];

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (isPlay)
        {
            if (data.Length != 512)
            {
                Debug.LogWarning(data.Length);
                return;
            }
            if (channels == 1)
            {
                tempData = data;
            }
            if (channels == 2)
            {
                for (int i = 0; i < tempData.Length; i++)
                {
                    tempData[i] = data[i * 2];
                }
            }
#if UNITY_EDITOR
            lpb.AddRange(tempData);
#endif
            for (int i = 0; i < tempData.Length; i++)
            {
                tempData[i] = tempData[i] * 0.25f;
                farQueue.Enqueue(tempData[i]);
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
        isPlay = false;
        if (ollama != null)
        {
            ollama.Stop();
        }
        if (dtlnaecProcessor != null)
        {
#if UNITY_EDITOR
            float[] end = dtlnaecProcessor.Flush();
            output.AddRange(end);
            Util.SaveClip(1, 16000, output.ToArray(), Application.dataPath + "/output.wav");
            Util.SaveClip(1, 16000, mic.ToArray(), Application.dataPath + "/mic.wav");
            Util.SaveClip(1, 16000, lpb.ToArray(), Application.dataPath + "/lpb.wav");
#endif
            dtlnaecProcessor.Dispose();
        }
    }
}

public struct TtsData
{
    public string content;
    public string audioPath;
}