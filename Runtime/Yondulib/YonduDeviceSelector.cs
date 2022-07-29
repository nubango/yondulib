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
    public sealed class YonduDeviceSelector : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] Dropdown _deviceList = null;
        [SerializeField] Dropdown _channelList = null;

        #endregion

        #region Public properties
        public int Channel => _channelList.value;
        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            //// Buffer allocation
            //_audioData = new NativeArray<float>(4096, Allocator.Persistent);

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

            _deviceList.value = DeviceDriver.DeviceCount > 0 ? DeviceDriver.DefaultDeviceIndex + 1 : 0;

            _deviceList.RefreshShownValue();
        }


        #endregion

        #region UI callback

        public void OnDeviceSelected(int index)
        {
            // Reset the UI elements.
            _channelList.ClearOptions();
            _channelList.RefreshShownValue();

            YondulibManager.Instance.OpenStreamOnDevice(_deviceList.value);

            // Construct the channel list.
            _channelList.options =
                Enumerable.Range(0, YondulibManager.Instance.ChannelCount).
                Select(i => $"Channel {i + 1}").
                Select(text => new Dropdown.OptionData() { text = text }).
                ToList();

            _channelList.RefreshShownValue();
        }

        #endregion
    }
}
