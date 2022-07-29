using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YonduLib.Utils;

namespace YonduLib.Recognizers
{

    /// <summary>
    /// Clase que se encarga de identificar golpes, chasquidos o similares
    /// </summary>
    public class ClickRecognizer : SoundRecognizer
    {
        public ClickRecognizer(EventName name, int resolution) : base(name)
        {
            _offsetFrequency = resolution;
            _eventDuration = 200;
        }

        /// <summary>
        /// Identifica si ha habido un golpe, chasquido o similar
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Para identificar el golpe/chasquido se analizan tres parametros. 
        /// </para>
        ///     <para>
        ///         Parametro 1: numero de picos que contiene el array. Cuantos mas picos contenga mas probabilidades hay de que sea un golpe/chasquido y viceversa.
        ///         Para ello recorremos todo el array comparando la posicion actual con la anterior y la posterior para saber cuando deja de crecer la intensidad y empieza
        ///         a decrecer.
        ///     </para>
        ///     <para>
        ///         Parametro 2: maxima amplitud de la frecuencia con mayor intensidad. Si la amplitud es pequeña hay mas probabilidades de que sea un golpe/chasquido.
        ///     </para>
        ///      <para>
        ///         Para hayar este parametro usamos el patron de ventana deslizante ligeramente modificado. Utilizamos dos colas de prioridad, una creciente y otra
        ///         decreciente, para ordenar las frecuencias por intensidad y poder obtener el maximo y el minimo de las frecuencias que abarca la ventana. La dimension de
        ///         la ventana es de el 10% de la longitud del array que es mas o menos lo que ocupa un pico cuando se silba o se toca una nota musical.
        ///     </para>
        ///     <para>
        ///         Parametro 3: comparacion de los picos maximos. Los picos maximos suelen tener poca diferencia de intensidad entre ellos en golpes o chasquidos
        ///     </para>
        ///    
        /// </remarks>
        /// <param name="array">Array de frecuencias e intensidades que proporciona SpectrumAnalizer</param>
        /// <returns>
        /// <para>
        ///     Devuelve un float entre 0 y 1 incluidos. {0 -> ninguna coincidencia | 1 -> 100% de coincidencia}
        /// </para>
        /// <para>
        ///     El valor devuelto lo componen la suma de los tres factores comentados anteriormente. Cada uno de ellos aportara un valor maximo de 0.33 aproximadamente, por
        ///     lo que la suma de los tres dara como maximo ~1~.
        /// </para>
        /// </returns>
        protected override float AnalizeSpectrum(float[] array)
        {
            #region factor 1: numero de picos

            // cuenta los picos que hay y despues saca el porcentaje de afinidad con los chasquidos (muchos picos -> muy afin con chasquidos)
            // lo multiplicamos por 0.33 porque hay otros dos factores que sumados conforman el 100% 
            int countFrecActivas = 0;
            float last = 0.01f;
            for (int j = 0; j < array.Length; j++)
            {
                if (array[j] > last && j + 1 < array.Length && array[j] > array[j + 1])
                    countFrecActivas++;
                last = array[j];
            }


            // si no hay sonido (countFrecActivas = 0) -> factor = 0
            // si hay sonido (countFrecActivas > 0) -> countFrecActivas = 1 => factor = 0,000001
            //                                      -> countFrecActivas = array.Length => factor = 0.33
            // (x/(length/7)) * 0.33   (length/7 => ¿1 de cada 7 son picos?-> 100% afinidad)

            if (countFrecActivas > array.Length / 7) countFrecActivas = array.Length / 7;

            float factor1 = countFrecActivas == 0 ? 0 : ((float)countFrecActivas / (array.Length / (float)7)) * 0.33f;

            //if (factor1 > 0.3)
            //    Debug.Log("Factor 1: " + factor1);
            #endregion


            #region factor 2: diferencia de entre intensidad max-min

            // Factor de escala que representa la dimension de la ventana deslizante.
            // factorScaleWindow = 0.1 -> extension de la ventana es el 10% de la dimension total del array.
            float factorScaleWindow = 0.1f;
            int ancho = (int)(factorScaleWindow * array.Length);

            int i = 0;
            YonduNote maxi = new YonduNote(array[0], i);
            YonduNote min = new YonduNote(array[0], i);

            // cola con todas las frecuencias ordenadas de mayor a menor
            PriorityQueue<YonduNote> pqAllFeq = new PriorityQueue<YonduNote>(true);

            // cola auxiliar de maximos
            PriorityQueue<YonduNote> pqMaxs = new PriorityQueue<YonduNote>(true);

            // cola auxiliar de minimos
            PriorityQueue<YonduNote> pqMins = new PriorityQueue<YonduNote>();

            // buscamos las frecuencias maxima y minima de la ventana para saber cual es la diferencia
            do
            {
                pqAllFeq.Enqueue(new YonduNote(array[i], i));
                if (array[i] > maxi.intensity)
                {
                    maxi.intensity = array[i];
                    maxi.frequency = i;
                }
                else if (array[i] < min.intensity) min.intensity = array[i];
            } while (i++ < ancho);

            YonduNote maxDiff = new YonduNote(maxi.intensity - min.intensity, maxi.frequency);

            // metemos el maximo y el minimo en las colas de prioridad
            pqMaxs.Enqueue(maxi);
            pqMins.Enqueue(min);

            // calculamos la diferencia de intensidad de todo el espectro
            //[a b c] d e f
            //a [b c d] e f
            while (i < array.Length)
            {
                pqAllFeq.Enqueue(new YonduNote(array[i], i));

                if (array[i] > maxi.intensity)
                {
                    maxi.intensity = array[i];
                    maxi.frequency = i;
                }
                // si el elemento que sale es el maximo o el minimo lo sacamos de la cola
                if (pqMaxs.Peek().intensity == array[i - ancho])
                    pqMaxs.Dequeue();
                else if (pqMins.Peek().intensity == array[i - ancho])
                    pqMins.Dequeue();

                // introducimos el nuevo elemento a la cola
                pqMaxs.Enqueue(new YonduNote(array[i], i));
                pqMins.Enqueue(new YonduNote(array[i], i));

                // comprobamos si la dif de esta ventana supera a la dif maxima hasta ahora
                float aux = pqMaxs.Peek().intensity - pqMins.Peek().intensity;
                if (aux > maxDiff.intensity)
                {
                    maxDiff.intensity = aux;
                    maxDiff.frequency = pqMaxs.Peek().frequency;
                }

                i++;
            }


            // el valor de maxDiff oscila entre 0 y 0.8, por lo tanto 0.8 equivale a 0.0
            // si el valor es menor a 0.4 le damos lo maximo 0.33

            float factor2 = maxDiff.intensity < 0.1 ? 0 : (maxDiff.intensity < 0.4f ? 1.0f : 1 - (maxDiff.intensity - 0.4f)) * 0.33f;

            //if (maxDiff.intensity > 0.2f)
            //    Debug.Log("MaxDiff: " + maxDiff.intensity);
            //if (factor2 > 0.1)
            //    Debug.Log("Factor 2: " + factor2);
            #endregion


            #region factor 3: comparacion de los picos máximos
            // Usamos la cola de prioridad que contiene todas las frecuencias de la muestra, ordenadas de mayor a menor. 
            // Compararemos sus valores para identificar la diferencia que hay entre unos maximos(picos) y otros.
            //
            // Es probable que cuando se detecta una subida, haya mas picos cerca por lo que dicha diferencia no es significativa. Nos interesa comparar los maximos que
            // esten distanciados. El valor limite utilizado es el ancho de la ventana deslizante (usada para detectar dichos maximos). 

            YonduNote top = pqAllFeq.Dequeue();
            float dif = 0;
            // Solo usamos las frecuencias con valor superior a 0
            while (pqAllFeq.Count > 0 && pqAllFeq.Peek().intensity > 0)
            {
                // si las frecuencias estan demasiado juntas se desecha la menor (ancho ventana deslixante como limite minimo)
                while (pqAllFeq.Count > 0 && Mathf.Abs(top.frequency - pqAllFeq.Peek().frequency) < ancho * 2)
                {
                    pqAllFeq.Dequeue();
                }

                if (pqAllFeq.Count > 0)
                {
                    dif = top.intensity - pqAllFeq.Peek().intensity > dif ? top.intensity - pqAllFeq.Peek().intensity : dif;
                    top = pqAllFeq.Dequeue();
                }
            }

            // si dif es 0 significa que solo hay un pico por lo que la diferencia es el valor mismo del pico
            dif = dif == 0 ? top.intensity : dif;

            // el valor de dif oscila entre 0 y 0.6, por lo tanto 0.6 equivale a 0.0
            // si el valor es menor a 0.25 le damos lo maximo 0.33
            float factor3 = dif == 0.0f ? 0.0f : (dif < 0.25f ? 1 : 1 - dif) * 0.33f;

            //Debug.Log("Diferencia: " + dif);
            //if (factor1 > 0.1)
            //    Debug.Log("Factor 3: " + factor3);
            #endregion

            return factor1 + factor2 + factor3;
        }
    }
}