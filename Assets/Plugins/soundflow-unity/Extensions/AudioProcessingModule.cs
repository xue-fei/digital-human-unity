using System;
using System.Runtime.InteropServices;

namespace SoundFlow.Extensions.WebRtc.Apm
{
    /// <summary>
    /// Error codes returned by the WebRTC Audio Processing Module
    /// </summary>
    public enum ApmError
    {
        NoError = 0,
        UnspecifiedError = -1,
        CreationFailed = -2,
        BadParameter = -6,
        BadSampleRate = -7,
        BadDataLength = -8,
        BadNumChannels = -9
    }

    /// <summary>
    /// Noise suppression levels
    /// </summary>
    public enum NoiseSuppressionLevel
    {
        Low,
        Moderate,
        High,
        VeryHigh
    }

    /// <summary>
    /// Gain controller modes
    /// </summary>
    public enum GainControlMode
    {
        AdaptiveAnalog,
        AdaptiveDigital,
        FixedDigital
    }

    /// <summary>
    /// Downmix methods
    /// </summary>
    public enum DownmixMethod
    {
        AverageChannels,
        UseFirstChannel
    }

    /// <summary>
    /// Runtime setting types
    /// </summary>
    public enum RuntimeSettingType
    {
        CapturePreGain,
        CaptureCompressionGain,
        CaptureFixedPostGain,
        PlayoutVolumeChange,
        CustomRenderSetting,
        PlayoutAudioDeviceChange,
        CapturePostGain,
        CaptureOutputUsed
    }

    /// <summary>
    /// Represents a stream configuration for audio processing
    /// </summary>
    public sealed class StreamConfig : IDisposable
    {
        private IntPtr _nativeConfig;

        /// <summary>
        /// Creates a new stream configuration
        /// </summary>
        /// <param name="sampleRateHz">Sample rate in Hz</param>
        /// <param name="numChannels">Number of channels</param>
        public StreamConfig(int sampleRateHz, int numChannels)
        {
            _nativeConfig = NativeMethods.StreamConfigCreate(sampleRateHz, (UIntPtr)numChannels);
            if (_nativeConfig == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create stream config");
        }

        /// <summary>
        /// Sample rate in Hz
        /// </summary>
        public int SampleRateHz => NativeMethods.StreamConfigSetSampleRate(_nativeConfig);

        /// <summary>
        /// Number of channels
        /// </summary>
        public int NumChannels => (int)NativeMethods.StreamConfigSetNumChannels(_nativeConfig);

        internal IntPtr NativePtr => _nativeConfig;

        #region IDisposable Support

        private bool _disposedValue;

        /// <summary>
        /// Disposes managed and unmanaged resources
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (_nativeConfig != IntPtr.Zero)
                {
                    NativeMethods.StreamConfigDestroy(_nativeConfig);
                    _nativeConfig = IntPtr.Zero;
                }

                _disposedValue = true;
            }
        }

        ~StreamConfig()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    /// Represents a processing configuration for audio processing
    /// </summary>
    public class ProcessingConfig : IDisposable
    {
        private IntPtr _nativeConfig;

