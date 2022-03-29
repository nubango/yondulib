using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace CustomeDevice
{
    public static class ThirdPartyAPI
    {
        // This example assumes that the argument is a string that
        // contains the name of the Device, and that no two Devices
        // have the same name in the external API.
        public static Action<string> deviceAdded;
        public static Action<string> deviceRemoved;
    }

    // This example uses a MonoBehaviour with [ExecuteInEditMode]
    // on it to run the setup code. You can do this many other ways.
    //[ExecuteInEditMode]
    public class MyDeviceSupport : MonoBehaviour
    {
        protected void OnEnable()
        {
            ThirdPartyAPI.deviceAdded += OnDeviceAdded;
            ThirdPartyAPI.deviceRemoved += OnDeviceRemoved;
        }

        protected void OnDisable()
        {
            ThirdPartyAPI.deviceAdded -= OnDeviceAdded;
            ThirdPartyAPI.deviceRemoved -= OnDeviceRemoved;
        }

        private void OnDeviceAdded(string name)
        {
            // Feed a description of the Device into the system. In response, the
            // system matches it to the layouts it has and creates a Device.
            InputSystem.AddDevice(
                new InputDeviceDescription
                {
                    interfaceName = "ThirdPartyAPI",
                    product = name
                });
        }

        private void OnDeviceRemoved(string name)
        {
            var device = InputSystem.devices.FirstOrDefault(
                x => x.description == new InputDeviceDescription
                {
                    interfaceName = "ThirdPartyAPI",
                    product = name,
                });

            if (device != null)
                InputSystem.RemoveDevice(device);
        }

        // Move the registration of MyDevice from the
        // static constructor to here, and change the
        // registration to also supply a matcher.
        protected void Awake()
        {
            // Add a match that catches any Input Device that reports its
            // interface as "ThirdPartyAPI".
            InputSystem.RegisterLayout<MyDevice>(
                matches: new InputDeviceMatcher()
                    .WithInterface("ThirdPartyAPI"));
        }
    }
}