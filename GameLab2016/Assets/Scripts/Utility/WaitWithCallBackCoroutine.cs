using UnityEngine;
using System.Collections;

namespace Simoncouche.Utils {
    public class WaitWithCallBackCoroutine : MonoBehaviour {

        public delegate void WaitFunction();

        public void Initialize(float time, WaitFunction function) {
            StartCoroutine(Waiting(time, function));
        }

        IEnumerator Waiting(float time, WaitFunction function) {
            yield return new WaitForSeconds(time);
            function.Invoke();
            Destroy(gameObject);
        }
    }
}
