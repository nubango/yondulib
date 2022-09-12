using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.InteropServices;
using Unity.Collections;
using YonduLib.Core.Driver;

namespace YonduLib
{
    /// <summary>
    /// Clase que gestiona un flujo de datos asociado a un micrófono.
    ///  - Permite abrir un flujo asociado a un micrófono
    ///  - Permite saber el canal por el que se estan transmitiendo los datos
    ///  - Permite acceder al 
    /// </summary>
    public class YonduStreamManager
    {
        #region Public properties and methods 
        /// <summary>
        /// Propiedad que devuelve el canal seleccionado del audio. Si no se ha seleccionado nunguno se devuelve el de por defecto.
        /// </summary>
        public int Channel => YondulibManager.Instance.deviceSelector != null ? YondulibManager.Instance.deviceSelector.Channel : _channel >= 0 ? _channel : 0;


        /// <summary>
        /// Propiedad que devuelve el número máximo de canales que tiene el micrófono
        /// </summary>
        public int ChannelCount => _stream?.ChannelCount ?? 0;

        /// <summary>
        /// Propiedad que devuelve la frecuencia de muestreo
        /// </summary>
        public int SampleRate => _stream?.SampleRate ?? 0;

        /// <summary>
        /// Propiedad que permite saber si hay un flujo abierto
        /// </summary>
        public bool isStreamExist => _stream != null;


        /// <summary>
        /// Propiedad que devuelve los datos recogidos por el micrófono
        /// </summary>
        public NativeArray<float> AudioDataSpan =>
        _audioData.GetSubArray(0, _audioDataFilled);


        /// <summary>
        /// Método que abre un flujo asociado al dispositivo que representa el número pasado por parámetro
        /// </summary>
        /// <param name="device"> Int que representa el micrófono que va asociado al flujo que se va a abrir</param>
        public void OpenStreamOnDevice(int device)
        {
            // Stop and destroy the current stream.
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            // Break if the null device option was selected.
            if (device == 0)
                return;

            // Open a new stream.
            try
            {
                _stream = DeviceDriver.OpenInputStream(device - 1);
            }
            catch (System.InvalidOperationException)
            {
                Debug.LogWarning("The stream has NOT been opened successfully. Device with ID:" + device + " does not exist.");
                return;
            }
        }

        /// <summary>
        /// Método que inicializa todo lo necesario para poder acceder al flujo de datos
        /// </summary>
        public void Init()
        {
            // Buffer allocation
            _audioData = new NativeArray<float>(4096, Allocator.Persistent);
            _channel = -1;
            _isInit = true;

            if (GameObject.FindObjectOfType<YonduDeviceSelector>() == null)
                //Debug.Log("No se ha encontrado ningun Device selector");
                OpenStreamOnDevice(DeviceDriver.DeviceCount > 0 ? DeviceDriver.DefaultDeviceIndex + 1 : 0);
        }

        /// <summary>
        /// Método que libera los recursos utilizados para acceder al flujo de dateos
        /// </summary>
        public void Dispose()
        {
            _stream?.Dispose();
            _audioData.Dispose();
            _isInit = false;
        }

        /// <summary>
        /// Se leen los datos recogidos por el micrófono en cada vuelta de bucle
        /// </summary>
        public void Update()
        {
            if (_stream == null || !_isInit)
            {
                _audioDataFilled = 0;
                Debug.LogError("Stream is not open or YonduStreamManager not initialize");
                return;
            }

            // Strided copy
            var input = MemoryMarshal.Cast<byte, float>(_stream.LastFrameWindow);
            var stride = _stream.ChannelCount;
            var offset = Channel;

            _audioDataFilled = Mathf.Min(input.Length, input.Length / stride);

            for (var i = 0; i < _audioDataFilled; i++)
                _audioData[i] = input[i * stride + offset] * YondulibManager.Instance.Volume;

        }

        #endregion

        #region Internal objects

        private InputStream _stream = null;
        private NativeArray<float> _audioData;
        private int _audioDataFilled;
        private int _channel;
        private bool _isInit = false;
        #endregion

    }
}