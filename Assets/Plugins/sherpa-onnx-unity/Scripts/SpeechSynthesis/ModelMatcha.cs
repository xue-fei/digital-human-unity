using SherpaOnnx;
using System;
using System.IO;
using UnityEngine;

namespace SherpaOnnxUnity
{
    public class ModelMatcha : SpeechSynthesis
    {
        OfflineTts ot;
        OfflineTtsGeneratedAudio otga;
        OfflineTtsConfig config;
        public bool initDone = false;
        public Action OnAudioEnd;
        public int sampleRate;
        string pathRoot;
        string tempAudioPath;

        // Start is called before the first frame update
        void Start()
        {
            tempAudioPath = Application.persistentDataPath + "/temp";
            if (!Directory.Exists(tempAudioPath))
            {
                Directory.CreateDirectory(tempAudioPath);
            }
            pathRoot = Util.GetPath() + "/tts";
            Loom.RunAsync(() =>
            {
                Init();
            });
        }

        void Init()
        {
            initDone = false;
            config = new OfflineTtsConfig();
            config.Model.Matcha.AcousticModel = Path.Combine(pathRoot, "matcha-icefall-zh-baker/model-steps-3.onnx");
            config.Model.Matcha.Vocoder = Path.Combine(pathRoot, "matcha-icefall-zh-baker/vocos-22khz-univ.onnx");
            config.Model.Matcha.Tokens = Path.Combine(pathRoot, "matcha-icefall-zh-baker/tokens.txt");
            config.Model.Matcha.DictDir = Path.Combine(pathRoot, "matcha-icefall-zh-baker/dict");
            config.Model.Matcha.Lexicon = Path.Combine(pathRoot, "matcha-icefall-zh-baker/lexicon.txt");
            config.Model.Matcha.LengthScale = 1f;
            config.Model.NumThreads = 4;
            config.Model.Debug = 1;
            config.Model.Provider = "cpu";
            config.RuleFsts = pathRoot + "/matcha-icefall-zh-baker/phone.fst" + ","
                        + pathRoot + "/matcha-icefall-zh-baker/date.fst" + ","
                    + pathRoot + "/matcha-icefall-zh-baker/number.fst";
            config.MaxNumSentences = 1;
            ot = new OfflineTts(config);
            sampleRate = ot.SampleRate;
            initDone = true;
            Loom.QueueOnMainThread(() =>
            {
                Debug.Log("文字转语音初始化完成 sampleRate:" + sampleRate);
            });
        }

        Action<string, int, string> callback; 
        public void Generate(string text, int index, Action<string, int, string> callback)
        {
            this.callback = callback; 
            Generate(text, index);
        }

        public override void Generate(string text, int index, float speed = 1, int speakerId = 0)
        {
            if (!initDone)
            {
                Debug.LogWarning("文字转语音未完成初始化");
                return;
            }
            Loom.RunAsync(() =>
            {
                otga = ot.Generate(text, speed, speakerId);
                string filePath = tempAudioPath + "/" + DateTime.Now.ToFileTime() + ".wav";
                if (otga.SaveToWaveFile(filePath))
                {
                    if (callback != null)
                    {
                        callback(filePath, index, text);
                    }
                }
            });
        }

        private void OnDestory()
        {
            if (ot != null)
            {
                ot.Dispose();
            }
            if (otga != null)
            {
                otga.Dispose();
            }
        }
    }
}