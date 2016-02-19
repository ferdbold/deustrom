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

        /// <summary> Transform of the aim object on screen </summary>
        private Transform _aimIndicator;


        void Awake() {
            _aimIndicator = transform.Find("AimIndicator");
        }

        public void SetupInput(bool isPlayerOne) {
            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Axis.p1_rightAnalog : InputManager.Axis.p2_rightAnalog,
                this.Aim
            );
        }

        void Update() {
            // Apply rotation continously to the aimIndicator to prevent character rotation from updating the indicator
            _aimIndicator.transform.rotation = Quaternion.Euler(0, 0, this.aimOrientation);
        }

        /// <summary>
		/// Handle user input to update the aim indicator
		/// </summary>
		/// <param name="axisValues">Axis values.</param>
		private void Aim(float[] axisValues) {
            Vector2 orientation = new Vector2(axisValues[0], axisValues[1]);
            aimOrientationVector2 = orientation;

            // Only apply aiming if the user input is relevant (higher than the deadzone)
            if (orientation.magnitude > _aimDeadzone) {
                this.aimOrientation = Vector2.Angle(Vector2.right, orientation);

                if (axisValues[1] < 0) {
                    this.aimOrientation = 360f - this.aimOrientation;
                }
            }
        }

    }

}