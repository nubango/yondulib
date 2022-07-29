using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YonduLib
{

    /// <summary>
    /// Clase que alberga una frecuencia y su intensidad.
    /// </summary>
    public class YonduNote : System.IComparable
    {
        /// <summary>
        /// Intensidad de la frecuencia. El valor oscila entre 0 y 1.
        /// </summary>
        public float intensity;

        /// <summary>
        /// Frecuencia en cuestion. Equivale al identificador del array que obtenemos del SpectrumAnalizer.
        /// </summary>
        public int frequency;

        public YonduNote(float i, int f)
        {
            intensity = i;
            frequency = f;
        }

        /// <summary>
        /// Compara el objeto instancia con el objeto pasado por parametro. 
        /// Ambos son tienen que ser de tipo Nota y se comparan en funcion de la intensidad. Lo usamos en el método WhistleIdentifier para poder ordenar las colas de prioridad.
        /// </summary>
        /// <param name="obj">Objeto Nota con el que queremos comparar la instancia</param>
        /// <returns>
        /// -1 si obj es mayor que la instancia
        /// 0 si obj es igual a la instancia
        /// 1 si obj es menor que la instancia
        /// </returns>
        public int CompareTo(object obj)
        {
            int val = 0;
            if (((YonduNote)obj).intensity < intensity) val = 1;
            else if (((YonduNote)obj).intensity > intensity) val = -1;

            return val;
        }
    }
}