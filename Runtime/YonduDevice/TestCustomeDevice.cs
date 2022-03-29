using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

namespace CustomeDevice
{


    // #################################### ESTRUCTURA DEL ESTADO #################################### //

    // El primer paso es crear un struct que represente la forma
    // en que el sistema recibe y almacena la entrada, y también 
    // describe las instancias InputControl que el sistema de entrada
    // debe crear para el dispositivo a fin de recuperar su estado.



    // A "state struct" describes the memory format that a Device uses. Each Device can
    // receive and store memory in its custom format. InputControls then connect to
    // the individual pieces of memory and read out values from them.
    //
    // If it's important for the memory format to match 1:1 at the binary level
    // to an external representation, it's generally advisable to use
    // LayoutLind.Explicit.
    public struct MyDeviceState : IInputStateTypeInfo
    {
        // You must tag every state with a FourCC code for type
        // checking. The characters can be anything. Choose something that allows
        // you to easily recognize memory that belongs to your own Device.
        public FourCC format => new FourCC('M', 'Y', 'D', 'V');

        // InputControlAttributes on fields tell the Input System to create Controls
        // for the public fields found in the struct.

        // Assume a 16bit field of buttons. Create one button that is tied to
        // bit #3 (zero-based). Note that buttons don't need to be stored as bits.
        // They can also be stored as floats or shorts, for example. The
        // InputControlAttribute.format property determines which format the
        // data is stored in. If omitted, the system generally infers it from the value
        // type of the field.

        [InputControl(name = "click", displayName = "Click Button", layout = "Button", bit = 0)]
        [InputControl(name = "whistle", displayName = "Whistle Button", layout = "Button", bit = 1)]
        public ushort buttons;

        // Create a floating-point axis. If a name is not supplied, it is taken
        // from the field.
       
    }





    // #################################### CLASE DEL DISPOSITIVO #################################### //

    // InputControlLayoutAttribute attribute is only necessary if you want
    // to override the default behavior that occurs when you register your Device
    // as a layout.
    // The most common use of InputControlLayoutAttribute is to direct the system
    // to a custom "state struct" through the `stateType` property. See below for details.
    [InputControlLayout(displayName = "My Device", stateType = typeof(MyDeviceState))]
    // Add the InitializeOnLoad attribute to automatically run the static
    // constructor of the class after each C# domain load.
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class MyDevice : InputDevice
    {
        // In the state struct, you added two Controls that you now want to
        // surface on the Device, for convenience. The Controls
        // get added to the Device either way. When you expose them as properties,
        // it is easier to get to the Controls in code.

        [InputControl(parameters = "pressPoint=1.0")]
        public ButtonControl click { get; private set; }
        public ButtonControl whistle { get; private set; }

        // The Input System calls this method after it constructs the Device,
        // but before it adds the device to the system. Do any last-minute setup
        // here.
        protected override void FinishSetup()
        {
            base.FinishSetup();

            // NOTE: The Input System creates the Controls automatically.
            //       This is why don't do `new` here but rather just look
            //       the Controls up.
            click = GetChildControl<ButtonControl>("click");
            whistle = GetChildControl<ButtonControl>("whistle");
        }

        static MyDevice()
        {
            // RegisterLayout() adds a "Control layout" to the system.
            // These can be layouts for individual Controls (like sticks)
            // or layouts for entire Devices (which are themselves
            // Controls) like in our case.
            InputSystem.RegisterLayout<MyDevice>();
        }


        // Revisar porque este metodo no sale en la docuemntacion oficial
        [MenuItem("Tools/Add MyDevice")]
        public static void Initialize()
        {
            InputSystem.AddDevice<MyDevice>();
        }

        // Acceder rápidamente al último dispositivo utilizado de un tipo
        // determinado o enumerar todos los dispositivos de un tipo específico
        public static MyDevice current { get; private set; }

        public static IReadOnlyList<MyDevice> All => s_AllMyDevices;
        private static List<MyDevice> s_AllMyDevices = new List<MyDevice>();
        
        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            s_AllMyDevices.Add(this);
        }

        protected override void OnRemoved()
        {
            base.OnRemoved();
            s_AllMyDevices.Remove(this);
        }
    }
}


