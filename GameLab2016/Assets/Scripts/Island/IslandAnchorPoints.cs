using UnityEngine;
using System.Collections;

namespace Simoncouche.Islands {
    public class IslandAnchorPoints : MonoBehaviour {

        /// <summary> The associated angle of this anchor point </summary>
        public float angle { get; private set; }

        /// <summary> The local position of the anchor points </summary>
        public Vector3 position {
            get {
                return transform.localPosition;
            }
        }

        private IslandChunk _parentRef;

        void Start() {
            _parentRef = GetComponentInParent<IslandChunk>();
        }

        public void Setup(float _angle) {
            angle = _angle;
        }

        /// <summary>
        /// Get the island connected to this anchor
        /// </summary>
        /// <returns>Returns an ISland chunk or null if no connection</returns>
        public IslandChunk GetConnectedIsland() {
            Collider2D[] others = Physics2D.OverlapCircleAll(transform.position, 0.5f);
            foreach (Collider2D other in others) {
                IslandChunk chunk = other.GetComponent<IslandChunk>();
                if (chunk != null && chunk != _parentRef) {
                    return chunk;
                }
            }
            return null;
        }

        void OnTriggerEnter2D(Collider2D other) {
            _parentRef.HandleAnchorPointCollision(other, this);
        }

        /// Returns the IslandChunk component related to this anchor point
        public IslandChunk GetIslandChunk() {
            IslandChunk chunk = transform.parent.parent.GetComponent<IslandChunk>();

            UnityEngine.Assertions.Assert.IsNotNull(chunk, "Island chunk component could not be found");

            return chunk;
        }
    }
}