        /// <summary>
        /// Creates a new processing configuration
        /// </summary>
        public ProcessingConfig()
        {
            _nativeConfig = NativeMethods.ProcessingConfigCreate();
            if (_nativeConfig == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create processing config");
        }

        /// <summary>
        /// Input stream configuration
        /// </summary>
        public StreamConfig InputStream
        {
            get
            {
                var ptr = NativeMethods.ProcessingConfigInputStream(_nativeConfig);
                return new StreamConfig(
                    NativeMethods.StreamConfigSetSampleRate(ptr),
                    (int)NativeMethods.StreamConfigSetNumChannels(ptr));
            }
        }

        /// <summary>
        /// Output stream configuration
        /// </summary>
        public StreamConfig OutputStream
        {
            get
            {
                var ptr = NativeMethods.ProcessingConfigOutputStream(_nativeConfig);
                return new StreamConfig(
                    NativeMethods.StreamConfigSetSampleRate(ptr),
                    (int)NativeMethods.StreamConfigSetNumChannels(ptr));
            }
        }

        /// <summary>
        /// Reverse input stream configuration
        /// </summary>
        public StreamConfig ReverseInputStream
        {
            get
            {
                var ptr = NativeMethods.ProcessingConfigReverseInputStream(_nativeConfig);
                return new StreamConfig(
                    NativeMethods.StreamConfigSetSampleRate(ptr),
                    (int)NativeMethods.StreamConfigSetNumChannels(ptr));
            }
        }

        /// <summary>
        /// Reverse output stream configuration
        /// </summary>
        public StreamConfig ReverseOutputStream
        {
            get
            {
                var ptr = NativeMethods.ProcessingConfigReverseOutputStream(_nativeConfig);
                return new StreamConfig(
                    NativeMethods.StreamConfigSetSampleRate(ptr),
                    (int)NativeMethods.StreamConfigSetNumChannels(ptr));
            }
        }

        internal IntPtr NativePtr => _nativeConfig;

        #region IDisposable Support

        private bool _disposedValue;

        /// <summary>
        /// Disposes managed and unmanaged resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (_nativeConfig != IntPtr.Zero)
                {
                    NativeMethods.ProcessingConfigDestroy(_nativeConfig);
                    _nativeConfig = IntPtr.Zero;
                }

                _disposedValue = true;
            }
        }

        ~ProcessingConfig()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    /// Represents an APM configuration
    /// </summary>
    public sealed class ApmConfig : IDisposable
    {
        private IntPtr _nativeConfig;

        /// <summary>
        /// Creates a new APM configuration
        /// </summary>
        public ApmConfig()
        {
            _nativeConfig = NativeMethods.ConfigCreate();
            if (_nativeConfig == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create APM config");
        }

        /// <summary>
        /// Configures the echo canceller
        /// </summary>
        /// <param name="enabled">Whether echo cancellation is enabled</param>
        /// <param name="mobileMode">Whether to use mobile mode</param>
        public void SetEchoCanceller(bool enabled, bool mobileMode)
        {
            NativeMethods.ConfigSetEchoCanceller(_nativeConfig, enabled ? 1 : 0, mobileMode ? 1 : 0);
        }

        /// <summary>
        /// Configures noise suppression
        /// </summary>
        /// <param name="enabled">Whether noise suppression is enabled</param>
        /// <param name="level">Noise suppression level</param>
        public void SetNoiseSuppression(bool enabled, NoiseSuppressionLevel level)
        {
            NativeMethods.ConfigSetNoiseSuppression(_nativeConfig, enabled ? 1 : 0, level);
        }

        /// <summary>
        /// Configures gain controller 1
        /// </summary>
        /// <param name="enabled">Whether gain controller is enabled</param>
        /// <param name="mode">Gain control mode</param>
        /// <param name="targetLevelDbfs">Target level in dBFS</param>
        /// <param name="compressionGainDb">Compression gain in dB</param>
        /// <param name="enableLimiter">Whether to enable the limiter</param>
        public void SetGainController1(bool enabled, GainControlMode mode, int targetLevelDbfs, int compressionGainDb,
            bool enableLimiter)
        {
            NativeMethods.ConfigSetGainController1(
                _nativeConfig,
                enabled ? 1 : 0,
                mode,
                targetLevelDbfs,
                compressionGainDb,
                enableLimiter ? 1 : 0);
        }

        /// <summary>
        /// Configures gain controller 2
        /// </summary>
        /// <param name="enabled">Whether gain controller 2 is enabled</param>
        public void SetGainController2(bool enabled)
        {
            NativeMethods.ConfigSetGainController2(_nativeConfig, enabled ? 1 : 0);
        }

        /// <summary>
        /// Configures the high pass filter
        /// </summary>
        /// <param name="enabled">Whether the high pass filter is enabled</param>
        public void SetHighPassFilter(bool enabled)
        {
            NativeMethods.ConfigSetHighPassFilter(_nativeConfig, enabled ? 1 : 0);
        }

        /// <summary>
        /// Configures the pre-amplifier
        /// </summary>
        /// <param name="enabled">Whether the pre-amplifier is enabled</param>
        /// <param name="fixedGainFactor">Fixed gain factor</param>
        public void SetPreAmplifier(bool enabled, float fixedGainFactor)
        {
            NativeMethods.webrtc_apm_config_set_pre_amplifier(_nativeConfig, enabled ? 1 : 0, fixedGainFactor);
        }

        /// <summary>
        /// Configures the processing pipeline
        /// </summary>
        /// <param name="maxInternalRate">Maximum internal processing rate</param>
        /// <param name="multiChannelRender">Whether to enable multichannel render</param>
        /// <param name="multiChannelCapture">Whether to enable multichannel capture</param>
        /// <param name="downmixMethod">Downmix method</param>
        public void SetPipeline(int maxInternalRate, bool multiChannelRender, bool multiChannelCapture,
            DownmixMethod downmixMethod)
        {
            NativeMethods.ConfigSetPipeline(
                _nativeConfig,
                maxInternalRate,
                multiChannelRender ? 1 : 0,
                multiChannelCapture ? 1 : 0,
                downmixMethod);
        }

        internal IntPtr NativePtr => _nativeConfig;

        #region IDisposable Support

        private bool _disposedValue;

        /// <summary>
        /// Disposes managed and unmanaged resources
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (_nativeConfig != IntPtr.Zero)
                {
                    NativeMethods.ConfigDestroy(_nativeConfig);
                    _nativeConfig = IntPtr.Zero;
                }

                _disposedValue = true;
            }
        }

