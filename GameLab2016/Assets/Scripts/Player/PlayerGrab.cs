using UnityEngine;
using System.Collections;

namespace Simoncouche.Controller {
    public class PlayerGrab : MonoBehaviour {

        /// <summary> Currently Grabbed GravityBody</summary>
        private GravityBody _grabbedBody = null;

        private PlayerController _controller;

        void Awake() {
            _controller = GetComponent<PlayerController>();
        }

        void Update() {
            if (_grabbedBody != null && Input.GetKeyDown(KeyCode.F)) {
                Throw();
            }
        }
        
        private void Throw() {

        }
    }
}
