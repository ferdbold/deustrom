using UnityEngine;
using System.Collections;

namespace Simoncouche.Controller {

    /// <summary>
    /// The Aimcontroller manages the aim system of the player which is used for different mechanics.
    /// It reads the inputs of the player and moves the Aim Indicator accordingly.
    /// Objects using the aim system need a reference to this.
    /// </summary>
    public class AimController : MonoBehaviour {

        [Tooltip("Input axis threshold before applying aiming")]
        [SerializeField]
        private float _aimDeadzone = 0.01f;

        /// <summary> The current aim orientation as set by the right analog input </summary>
        public float aimOrientation { get; private set; }

        /// <summary> The current aim orientation in vector2 as set by the right analog input </summary>
        public Vector2 aimOrientationVector2 { get; private set; }


        void Update() {
            UpdateAim();
        }


        /// <summary>
        /// Update the aim of the player
        /// </summary>
        private void UpdateAim() {
            Vector2 orientation = transform.right;
            aimOrientationVector2 = orientation;

            if (orientation.magnitude > _aimDeadzone) {
                this.aimOrientation = Vector2.Angle(Vector2.right, orientation);

                if (orientation.y < 0) {
                    this.aimOrientation = 360f - this.aimOrientation;
                }
            }
        }

    }

}