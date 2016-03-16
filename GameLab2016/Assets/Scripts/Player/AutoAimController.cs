using UnityEngine;
using System.Collections;

namespace Simoncouche.Controller {

    /// <summary>
    /// The AutoAimController can pick automatically a target for its parent game object 
    /// based on its position and rotation and highlight it to the user.
    /// </summary>
    public class AutoAimController : MonoBehaviour {

        [Tooltip("The top angle of the cone of detection starting from the player in degrees")]
        [SerializeField]
        private float theta;

        [Tooltip("The max distance of detection from the player")]
        [SerializeField]
        private float distance;

        // COMPONENTS
        private ParticleSystem _targetParticles;

        // METHODS

        private void Awake() {
            _targetParticles = transform.Find("AutoAimIndicator").GetComponent<ParticleSystem>();
        }

        /// <summary>Activate the auto aim behaviour</summary>
        public void Activate() {

        }

        /// <summary>Deactivate the auto aim behaviour</summary>
        public void Deactivate() {

        }
    }
}