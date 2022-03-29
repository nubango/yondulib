using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using YonduLib.Core;


// DFT spectrum analyzer graph
namespace YonduLib
{
    public sealed class SpectrumAnalyzer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] DeviceSelector _selector = null;

        #endregion

        #region Public properties

        public float CurrentGain { get; set; } = -13;
        public float DynamicRange { get; set; } = 75;
        #endregion

        #region Internal objects

        public readonly int resolution = 256;
        //DftBuffer _dft;
        //public ReadOnlySpan<float> logSpectrumSpan => _dft.Spectrum;


        // X-axis log scaled spectrum data as NativeArray
        public Unity.Collections.NativeArray<float> logSpectrumArray
          => LogScaler.Resample(Fft.Spectrum);

        // X-axis log scaled spectrum data as ReadOnlySpan
        public System.ReadOnlySpan<float> logSpectrumSpan
          => logSpectrumArray.GetReadOnlySpan();

        public bool DeviceSelected => _selector.Stream != null;


        // FFT buffer object with lazy initialization
        FftBuffer Fft => _fft ??= new FftBuffer(resolution * 2);
        FftBuffer _fft;

        // Log scale resampler with lazy initialization
        LogScaler LogScaler => _logScaler ??= new LogScaler();
        LogScaler _logScaler;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            //_dft = new DftBuffer(resolution);
        }

        void OnDisable()
        {
            _fft?.Dispose();
            _fft = null;

            _logScaler?.Dispose();
            _logScaler = null;
        }

        void Update()
        {
            //_dft.Push(_selector.AudioDataSpan);
            //_dft.Analyze();


            // FFT
            _fft?.Push(_selector.AudioDataSpan);
            _fft?.Analyze(-CurrentGain - DynamicRange, -CurrentGain);
        }



        #endregion
    }
}