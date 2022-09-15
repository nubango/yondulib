using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;


/*
emitimos un evento al reconocer y otro al dejar de reconocer.
dispara con los dos eventos (aun poniendo un minimo (interactions) en el actionAsset)
 
Investigar sobre como generamos el evento y como escribimos los valores.
 */

/*
 TODO:
 Utilizar tiempo de verdad en vez de contadores++
 */

namespace YonduLib.Recognizers
{
    public enum EventName { Click, Whistle, Silence, Null }


    public abstract class SoundRecognizer
    {
        protected SoundRecognizer(EventName name)
        {
            this.name = name;
        }


        #region ATTRIBUTES
        // nombre del evento que va a reconocer
        protected EventName name;

        // maxima frecuencia del espectro
        protected YonduNote maxFrequency = new YonduNote(0, 0);

        // intervalo en el cual se reconoce la misma frecuencia (inicializado en el Awake)
        protected float _offsetFrequency = 10.0f;

        // frecuencia mas alta del reconocedor
        protected float _eventFrequency = -1.0f;

        // Representa la duracion de un unico evento. Si una palmada se da muy fuerte
        // el sonido permanece un tiempo y si el valor de este atributo es muy bajo entonces
        // se generarán varios eventos CLICK cuento en realidad solo se ha producido una sola palmada.
        // El valor se inicializa en los contructores de cada reconocedor
        protected int _eventDuration = 100;

        // evento reconocido ahora
        // nivel de reconocimiento
        private float _recognitionLevel = 0.0f;

        // contador de silencio
        private int _countNotSoundDetected = 0;
        // contador de sonido
        private int _countSoundDetected = 0;
        // flag que se activa cuando se ha reconocido algo para que se cree un solo evento mientas dure el reconocimineto
        private bool _eventRecording = false;

        // contador de evento reconocido (suma mientras dure el reconocimiento)
        private bool _soundRecognize = false;
        #endregion


        #region PROTECTED_METHODS
        // metodo que analiza el espectro y devueve un float en el intervalo 0-1 siendo 0 = no reconocido y 1 = total reconocimiento
        protected abstract float AnalizeSpectrum(float[] array);
        #endregion


        #region PUBLIC_METHODS

        // trata de reconocer el evento que le corresponda y devuelve el porcentaje de reconocimiento
        public float Recognize(float[] array)
        {
            _recognitionLevel = AnalizeSpectrum(array);

            // maxFrequency contiene la frecuencia maxima en cada vuelta de bucle
            float freq = maxFrequency.frequency;

            // si eventFrequency es -1 significa que estamos ante la primera vuelta
            // despues de un reconocimiento positivo por lo que asignamos la frecuencia
            // maxima en esta vuelta a la frecuencia del posible evento actual
            if (_eventFrequency == -1)
                _eventFrequency = freq;


            // Analizamos el dato de reconocimiento del reconocedor para generar
            // el evento correspondiente.
            // si el grado de reconocimiento es superior al 87% y si la frecuencia
            // del sonido esta dentro del rango se genera un evento del tipo correspondiente
            if (_recognitionLevel > 0.8f && freq > _eventFrequency - _offsetFrequency
                && freq < _eventFrequency + _offsetFrequency)
            {
                _soundRecognize = true;
                _countNotSoundDetected = 0;
            }
            else
            {
                _countNotSoundDetected++;
            }



            // cuenta el tiempo que esta reconociendo un sonido
            if (_soundRecognize)
            {
                _countSoundDetected++;
            }

            // si el tiempo que ha pasado sin reconocerse ningun sonido es mayor a 25 
            // de resetean los flags y el evento actual se le da el valor de silencio
            if (_soundRecognize && _countNotSoundDetected * Time.deltaTime > 25 * Time.deltaTime)
            {
                _countNotSoundDetected = 0;
                _soundRecognize = false;
                _eventRecording = false;
                _eventFrequency = -1;

                //Debug.Log("Silencio");

                // generamos el evento de release 
                //name = EventName.Silence;
                EnqueueEvent(-1.0f, array.Length);

            }
            else if (!_soundRecognize)
                _eventFrequency = -1;


            // Si se lleva reconociendo un sonido mas de 100 entonces se genera otro evento
            if (_countSoundDetected * Time.deltaTime > _eventDuration * Time.deltaTime)
            {
                _countSoundDetected = 0;
                _eventRecording = false;
            }


            // Si se ha reconocido el sonido y no estamos generando el evento ya,
            // se genera el evento correspondiente
            if (_soundRecognize && !_eventRecording)
            {
                _eventRecording = true;

                _eventFrequency = freq;

                //if ((freq < array.Length * 0.6f) && (freq > array.Length * 0.2f))
                //{
                //    freq = freq - (array.Length * 0.2f);
                //    //freq = aux / array.Length * 0.4f;
                //}
                //else
                //    freq = 1;

                //Debug.Log(name.ToString() + " " + freq);
                EnqueueEvent(freq, array.Length);
            }

            return _recognitionLevel;
        }

