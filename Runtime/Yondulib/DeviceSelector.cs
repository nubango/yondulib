using System;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using YonduLib.Core.Driver;

// Device selector class
// - Controlls the device selection UI.
// - Manages SoundIO objects.
// - Provides audio data for the other scripts.


namespace YonduLib
{
    public sealed class DeviceSelector : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] Dropdown _deviceList = null;
        [SerializeField] Dropdown _channelList = null;

        #endregion

        #region Public properties

        public int Channel => _channelList.value;
        public int ChannelCount => Stream?.ChannelCount ?? 0;
        public int SampleRate => Stream?.SampleRate ?? 0;

        public float Volume { get; set; } = 1;

        public NativeArray<float> AudioDataSpan =>
            _audioData.GetSubArray(0, _audioDataFilled);

        public NativeSlice<float> AudioDataSlice =>
            new NativeSlice<float>(_audioData, 0, _audioDataFilled);

        public InputStream Stream;

        #endregion

        #region Internal objects

        NativeArray<float> _audioData;
        int _audioDataFilled;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            // Buffer allocation
            _audioData = new NativeArray<float>(4096, Allocator.Persistent);

            // Clear the UI contents.
            _deviceList.ClearOptions();
            _channelList.ClearOptions();

            // Null device option
            _deviceList.options.Add(new Dropdown.OptionData() { text = "--" });

            // Device list initialization
            _deviceList.options.AddRange(
                Enumerable.Range(0, DeviceDriver.DeviceCount).
                    Select(i => DeviceDriver.GetDeviceName(i)).
                    Select(name => new Dropdown.OptionData() { text = name }));

            _deviceList.RefreshShownValue();
        }

        void OnDestroy()
        {
            Stream?.Dispose();
            _audioData.Dispose();
        }

        void Update()
        {
            if (Stream == null)
            {
                _audioDataFilled = 0;
                return;
            }

            // Strided copy
            var input = MemoryMarshal.Cast<byte, float>(Stream.LastFrameWindow);
            var stride = Stream.ChannelCount;
            var offset = Channel;

            _audioDataFilled = Mathf.Min(input.Length, input.Length / stride);

            for (var i = 0; i < _audioDataFilled; i++)
                _audioData[i] = input[i * stride + offset] * Volume;

        }

        #endregion

        #region UI callback

        public void OnDeviceSelected(int index)
        {
            // Stop and destroy the current stream.
            if (Stream != null)
            {
                Stream.Dispose();
                Stream = null;
            }

            // Reset the UI elements.
            _channelList.ClearOptions();
            _channelList.RefreshShownValue();

            // Break if the null device option was selected.
            if (_deviceList.value == 0)
                return;


            // Open a new stream.
            try
            {
                Stream = DeviceDriver.OpenInputStream(_deviceList.value - 1);
            }
            catch (System.InvalidOperationException)
            {
                return;
            }


            // Construct the channel list.
            _channelList.options =
                Enumerable.Range(0, Stream.ChannelCount).
                Select(i => $"Channel {i + 1}").
                Select(text => new Dropdown.OptionData() { text = text }).
                ToList();

            _channelList.RefreshShownValue();
        }

        #endregion
    }
}
