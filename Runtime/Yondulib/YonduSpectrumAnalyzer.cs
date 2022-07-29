using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using YonduLib.Core;


// DFT spectrum analyzer graph
namespace YonduLib
{
    public sealed class YonduSpectrumAnalyzer : MonoBehaviour
    {
        #region Public properties

        public float CurrentGain { get; set; } = -13;
        public float DynamicRange { get; set; } = 75;


        public readonly int resolution = 1024;
        //DftBuffer _dft;
        //public ReadOnlySpan<float> logSpectrumSpan => _dft.Spectrum;


        // X-axis log scaled spectrum data as NativeArray
        public Unity.Collections.NativeArray<float> logSpectrumArray
          => LogScaler.Resample(Fft.Spectrum);

        // X-axis log scaled spectrum data as ReadOnlySpan
        public System.ReadOnlySpan<float> logSpectrumSpan
          => logSpectrumArray.GetReadOnlySpan();

        #endregion

        #region Internal objects
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
            _fft?.Push(YondulibManager.Instance.AudioDataSpan);
            _fft?.Analyze(-CurrentGain - DynamicRange, -CurrentGain);
        }



        #endregion
    }
}