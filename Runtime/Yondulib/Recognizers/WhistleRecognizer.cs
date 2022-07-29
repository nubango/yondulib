using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YonduLib.Utils;

namespace YonduLib.Recognizers
{
    /// <summary>
    /// Clase que se encarga de identificar silbidos
    /// </summary>
    public class WhistleRecognizer : SoundRecognizer
    {
        public WhistleRecognizer(EventName name, int resolution) : base(name)
        {
            _offsetFrequency = 0.05f * resolution;
            _eventDuration = 10;
        }

        /*
        * ventana deslizante: https://stackoverflow.com/questions/8269916/what-is-sliding-window-algorithm-examples
        **/

        /*
        TODO: Mejorar el reconocimiento de silbidos configurando los valores 
        de los distintos parámetros en funcion de la frecuencia a la que se silbe. 
        Actualmente se reconocen mejor los silbidos agudos que los graves
         */

        /// <summary>
        /// Identifica si ha habido un silbido/nota musical.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Para identificar el silbido/nota musical se analizan cuatro parametros. 
        /// </para>
        ///     <para>
        ///         Parametro 1: numero de picos que contiene el array. Cuantos mas picos contenga menos probabilidades hay de que sea un silbido/nota musical y viceversa.
        ///         Para ello recorremos todo el array comparando la posicion actual con la anterior y la posterior para saber cuando deja de crecer la intensidad y empieza
        ///         a decrecer.
        ///     </para>
        ///     <para>
        ///         Parametro 2: maxima amplitud de la frecuencia con mayor intensidad. Si la amplitud es grande hay mas probabilidades de que sea un silbido. Al lo
        ///         contrario, si la amplitud es baja lo mas probable es que haya sido un golpe.
        ///     </para>
        ///     <para>
        ///         Parametro 3: posicion de la frecuencia. Un silbido no llega a notas ni muy graves ni muy agudas, se queda en el centro, mas especificamente entre el 
        ///         primer cuarto y la mitad del array (1/4 - 1/2) \n{ - - [ - - - ] - - - - - } 
        ///     </para>
        ///      <para>
        ///         Para hallar los parametros 2 y 3 usamos el patron de ventana deslizante ligeramente modificado. Utilizamos dos colas de prioridad, una creciente y otra
        ///         decreciente, para ordenar las frecuencias por intensidad y poder obtener el maximo y el minimo de las frecuencias que abarca la ventana. La dimension de
        ///         la ventana es de el 10% de la longitud del array que es mas o menos lo que ocupa un pico cuando se silba o se toca una nota musical.
        ///     </para>
        ///     <para>
        ///         Parametro 4: diferencia de intensidad entre los picos maximos. Cuanta mas diferencia haya entre los picos maximos registrados mas probabilidades hay de 
        ///         que sea un silbido. 
        ///     </para>
        ///     <para>
        ///         Diferencias entre los parametros 2 y 4: el parametro 2 sirve para detectar la amplitud del pico mas pronunciado y el 4 sirve para detectar la diferencia
        ///         de amplitud entre los picos mas altos. Los valores fluctuan en de manera parecida, pero en el caso de que por ejemplo toquemos notas con una flauta 
        ///         dulce, el parametro 2 no variara mucho, devolviendo valores altos, pero en cambio el parametro 4 si que detectara los picos resonantes y devolvera un
        ///         valor un poco inferior. Donde el p2 devuelve 0.20 el p4 devuelve 0.14
        ///     </para>
        ///    
        /// </remarks>
        /// <param name="array">Array de frecuencias e intensidades que proporciona SpectrumAnalizer</param>
        /// <returns>
        /// <para>
        ///     Devuelve un float entre 0 y 1 incluidos. {0 -> ninguna coincidencia | 1 -> 100% de coincidencia}
        /// </para>
        /// <para>
        ///     El valor devuelto lo componen la suma de los cuatro factores comentados anteriormente. Cada uno de ellos aportara un valor maximo de 0.25 aproximadamente, 
        ///     por lo que la suma de los cuatro dara como maximo ~1~.
        /// </para>
        /// </returns>
        protected override float AnalizeSpectrum(float[] array)
        {

            #region factor 1: numero de picos

            // cuenta los picos que hay y despues saca el porcentaje de afinidad con el silbido (pocos picos -> muy afin con silbido)
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
            // si hay sonido (countFrecActivas > 0) -> countFrecActivas = 1 => factor = 0,333
            //                                      -> countFrecActivas = array.Length => factor = 0,000001
            // ((-x/(length/6)) + 1) * 0.33 (length/6 => ¿1 de cada 6 son picos?-> 0% afinidad)
            float factor1 = countFrecActivas == 0 ? 0 : (1 - ((float)countFrecActivas / (array.Length / 6))) * 0.25f;

            //Debug.Log("Factor 1: " + factor1);
            #endregion


            #region factor 2 y 3: diferencia de entre intensidad max-min y estudio posicion de la frecuencia

            // Factor de escala que representa la dimension de la ventana deslizante.
            // factorScaleWindow = 0.1 -> extension de la ventana es el 10% de la dimension total del array.
            float factorScaleWindow = 0.1f;
            int ancho = (int)(factorScaleWindow * array.Length);

            int i = 0;
            maxFrequency = new YonduNote(array[0], i);
            YonduNote minFrequency = new YonduNote(array[0], i);

            // cola con todas las frecuencias ordenadas de mayor a menor
            PriorityQueue<YonduNote> pqAllFeq = new PriorityQueue<YonduNote>(true);

            // cola auxiliar de maximos
            PriorityQueue<YonduNote> pqMaxs = new PriorityQueue<YonduNote>(true);

            // cola auxiliar de minimos
            PriorityQueue<YonduNote> pqMins = new PriorityQueue<YonduNote>();

            // caso base
            // buscamos las frecuencias maxima y minima de la ventana para saber cual es la diferencia
            do
            {
                pqAllFeq.Enqueue(new YonduNote(array[i], i));
                if (array[i] > maxFrequency.intensity)
                {
                    maxFrequency.intensity = array[i];
                    maxFrequency.frequency = i;
                }
                else if (array[i] < minFrequency.intensity)
                    minFrequency.intensity = array[i];
            } while (i++ < ancho);

            YonduNote maxDiff = new YonduNote(maxFrequency.intensity - minFrequency.intensity, maxFrequency.frequency);

            // metemos el maximo y el minimo en las colas de prioridad
            pqMaxs.Enqueue(maxFrequency);
            pqMins.Enqueue(minFrequency);

            // calculamos la diferencia de intensidad de todo el espectro
            //[a b c] d e f
            //a [b c d] e f
            while (i < array.Length)
            {
                pqAllFeq.Enqueue(new YonduNote(array[i], i));

                // Actualizamos la frecuencia maxima
                if (array[i] > maxFrequency.intensity)
                {
                    maxFrequency.intensity = array[i];
                    maxFrequency.frequency = i;
                }

                // si el elemento que sale es el maximo lo sacamos de la cola
                if (pqMaxs.Peek().intensity == array[i - ancho])
                    pqMaxs.Dequeue();
                // si el elemento que sale es el minimo lo sacamos de la cola
                else if (pqMins.Peek().intensity == array[i - ancho])
                    pqMins.Dequeue();

                //pqMaxs = DeleteValorFromQueue(array[i - ancho], pqMaxs);
                //pqMins = DeleteValorFromQueue(array[i - ancho], pqMins);




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


            // el valor de maxDiff oscila entre 0 y 0.8, por lo tanto 0.8 equivale a 0.25
            // hay ocasiones que el valor de dif supera 0.8 por lo que redondeamos a 0.8 para que no supere el 0.25 de valor maximo
            float factor2 = ((maxDiff.intensity > 0.8f ? 0.8f : maxDiff.intensity) / 0.8f) * 0.25f;

            //Debug.Log("Factor 2: " + factor2);



            // factor3 analiza la posicion en frecuencia del valor mas alto del array. El valor normal de un silbido esta comprendido entre 1/4 y 1/2 del array, por lo que
            // los valores entre ese rango obtendrán 0.25. Los valores que estan por encima o por debajo de ese intervalo obtienen una puntuacion entre 0.15 y 0, siendo
            // 0.15 el valor mas proximo al intervalo y 0 el valor mas alejado.
            float factor3 = 0;
            if (maxFrequency.frequency != 0)
            {
                if ((maxFrequency.frequency < array.Length * 0.6f) && (maxFrequency.frequency > array.Length * 0.2f))
                    factor3 = 0.25f;
                else if (maxFrequency.frequency > array.Length * 0.6f)
                    factor3 = ((-(maxFrequency.frequency - (0.6f * array.Length)) / array.Length) + 1) * 0.125f;
                else if (maxFrequency.frequency < array.Length * 0.2f)
                    factor3 = (maxFrequency.frequency / array.Length) * 0.125f;
            }

            //Debug.Log("Factor 3: " + factor3);
            #endregion


            #region factor 4: comparacion de los picos máximos

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
                // si las frecuencias estan demasiado juntas se desecha la menor (ancho ventana deslizante como limite minimo)
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

            // el valor de dif oscila entre 0 y 0.6, por lo tanto 0.6 equivale a 0.25
            // hay ocasiones que el valor de dif supera 0.6 por lo que redondeamos a 0.6 para que no supere el 0.25 de valor maximo
            float factor4 = ((dif > 0.6f ? 0.6f : dif) / 0.6f) * 0.25f;

            //Debug.Log("Factor 4: " + factor4);
            #endregion


            return factor1 + factor2 + factor3 + factor4;
        }

        // elimina el valor v de la cola c
        private PriorityQueue<YonduNote> DeleteValorFromQueue(float v, PriorityQueue<YonduNote> c)
        {
            bool find = false;
            PriorityQueue<YonduNote> aux = new PriorityQueue<YonduNote>();
            while (!find && c.Count > 0)
            {
                YonduNote p = c.Peek();
                if (p.intensity == v)
                {
                    c.Dequeue();
                    find = true;
                }
                else
                {
                    aux.Enqueue(p);
                    c.Dequeue();
                }
            } 

            if (aux.Count > c.Count)
            {
                while (c.Count > 0) aux.Enqueue(c.Dequeue());
                return aux;
            }
            else
            {
                while (aux.Count > 0) c.Enqueue(aux.Dequeue());
                return c;
            }
        }
    }
}