        ~ApmConfig()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    /// Provides access to the WebRTC Audio Processing Module
    /// </summary>
    public sealed class AudioProcessingModule : IDisposable
    {
        /// <summary>
        /// The native APM instance
        /// </summary>
        public IntPtr NativePtr;

        /// <summary>
        /// Creates a new Audio Processing Module instance
        /// </summary>
        public AudioProcessingModule()
        {
            NativePtr = NativeMethods.Create();
            if (NativePtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create APM instance");
        }

        /// <summary>
        /// Applies configuration to the APM
        /// </summary>
        /// <param name="config">Configuration to apply</param>
        /// <returns>Error code</returns>
        public ApmError ApplyConfig(ApmConfig config)
        {
            //ArgumentNullException.ThrowIfNull(config);

            return NativeMethods.ConfigApply(NativePtr, config.NativePtr);
        }

        /// <summary>
        /// Initializes the APM with default settings
        /// </summary>
        /// <returns>Error code</returns>
        public ApmError Initialize()
        {
            return NativeMethods.Initialize(NativePtr);
        }

        /// <summary>
        /// Processes a stream of audio data
        /// </summary>
        /// <param name="src">Source audio data (array of channels)</param>
        /// <param name="inputConfig">Input stream configuration</param>
        /// <param name="outputConfig">Output stream configuration</param>
        /// <param name="dest">Destination audio data (array of channels)</param>
        /// <returns>Error code</returns>
        public ApmError ProcessStream(float[][] src, StreamConfig inputConfig, StreamConfig outputConfig, float[][] dest)
        {
            if (src == null || inputConfig == null || outputConfig == null || dest == null)
                throw new ArgumentNullException();

            // Convert float[][] to IntPtr[] for interop
            var srcPtrs = new IntPtr[src.Length];
            var destPtrs = new IntPtr[dest.Length];

            try
            {
                for (int i = 0; i < src.Length; i++)
                {
                    srcPtrs[i] = Marshal.AllocHGlobal(src[i].Length * sizeof(float));
                    Marshal.Copy(src[i], 0, srcPtrs[i], src[i].Length);
                }

                for (int i = 0; i < dest.Length; i++)
                {
                    destPtrs[i] = Marshal.AllocHGlobal(dest[i].Length * sizeof(float));
                }

                // Pin the arrays
                GCHandle srcHandle = GCHandle.Alloc(srcPtrs, GCHandleType.Pinned);
                GCHandle destHandle = GCHandle.Alloc(destPtrs, GCHandleType.Pinned);

                try
                {
                    var error = NativeMethods.ProcessStream(
                        NativePtr,
                        srcHandle.AddrOfPinnedObject(),
                        inputConfig.NativePtr,
                        outputConfig.NativePtr,
                        destHandle.AddrOfPinnedObject());

                    // Copy results back
                    for (int i = 0; i < dest.Length; i++)
                    {
                        Marshal.Copy(destPtrs[i], dest[i], 0, dest[i].Length);
                    }

                    return error;
                }
                finally
                {
                    srcHandle.Free();
                    destHandle.Free();
                }
            }
            finally
            {
                // Free allocated memory
                for (var i = 0; i < srcPtrs.Length; i++)
                {
                    if (srcPtrs[i] != IntPtr.Zero)
                        Marshal.FreeHGlobal(srcPtrs[i]);
                }

                for (var i = 0; i < destPtrs.Length; i++)
                {
                    if (destPtrs[i] != IntPtr.Zero)
                        Marshal.FreeHGlobal(destPtrs[i]);
                }
            }
        }

        /// <summary>
        /// Processes a reverse stream of audio data
        /// </summary>
        /// <param name="src">Source audio data (array of channels)</param>
        /// <param name="inputConfig">Input stream configuration</param>
        /// <param name="outputConfig">Output stream configuration</param>
        /// <param name="dest">Destination audio data (array of channels)</param>
        /// <returns>Error code</returns>
        public ApmError ProcessReverseStream(float[][] src, StreamConfig inputConfig, StreamConfig outputConfig,
            float[][] dest)
        {
            if (src == null || inputConfig == null || outputConfig == null || dest == null)
                throw new ArgumentNullException();

            // Convert float[][] to IntPtr[] for interop
            var srcPtrs = new IntPtr[src.Length];
            var destPtrs = new IntPtr[dest.Length];

            try
            {
                for (int i = 0; i < src.Length; i++)
                {
                    srcPtrs[i] = Marshal.AllocHGlobal(src[i].Length * sizeof(float));
                    Marshal.Copy(src[i], 0, srcPtrs[i], src[i].Length);
                }

                for (int i = 0; i < dest.Length; i++)
                {
                    destPtrs[i] = Marshal.AllocHGlobal(dest[i].Length * sizeof(float));
                }

                // Pin the arrays
                GCHandle srcHandle = GCHandle.Alloc(srcPtrs, GCHandleType.Pinned);
                GCHandle destHandle = GCHandle.Alloc(destPtrs, GCHandleType.Pinned);

                try
                {
                    var error = NativeMethods.ProcessReverseStream(
                        NativePtr,
                        srcHandle.AddrOfPinnedObject(),
                        inputConfig.NativePtr,
                        outputConfig.NativePtr,
                        destHandle.AddrOfPinnedObject());

                    // Copy results back
                    for (int i = 0; i < dest.Length; i++)
                    {
                        Marshal.Copy(destPtrs[i], dest[i], 0, dest[i].Length);
                    }

                    return error;
                }
                finally
                {
                    srcHandle.Free();
                    destHandle.Free();
                }
            }
            finally
            {
                // Free allocated memory
                for (int i = 0; i < srcPtrs.Length; i++)
                {
                    if (srcPtrs[i] != IntPtr.Zero)
                        Marshal.FreeHGlobal(srcPtrs[i]);
                }

                for (int i = 0; i < destPtrs.Length; i++)
                {
                    if (destPtrs[i] != IntPtr.Zero)
                        Marshal.FreeHGlobal(destPtrs[i]);
                }
            }
        }

        /// <summary>
        /// Analyzes a reverse stream of audio data
        /// </summary>
        /// <param name="data">Audio data to analyze (array of channels)</param>
        /// <param name="reverseConfig">Reverse stream configuration</param>
        /// <returns>Error code</returns>
        public ApmError AnalyzeReverseStream(float[][] data, StreamConfig reverseConfig)
        {
            if (data == null || reverseConfig == null)
                throw new ArgumentNullException();

            // Convert float[][] to IntPtr[] for interop
            var dataPtrs = new IntPtr[data.Length];

            try
            {
                for (var i = 0; i < data.Length; i++)
                {
                    dataPtrs[i] = Marshal.AllocHGlobal(data[i].Length * sizeof(float));
                    Marshal.Copy(data[i], 0, dataPtrs[i], data[i].Length);
                }

                // Pin the array
                var dataHandle = GCHandle.Alloc(dataPtrs, GCHandleType.Pinned);

                try
                {
                    return NativeMethods.AnalyzeReverseStream(
                        NativePtr,
                        dataHandle.AddrOfPinnedObject(),
                        reverseConfig.NativePtr);
                }
                finally
                {
                    dataHandle.Free();
                }
            }
            finally
            {
                // Free allocated memory
                for (int i = 0; i < dataPtrs.Length; i++)
                {
                    if (dataPtrs[i] != IntPtr.Zero)
                        Marshal.FreeHGlobal(dataPtrs[i]);
                }
            }
        }

        /// <summary>
        /// Sets the analog level for the current stream
        /// </summary>
        /// <param name="level">Analog level (0-255)</param>
        public void SetStreamAnalogLevel(int level)
        {
            NativeMethods.SetStreamAnalogLevel(NativePtr, level);
        }

        /// <summary>
        /// Gets the recommended analog level for the current stream
        /// </summary>
        /// <returns>Recommended analog level (0-255)</returns>
        public int GetRecommendedStreamAnalogLevel()
        {
            return NativeMethods.GetRecommendedStreamAnalogLevel(NativePtr);
        }

        /// <summary>
        /// Sets the delay in ms between ProcessReverseStream() and ProcessStream()
        /// </summary>
        /// <param name="delayMs">Delay in milliseconds</param>
        public void SetStreamDelayMs(int delayMs)
        {
            NativeMethods.SetStreamDelayMs(NativePtr, delayMs);
        }

        /// <summary>
        /// Gets the current stream delay
        /// </summary>
        /// <returns>Current delay in milliseconds</returns>
        public int GetStreamDelayMs()
        {
            return NativeMethods.GetStreamDelayMs(NativePtr);
        }

        /// <summary>
        /// Sets a runtime setting with a float value
        /// </summary>
        /// <param name="type">Type of setting</param>
        /// <param name="value">Float value</param>
        public void SetRuntimeSetting(RuntimeSettingType type, float value)
        {
            NativeMethods.SetRuntimeSettingFloat(NativePtr, type, value);
        }

        /// <summary>
        /// Sets a runtime setting with an integer value
        /// </summary>
        /// <param name="type">Type of setting</param>
        /// <param name="value">Integer value</param>
        public void SetRuntimeSetting(RuntimeSettingType type, int value)
        {
            NativeMethods.SetRuntimeSettingInt(NativePtr, type, value);
        }

        /// <summary>
        /// Gets the current processing sample rate
        /// </summary>
        /// <returns>Sample rate in Hz</returns>
        public int GetProcSampleRateHz()
        {
            return NativeMethods.GetProcSampleRateHz(NativePtr);
        }

        /// <summary>
        /// Gets the current processing split sample rate
        /// </summary>
        /// <returns>Split sample rate in Hz</returns>
        public int GetProcSplitSampleRateHz()
        {
            return NativeMethods.GetProcSplitSampleRateHz(NativePtr);
        }

        /// <summary>
        /// Gets the number of input channels
        /// </summary>
        /// <returns>Number of input channels</returns>
        public int GetNumInputChannels()
        {
            return (int)NativeMethods.GetInputChannelsNum(NativePtr);
        }

        /// <summary>
        /// Gets the number of processing channels
        /// </summary>
        /// <returns>Number of processing channels</returns>
        public int GetNumProcChannels()
        {
            return (int)NativeMethods.GetProcChannelsNum(NativePtr);
        }

        /// <summary>
        /// Gets the number of output channels
        /// </summary>
        /// <returns>Number of output channels</returns>
        public int GetNumOutputChannels()
        {
            return (int)NativeMethods.GetOutputChannelsNum(NativePtr);
        }

        /// <summary>
        /// Gets the number of reverse channels
        /// </summary>
        /// <returns>Number of reverse channels</returns>
        public int GetNumReverseChannels()
        {
            return (int)NativeMethods.GetReverseChannelsNum(NativePtr);
        }

        /// <summary>
        /// Calculates the frame size for a given sample rate
        /// </summary>
        /// <param name="sampleRateHz">Sample rate in Hz</param>
        /// <returns>Frame size in samples</returns>
        public static int GetFrameSize(int sampleRateHz)
        {
            return (int)NativeMethods.GetFrameSize(sampleRateHz);
        }

        #region IDisposable Support

        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (NativePtr != IntPtr.Zero)
                {
                    NativeMethods.Destroy(NativePtr);
                    NativePtr = IntPtr.Zero;
                }

                _disposedValue = true;
            }
        }

        ~AudioProcessingModule()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}