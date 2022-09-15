using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using YonduLib.Recognizers;
using Unity.Collections;
using YonduLib.Core.Driver;


namespace YonduLib
{
    /*
     * Clase que se encarga de gestionar los SoundRecognizers y les proporciona el array de frecuencias a cada uno
     * 
     * Guarda el stream de datos
     * 
     * Gestiona la representación gráfica de los reconocedores (medidores)
     * 
     * **/
    public class YondulibManager : MonoBehaviour
    {
        #region Unity implementation

        public YonduSpectrumAnalyzer analyzer;


        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                TransferInfo();
                DestroyImmediate(this.gameObject);
                return;
            }

            _instance = this;

            InitInformation();

            DontDestroyOnLoad(this);
        }

        private void TransferInfo()
        {
            _instance.medidorPrefab = medidorPrefab;
            _instance.canvas = canvas;
        }

        private void InitInformation()
        {
            if (analyzer == null)
                Debug.LogError("Atributo YonduSpectrumAnalyzer no asignado en el componente YondulibManager");

            if (_streamManager == null)
                (_streamManager = new YonduStreamManager()).Init();


            // Init recognizers

            if (_recognizers == null)
            {
                _recognizers = new List<SoundRecognizer>();

                // El orden en el que de meten debe ser el mismo del enum COMBONAME
                // de la clase COMBO para que la creacion de los combos sea correcta
                _recognizers.Add(new ClickRecognizer(EventName.Click, analyzer.resolution));
                _recognizers.Add(new WhistleRecognizer(EventName.Whistle, analyzer.resolution));
            }

            // Init medidores

            if (_medidor == null && _medidorInitSize == null && canvas != null)
            {
                int numRecognizers = _recognizers.Count;

                //_selector = Instantiate(selectorPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                //_selector.transform.SetParent(canvas.transform, false);
                //_selector.gameObject.SetActive(false);


                _medidor = new YonduMedidor[numRecognizers];
                _medidorInitSize = new Vector3[numRecognizers];

                for (int i = 0; i < numRecognizers; i++)
                {
                    _medidor[i] = Instantiate(medidorPrefab, new Vector3(i * 100, 0, 0), Quaternion.identity);
                    _medidor[i].transform.SetParent(canvas.transform, false);
                    _medidor[i].gameObject.SetActive(activeMedidores);

                    _medidorInitSize[i] = _medidor[i].transform.localScale;
                }
            }
        }

        private void Update()
        {
            // si no se ha seleccionado ningun dispositivo de entrada no hay sonido que analizar
            if (!_streamManager.isStreamExist)
                return;

            _streamManager.Update();

            float[] array = analyzer.logSpectrumSpan.ToArray();

            //_selector.SetActive(activeSelector);

            for (int i = 0; i < _recognizers.Count; i++)
            {
                float valueRecognize = _recognizers[i].Recognize(array);

                if (_medidor != null && _medidor[i] != null)
                {
                    _medidor[i].gameObject.SetActive(activeMedidores);
                    if (activeMedidores) UpdateMedidor(valueRecognize, i);
                }
            }
        }

        private void OnDestroy()
        {
            _streamManager?.Dispose();
        }

        #endregion


        #region Graphic implementation


        public YonduMedidor medidorPrefab;
        public Canvas canvas;
        public bool activeMedidores;

        private YonduMedidor[] _medidor = null;
        private Vector3[] _medidorInitSize;


        /*
         * Metodo que se encarga de actualizar los medidores graficos de los recognizers
         * **/
        private void UpdateMedidor(float valueRecognize, int it)
        {
            if (_medidor == null || it >= _medidor.Length || _medidor[it] == null)
                return;

            Image i = _medidor[it].Green.GetComponentInChildren<Image>();
            MeshRenderer m = _medidor[it].Green.GetComponentInChildren<MeshRenderer>();

            if (i != null)
            {
                i.transform.localScale = new Vector3(_medidorInitSize[it].x, _medidorInitSize[it].y * valueRecognize, _medidorInitSize[it].z);
                i.color = Color.Lerp(Color.red, Color.green, valueRecognize);
            }
            else if (m != null)
            {
                m.transform.localScale = new Vector3(_medidorInitSize[it].x, _medidorInitSize[it].y * valueRecognize, _medidorInitSize[it].z);
                m.material.color = Color.Lerp(Color.red, Color.green, valueRecognize);
            }
        }
        #endregion


        #region Recognizers implementation
        /*
         * Devuelve la resolucion a la que se está captando el sonido
         * **/
        public int GetResolution() => analyzer.resolution;

        // Lista con los reconocedores que hay
        private List<SoundRecognizer> _recognizers = null;

        #endregion


        #region Stream manager
        public YonduDeviceSelector deviceSelector;

        private YonduStreamManager _streamManager = null;

        public int Channel => _streamManager.Channel;
        public int ChannelCount => _streamManager.ChannelCount;
        public int SampleRate => _streamManager.SampleRate;

        public float Volume { get; set; } = 1;

        public NativeArray<float> AudioDataSpan => _streamManager.AudioDataSpan;

        public void OpenStreamOnDevice(int device) { _streamManager.OpenStreamOnDevice(device); }

        #endregion


        #region Singleton
        private static YondulibManager _instance = null;
        public static YondulibManager Instance => _instance;
        #endregion

    }
}