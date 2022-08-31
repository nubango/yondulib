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

namespace YonduLibDevice
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
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct YonduDeviceState : IInputStateTypeInfo
    {
        // You must tag every state with a FourCC code for type
        // checking. The characters can be anything. Choose something that allows
        // you to easily recognize memory that belongs to your own Device.
        public FourCC format => new FourCC('Y', 'D', 'D', 'V');

        // InputControlAttributes on fields tell the Input System to create Controls
        // for the public fields found in the struct.

        // Assume a 16bit field of buttons. Create one button that is tied to
        // bit #3 (zero-based). Note that buttons don't need to be stored as bits.
        // They can also be stored as floats or shorts, for example. The
        // InputControlAttribute.format property determines which format the
        // data is stored in. If omitted, the system generally infers it from the value
        // type of the field.

        [InputControl(name = "click", displayName = "Click Button", shortDisplayName = "CB", layout = "Button", format = "BIT", bit = 0)]
        [FieldOffset(0)]
        public bool click;


        //[InputControl(name = "whistle", layout = "Stick", format = "VC2B")]
        //[InputControl(name = "whistle/x", offset = 0, format = "BYTE")]//, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        //[InputControl(name = "whistle/left", offset = 0, format = "BYTE")]//, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp=1,clampMin=0,clampMax=0.5")]
        //[InputControl(name = "whistle/right", offset = 0, format = "BYTE")]//, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp=1,clampMin=0.5,clampMax=1")]
        //[InputControl(name = "whistle/y", offset = 1, format = "BYTE")]//, parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        //[InputControl(name = "whistle/up", offset = 1, format = "BYTE")]//, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp=1,clampMin=0,clampMax=0.5")]
        //[InputControl(name = "whistle/down", offset = 1, format = "BYTE")]//, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp=1,clampMin=0.5,clampMax=1,invert=false")]
        //[FieldOffset(1)] public float whistleX;
        //[FieldOffset(2)] public float whistleY;


        [InputControl(name = "whistle", displayName = "Whistle Stick", shortDisplayName = "WS", layout = "Stick", usage = "Primary2DMotion")]
        [FieldOffset(1)]
        public Vector2 whistle;

        //[InputControl(name = "whistle/x", offset = 0, format = "BYTE")] //, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        //[InputControl(name = "whistle/left", offset = 0, format = "BYTE")] //, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp=1,clampMin=0,clampMax=0.5,invert")]
        //[InputControl(name = "whistle/right", offset = 0, format = "BYTE")] //, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp=1,clampMin=0.5,clampMax=1")]
        //[InputControl(name = "whistle/y", offset = 1, format = "BYTE")] //, parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        //[InputControl(name = "whistle/up", offset = 1, format = "BYTE")] //, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp=1,clampMin=0,clampMax=0.5,invert")]
        //[InputControl(name = "whistle/down", offset = 1, format = "BYTE")] //, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp=1,clampMin=0.5,clampMax=1,invert=false")]
        //[InputControl(name = "whistle", displayName = "Whistle Stick", shortDisplayName = "WS", layout = "Stick", usage = "Primary2DMotion", processors = "stickDeadzone")]
        //[FieldOffset(3)]
        //public Vector2 whistle;


        //[InputControl(name = "whistle2", displayName = "Whistle Stick2", layout = "Stick", format = "VC2B")]
        //[InputControl(name = "whistle2/x", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        //[InputControl(name = "whistle2/left", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp=1,clampMin=0,clampMax=0.5,invert")]
        //[InputControl(name = "whistle2/right", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp=1,clampMin=0.5,clampMax=1")]
        //[InputControl(name = "whistle2/y", offset = 1, format = "BYTE", parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        //[InputControl(name = "whistle2/up", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp=1,clampMin=0,clampMax=0.5,invert")]
        //[InputControl(name = "whistle2/down", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp=1,clampMin=0.5,clampMax=1,invert=false")]
        //[FieldOffset(2)] public byte leftStickX;
        //[FieldOffset(3)] public byte leftStickY;



    }





    // #################################### CLASE DEL DISPOSITIVO #################################### //

    // InputControlLayoutAttribute attribute is only necessary if you want
    // to override the default behavior that occurs when you register your Device
    // as a layout.
    // The most common use of InputControlLayoutAttribute is to direct the system
    // to a custom "state struct" through the `stateType` property. See below for details.
    [InputControlLayout(displayName = "YonduDevice", stateType = typeof(YonduDeviceState))]
    // Add the InitializeOnLoad attribute to automatically run the static
    // constructor of the class after each C# domain load.
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class YonduDevice : InputDevice
    {
        // In the state struct, you added two Controls that you now want to
        // surface on the Device, for convenience. The Controls
        // get added to the Device either way. When you expose them as properties,
        // it is easier to get to the Controls in code.

        [InputControl(parameters = "pressPoint=1.0")]
        public ButtonControl click { get; private set; }
        public StickControl whistle { get; private set; }

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
            whistle = GetChildControl<StickControl>("whistle");

        }

        static YonduDevice()
        {
            // RegisterLayout() adds a "Control layout" to the system.
            // These can be layouts for individual Controls (like sticks)
            // or layouts for entire Devices (which are themselves
            // Controls) like in our case.
            InputSystem.RegisterLayout<YonduDevice>();

            // Register a new layout and supply a matcher for it.
            InputSystem.RegisterLayoutMatcher<YonduDevice>(
                matcher: new InputDeviceMatcher()
                    .WithInterface("YonduLib")
                    .WithProduct("Yondu Device*")
                    .WithManufacturer("UCM"));
        }


        // Revisar porque este metodo no sale en la docuemntacion oficial
#if UNITY_EDITOR
        [MenuItem("Tools/Add YonduDevice")]
#endif
        public static void Initialize()
        {
            InputSystem.AddDevice<YonduDevice>();
        }

        // Acceder rápidamente al último dispositivo utilizado de un tipo
        // determinado o enumerar todos los dispositivos de un tipo específico
        public static YonduDevice current { get; private set; }

        public static IReadOnlyList<YonduDevice> All => s_AllYonduDevices;
        private static List<YonduDevice> s_AllYonduDevices = new List<YonduDevice>();

        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            s_AllYonduDevices.Add(this);
        }

        protected override void OnRemoved()
        {
            base.OnRemoved();
            s_AllYonduDevices.Remove(this);
        }
    }
}