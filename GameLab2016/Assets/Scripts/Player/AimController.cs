using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace Simoncouche.Controller {

    /// <summary>
    /// The Aimcontroller manages the aim system of the player which is used for different mechanics.
    /// It reads the inputs of the player and moves the Aim Indicator accordingly.
    /// Objects using the aim system need a reference to this.
    /// </summary>
    public class AimController : MonoBehaviour {

        [SerializeField]
        [Tooltip("Duration for the bounce-in animation")]
        private float _animDuration = 0.2f;

        /// <summary> The current aim orientation as set by the right analog input </summary>
        public float aimOrientation { get; private set; }
        /// <summary> The current aim orientation in vector2 as set by the right analog input </summary>
        public Vector2 aimOrientationVector2 { get; private set; }

        // COMPONENTS

        private AutoAimController _autoAimController;
        private GameObject _aimIndicator;

        // METHODS

        private void Awake() {
            _autoAimController = GetComponent<AutoAimController>();
            _aimIndicator = transform.Find("AimIndicator").gameObject;

            ToggleAimIndicator(false);
        }

        private void Update() {
            UpdateAim();

            if (_autoAimController) {
                SnapToAutoAimTarget();
            }
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

        private void SnapToAutoAimTarget() {
            float targetOrientation = _autoAimController.targetOrientation;
            if (targetOrientation != 0) {
                _aimIndicator.transform.rotation = Quaternion.Euler(0, 0, targetOrientation);
            } else {
                _aimIndicator.transform.localRotation = Quaternion.identity;
            }
        }

        /// <summary> Public method call to toggle indicator on or off from the outside </summary>
        /// <param name="active">If true, activate. Otherwise, deactivate.</param>
        public void ToggleAimIndicator(bool active) {
            if (active) ActivateIndicator();
            else DeactivateIndicator();
        }

        /// <summary>Activate indicator</summary>
        private void ActivateIndicator() {
            _aimIndicator.transform.localScale = Vector3.zero;
            _aimIndicator.SetActive(true);
            _aimIndicator.transform.DOScale(Vector3.one, _animDuration).SetEase(Ease.OutBounce);
        }

        /// <summary>Deactivate indicator</summary>
        private void DeactivateIndicator() {
            _aimIndicator.transform.DOScale(Vector3.zero, _animDuration)
                .SetEase(Ease.InBounce)
                .OnComplete(() => {
                    _aimIndicator.SetActive(false);
                });
        }
    }
}