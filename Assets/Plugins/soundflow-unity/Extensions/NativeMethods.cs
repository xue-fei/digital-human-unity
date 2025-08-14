using System;
using System.Runtime.InteropServices;

namespace SoundFlow.Extensions.WebRtc.Apm
{
    public static class NativeMethods
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private const string LibraryName = "webrtc-apm";
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        private const string LibraryName = "webrtc-apm.so";
#elif UNITY_ANDROID && !UNITY_EDITOR
        private const string LibraryName = "webrtc-apm.so";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        private const string LibraryName = "webrtc-apm";
#endif 
        [DllImport(LibraryName, EntryPoint = "webrtc_apm_create")]
        internal static extern IntPtr Create();

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_destroy")]
        public static extern void Destroy(IntPtr apm);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_config_create")]
        public static extern IntPtr ConfigCreate();

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_config_destroy")]
        public static extern void ConfigDestroy(IntPtr config);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_config_set_echo_canceller")]
        public static extern void ConfigSetEchoCanceller(IntPtr config, int enabled, int mobileMode);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_config_set_noise_suppression")]
        public static extern void ConfigSetNoiseSuppression(IntPtr config, int enabled,
            NoiseSuppressionLevel level);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_config_set_gain_controller1")]
        public static extern void ConfigSetGainController1(IntPtr config, int enabled, GainControlMode mode,
            int targetLevelDbfs, int compressionGainDb, int enableLimiter);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_config_set_gain_controller2")]
        public static extern void ConfigSetGainController2(IntPtr config, int enabled);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_config_set_high_pass_filter")]
        public static extern void ConfigSetHighPassFilter(IntPtr config, int enabled);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_config_set_pre_amplifier")]
        public static extern void webrtc_apm_config_set_pre_amplifier(IntPtr config, int enabled, float fixedGainFactor);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_config_set_pipeline")]
        public static extern void ConfigSetPipeline(IntPtr config, int maxInternalRate,
            int multiChannelRender, int multiChannelCapture, DownmixMethod downmixMethod);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_apply_config")]
        public static extern ApmError ConfigApply(IntPtr apm, IntPtr config);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_stream_config_create")]
        public static extern IntPtr StreamConfigCreate(int sampleRateHz, UIntPtr numChannels);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_stream_config_destroy")]
        public static extern void StreamConfigDestroy(IntPtr config);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_stream_config_sample_rate_hz")]
        public static extern int StreamConfigSetSampleRate(IntPtr config);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_stream_config_num_channels")]
        public static extern UIntPtr StreamConfigSetNumChannels(IntPtr config);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_processing_config_create")]
        public static extern IntPtr ProcessingConfigCreate();

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_processing_config_destroy")]
        public static extern void ProcessingConfigDestroy(IntPtr config);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_processing_config_input_stream")]
        public static extern IntPtr ProcessingConfigInputStream(IntPtr config);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_processing_config_output_stream")]
        public static extern IntPtr ProcessingConfigOutputStream(IntPtr config);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_processing_config_reverse_input_stream")]
        public static extern IntPtr ProcessingConfigReverseInputStream(IntPtr config);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_processing_config_reverse_output_stream")]
        public static extern IntPtr ProcessingConfigReverseOutputStream(IntPtr config);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_initialize")]
        public static extern ApmError Initialize(IntPtr apm);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_initialize_with_config")]
        public static extern ApmError InitializeWithConfig(IntPtr apm, IntPtr config);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_process_stream")]
        public static extern ApmError ProcessStream(IntPtr apm, IntPtr src, IntPtr inputConfig,
            IntPtr outputConfig, IntPtr dest);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_process_reverse_stream")]
        public static extern ApmError ProcessReverseStream(IntPtr apm, IntPtr src, IntPtr inputConfig,
            IntPtr outputConfig, IntPtr dest);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_analyze_reverse_stream")]
        public static extern ApmError AnalyzeReverseStream(IntPtr apm, IntPtr data, IntPtr reverseConfig);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_set_stream_analog_level")]
        public static extern void SetStreamAnalogLevel(IntPtr apm, int level);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_recommended_stream_analog_level")]
        public static extern int GetRecommendedStreamAnalogLevel(IntPtr apm);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_set_stream_delay_ms")]
        public static extern void SetStreamDelayMs(IntPtr apm, int delay);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_stream_delay_ms")]
        public static extern int GetStreamDelayMs(IntPtr apm);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_set_runtime_setting_float")]
        public static extern void SetRuntimeSettingFloat(IntPtr apm, RuntimeSettingType type, float value);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_set_runtime_setting_int")]
        public static extern void SetRuntimeSettingInt(IntPtr apm, RuntimeSettingType type, int value);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_proc_sample_rate_hz")]
        public static extern int GetProcSampleRateHz(IntPtr apm);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_proc_split_sample_rate_hz")]
        public static extern int GetProcSplitSampleRateHz(IntPtr apm);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_num_input_channels")]
        public static extern UIntPtr GetInputChannelsNum(IntPtr apm);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_num_proc_channels")]
        public static extern UIntPtr GetProcChannelsNum(IntPtr apm);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_num_output_channels")]
        public static extern UIntPtr GetOutputChannelsNum(IntPtr apm);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_num_reverse_channels")]
        public static extern UIntPtr GetReverseChannelsNum(IntPtr apm);

        [DllImport(LibraryName, EntryPoint = "webrtc_apm_get_frame_size")]
        public static extern UIntPtr GetFrameSize(int sampleRateHz);
    }
}