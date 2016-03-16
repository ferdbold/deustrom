using UnityEngine;
using System.Collections;

namespace Simoncouche.Controller {

    /// <summary>
    /// The Aimcontroller manages the aim system of the player which is used for different mechanics.
    /// It reads the inputs of the player and moves the Aim Indicator accordingly.
    /// Objects using the aim system need a reference to this.
    /// </summary>
    public class AimController : MonoBehaviour {

        /// <summary> The current aim orientation as set by the right analog input </summary>
        public float aimOrientation { get; private set; }
        /// <summary> The current aim orientation in vector2 as set by the right analog input </summary>
        public Vector2 aimOrientationVector2 { get; private set; }

        // COMPONENTS

        private GameObject _aimIndicator;

        // METHODS

        private void Awake() {
            _aimIndicator = transform.Find("AimIndicator").gameObject;
            ToggleAimIndicator(false);
        }

        private void Update() {
            UpdateAim();
        }

        /// <summary>
        /// Update the aim of the player
        /// </summary>
        private void UpdateAim() {
            //Get orientation as a Vector2 value (x, y)
            Vector2 orientation = transform.right;
            aimOrientationVector2 = orientation;

            //Get orientation as a float value (degrees around circle)
            this.aimOrientation = Vector2.Angle(Vector2.right, orientation);
            //Correct angle if in lower quadrants
            if (orientation.y < 0) {
                this.aimOrientation = 360f - this.aimOrientation;
            }
        }

        /// <summary> Public method call to toggle indicator on or off from the outside </summary>
        /// <param name="active">If true, activate. Otherwise, deactivate.</param>
        public void ToggleAimIndicator(bool active) {
            if (active) ActivateIndicator();
            else DeactivateIndicator();
        }
        /// <summary> Deactivate Indicator </summary>
        private void DeactivateIndicator() {
            _aimIndicator.SetActive(false);
        }
        /// <summary> Activate Indicator </summary>
        private void ActivateIndicator() {
            _aimIndicator.SetActive(true);
        }
    }
}