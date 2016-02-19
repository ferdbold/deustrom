using UnityEngine;
using System.Collections;

namespace Simoncouche.Utils {
    public class Utils : MonoBehaviour {
        #region Timer 
        /// <summary>
        /// Start a timer and calls the callback function when it's completed
        /// </summary>
        /// <param name="time">The time before the timer ends</param>
        /// <param name="callback">The function callback at the end </param>
        public static void StartTimer(float time, TimerCallback callback) {
            (new MonoBehaviour()).StartCoroutine(TimerCoroutine(time, callback)); 
        }

        static IEnumerator TimerCoroutine(float time, TimerCallback callback) {
            yield return new WaitForSeconds(time);
            callback.Invoke();
        }

        public delegate void TimerCallback();
        #endregion
    }
}
