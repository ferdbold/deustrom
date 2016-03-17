using UnityEngine;
using System.Collections;

namespace Simoncouche.Utils {
    public class WaitWithCallback {

        public WaitWithCallback(float timeToWait, WaitWithCallBackCoroutine.WaitFunction callback) {
            GameObject wait = new GameObject();
            wait.AddComponent<WaitWithCallBackCoroutine>().Initialize(timeToWait, callback);
        }
    }
}
