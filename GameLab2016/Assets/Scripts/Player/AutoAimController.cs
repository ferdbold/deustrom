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

        [Header("Debug")]
        [SerializeField]
        private bool _displayScanRays = false;

        private IslandChunk _target;

        // COMPONENTS
        private Transform _indicator;
        private AimController _aimController;

        // METHODS

        private void Awake() {
            _indicator = transform.Find("AutoAimIndicator");
            _aimController = GetComponent<AimController>();
        }

        private IEnumerator ScanUpdate() {
            while (true) {
                IslandChunk oldTarget = _target;
                List<IslandChunk> targets = GetAllTargetsInRange();

                _target = ChooseTarget(targets);

                if (_target != oldTarget && _target != null) {
                    SetIndicatorTo(_target.transform);
                }

                _indicator.Find("Icon").LookAt(Camera.main.transform.position);
                yield return new WaitForSeconds(SCAN_UPDATE_TICK);
            }
        }

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

            if (gameObject.name == "Sobek") Debug.Log(islandsInRange.Count);
            return islandsInRange;
        }

        private IslandChunk ChooseTarget(List<IslandChunk> targets) {
            float distanceToBeat = float.PositiveInfinity;
            IslandChunk pickedTarget = null;

            foreach (IslandChunk target in targets) {
                float distance = Vector2.Distance(transform.position, target.transform.position);
                if (distance < distanceToBeat) {
                    pickedTarget = target;
                    distanceToBeat = distance;
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
            _indicator.gameObject.SetActive(true);

            StartCoroutine(this.ScanUpdate());
        }

        private void OnDisable() {
            // Take back the indicator and disable it
            _indicator.parent = transform;
            _indicator.gameObject.SetActive(false);

            StopCoroutine(this.ScanUpdate());
        }

        // PROPERTIES

        public float aimOrientation {
            get {
                return _aimController.aimOrientation;
            }
        }
    }
}