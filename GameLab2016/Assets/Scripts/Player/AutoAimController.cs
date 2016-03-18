using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Simoncouche.Islands;

namespace Simoncouche.Controller {

    /// <summary>
    /// The AutoAimController can pick automatically a target island chunk for its parent game object 
    /// based on its position and rotation and highlight it to the user.
    /// </summary>
    [RequireComponent(typeof(AimController))]
    public class AutoAimController : MonoBehaviour {

        /// <summary>
        /// The update tick rate between scans in seconds.
        /// </summary>
        private static float SCAN_UPDATE_TICK = 0.1f;

        /// <summary>
        /// How much we shrink the chunk diameter when we calculate ray angle in target scans 
        /// to ensure all chunks are hit. Smaller number means more rays will be fired.
        /// </summary>
        private static float CHUNK_DIAMETER_SHRINK_FACTOR = 0.75f;

        [Tooltip("The top angle of the cone of detection starting from the player in degrees")]
        [SerializeField]
        private float _theta = 30;

        [Tooltip("The max distance of detection from the player")]
        [SerializeField]
        private float _distance = 5;

        [SerializeField]
        private Transform _aimIndicatorPrefab;

        [Header("Debug")]
        [SerializeField]
        private bool _displayScanRays = false;

        public IslandChunk target { get; private set; }

        private Coroutine _updateCoroutine;

        // COMPONENTS
        private Transform _indicator;
        private AimController _aimController;

        // METHODS

        private void Awake() {
            _indicator = transform.Find("AutoAimIndicator");
            _aimController = GetComponent<AimController>();
        }
            
        private void Update() {
            _indicator.Find("Icon").LookAt(Camera.main.transform.position);
        }

        /// <summary>
        /// Main scanning and operation loop.
        /// </summary>
        /// <returns>Coroutine</returns>
        private IEnumerator ScanUpdate() {
            while (true) {
                IslandChunk oldTarget = this.target;
                List<IslandChunk> targets = GetAllTargetsInRange();

                if (_indicator == null) {
                    RestoreIndicator();
                }

                this.target = ChooseTarget(targets);
                _indicator.gameObject.SetActive(this.target != null);

                if (this.target != oldTarget && this.target != null) {
                    SetIndicatorTo(this.target.transform);
                }

                yield return new WaitForSeconds(SCAN_UPDATE_TICK);
            }
        }

        /// <summary>
        /// Recreates the aim indicator in case it has been destroyed by its parent chunk.
        /// </summary>
        private void RestoreIndicator() {
            Debug.Log("Restoring auto aim indicator");
            _indicator = GameObject.Instantiate(_aimIndicatorPrefab);
            _indicator.parent = transform;
            _indicator.gameObject.SetActive(false);
        }

        /// <summary>
        /// Builds a list of every island chunk in current range of the player.
        /// </summary>
        /// <returns>The all targets in range.</returns>
        private List<IslandChunk> GetAllTargetsInRange() {
            List<IslandChunk> islandsInRange = new List<IslandChunk>();
            float chunkDiameter = 5; // TODO: Fetch diameter from some reliable source
            float thetaRad = _theta * Mathf.PI/180;

            // Ensure the ray angle is small enough to capture all chunks
            float rayAngle = Mathf.Atan2(chunkDiameter * CHUNK_DIAMETER_SHRINK_FACTOR, _distance);
            int raysNeeded = (int)Mathf.Ceil(thetaRad / rayAngle);

            for (var i = 0; i < raysNeeded; i++) {
                float stepRayAngle = _aimController.aimOrientation*Mathf.PI/180 - thetaRad/2 + i*rayAngle;
                Vector2 stepRayDirection = new Vector2(Mathf.Cos(stepRayAngle), Mathf.Sin(stepRayAngle)) * _distance;

                if (_displayScanRays) {
                    Debug.DrawRay(transform.position, stepRayDirection, Color.white, SCAN_UPDATE_TICK);
                }

                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, stepRayDirection);

                foreach (RaycastHit2D hit in hits) {
                    IslandChunk hitChunk = hit.transform.GetComponent<IslandChunk>();

                    if (hitChunk != null && !islandsInRange.Contains(hitChunk)) {
                        islandsInRange.Add(hitChunk);
                    }
                }
            }

            return islandsInRange;
        }

        /// <summary>
        /// Choose within a list of potential targets the one closest to the center of the player's detection cone.
        /// </summary>
        /// <returns>The target.</returns>
        /// <param name="targets">Targets.</param>
        private IslandChunk ChooseTarget(List<IslandChunk> targets) {
            float angleToBeat = _theta/2;
            IslandChunk pickedTarget = null;

            foreach (IslandChunk target in targets) {
                Vector2 toTarget = target.transform.position - transform.position;
                float targetAngle = Vector2.Angle(transform.right, toTarget);

                if (targetAngle < angleToBeat) {
                    pickedTarget = target;
                    angleToBeat = targetAngle;
                }
            }

            return pickedTarget;
        }

        /// <summary>Send the indicator to a certain target transform.</summary>
        /// <param name="targetTransform">The target transform</param>
        private void SetIndicatorTo(Transform targetTransform) {
            _indicator.parent = targetTransform;
            _indicator.localPosition = Vector3.zero;
        }

        private void OnEnable() {
            _updateCoroutine = StartCoroutine(this.ScanUpdate());
        }

        private void OnDisable() {
            _indicator.gameObject.SetActive(false);

            StopCoroutine(_updateCoroutine);
        }

        // PROPERTIES

        public float aimOrientation {
            get {
                return _aimController.aimOrientation;
            }
        }

        /// <summary>
        /// Returns the angle in degrees between Vector2.right and the vector leading to the current target. Returns 0 
        /// if there is no current target.
        /// </summary>
        public float targetOrientation {
            get {
                float orientation = 0;
                Vector2 toTarget = this.target.transform.position - transform.position;

                if (this.target != null) {
                    orientation = Vector2.Angle(Vector2.right, toTarget);

                    // Correct angle in lower quadrants
                    if (toTarget.y < 0) {
                        orientation = 360f - orientation;
                    }
                }

                return orientation;
            }
        }
    }
}