        private void EnqueueEvent(float dataEvent, int length)
        {

            if (dataEvent == -1)
            {
                //InputSystem.QueueDeltaStateEvent(YonduLibDevice.YonduDevice.current.whistle, (short)0);
                //InputSystem.QueueDeltaStateEvent(YonduLibDevice.YonduDevice.current.whistle.x, (byte)0);
                //InputSystem.QueueDeltaStateEvent(YonduLibDevice.YonduDevice.current.whistle.y, (byte)0);
                InputSystem.QueueDeltaStateEvent(YonduLibDevice.YonduDevice.current.whistle, new Vector2(0f, 0f));

                InputSystem.QueueDeltaStateEvent(YonduLibDevice.YonduDevice.current.click, false);
            }
            else if (name == EventName.Click)
            {
                InputSystem.QueueDeltaStateEvent(YonduLibDevice.YonduDevice.current.click, true);
            }
            /*
            Modificado el método EnqueueEvent de la clase SoundRecognizer 
            para que los datos del evento generado se calculen en base 
            la frecuencia a la que se silba. 

            El silbido se trata como un joystick en el que la coordenada 
            y es 1 o 0 (silba o no silba) y la coordenada x es la que varia 
            en función de la frecuencia, -1 frecuencia graves y 1 frecuencia agudas.
            */
            else if (name == EventName.Whistle)
            {

                // Los rangos de frecuencia entre los que se mueven los silbidos es de 90 a 130. (depende de la resolucion de la muestra)
                // Restamos el minimo (90) para preparar las variables para la normalizacion
                float minFreq = 210f, maxFreq = 270f, curFreq;

                if (YondulibManager.Instance.analyzer.resolution == 256)
                {
                    minFreq = 90f; maxFreq = 130f; //256
                }
                else if (YondulibManager.Instance.analyzer.resolution == 512)
                {
                    //minFreq = 200f; maxFreq = 275f;
                    minFreq = 215f; maxFreq = 270f; //512
                }
                else if (YondulibManager.Instance.analyzer.resolution == 1024)
                {
                    minFreq = 500f; maxFreq = 600f; //1024
                }


                curFreq = dataEvent - minFreq;
                maxFreq -= minFreq;

                // Utilizamos la funcion f para calcular el valor de la y.
                // Necesitamos una funcion cuadrática ya que reparte el 

                // CALCULAMOS LA X
                // normalizamos los valores entre -1 y 1;
                float curFreqNorm;
                curFreqNorm = (curFreq * 2f / maxFreq) - 1f;
                //x = (curFreq * 3.2f / maxFreq) - 1.6f;
                //x = (curFreq * 2.314f / maxFreq) - 1.157f;


                // 1.- funcion de media circunferencia -> f(x) = sqtr(1 - x^2)
                //float x = curFreqNorm, y;
                //y = Mathf.Sqrt(1 - ((x * x) > 1 ? 1 : x * x));

                // 2.- f(x) = (sqrt(1 - (x^4)))^0.5
                //float x4 = Mathf.Pow(x, 4);
                //y = Mathf.Pow(Mathf.Sqrt(1 - (x4 > 1 ? 1 : x4)), 0.5f);


                // 3.- f(x) = 1.34(sqrt(0.87 - 0.65*(x^2))
                //y = 1.34f * Mathf.Sqrt(0.87f - (0.65f * ((x * x) > 1 ? 1 : x * x)));


                // 4.- f(x) = 1.6 - x -> y>1?1:y
                //y = 1f - Mathf.Abs(x);


                // 5.- f(x) = f(x) = sqtr(0.5 - x^2)
                //if (x > 0.71f)
                //{
                //    x = 1;
                //    y = 0;
                //}
                //else if (x < -0.71f)
                //{
                //    x = -1;
                //    y = 0;
                //}
                //else if (Mathf.Abs(x) < 0.1f)
                //{
                //    x = 0;
                //    y = 1;
                //}
                //else
                //{
                //    y = Mathf.Sqrt(0.5f - (x * x));
                //}
                //if (y > 0.7f) y = 1;



                // 6.- Media elipse pero al llegar al valor 0.812 la ecuacion de invierte
                float x = curFreqNorm, y;
                if (Mathf.Abs(curFreqNorm) > 0.812f)
                {
                    //sqrt {(1.3*1.3) * [1.05 - (x*x)]}
                    float t1 = 1.3f * 1.3f;
                    float t2 = x * x;
                    t2 = t2 > 1.0f ? 1.0f : t2;
                    float t3 = 1.0f - t2;
                    y = Mathf.Sqrt(t1 * t3);
                }
                else
                {
                    //sqrt {1.05 - [(x*x)/(1.3*1.3)]}
                    float t1 = 1.05f;
                    float t2 = x * x;
                    float t3 = t2 / (1.3f * 1.3f);
                    t3 = t3 > t1 ? t1 : t3;
                    y = Mathf.Sqrt(t1 - t3);
                }

                if (x > 1.0f) x = 1;
                else if (x < -1.0f) x = -1;

                if (y > 1.0f) y = 1;
                else if (y < 0.0f) y = 0;



                // 3
                //y = y > 1 ? 1 : y < 0.2f ? 0 : y;
                //if (x > 1) x = 1;
                //else if (x < -1) x = -1;



                //byte xi = (byte)(255 * (x + 1) / 2);
                //byte yi = (byte)(y * 255);

                //xi -= 127;
                //yi -= 127;

                //InputSystem.QueueDeltaStateEvent(YonduLibDevice.YonduDevice.current.whistle.x, xi);
                //InputSystem.QueueDeltaStateEvent(YonduLibDevice.YonduDevice.current.whistle.y, yi);


                //InputSystem.QueueDeltaStateEvent(YonduLibDevice.YonduDevice.current.whistle, (short)x);


                InputSystem.QueueDeltaStateEvent(YonduLibDevice.YonduDevice.current.whistle, new Vector2(x, y));

                //Debug.Log(x + " - " + y);
            }
        }
    }
    #endregion
}