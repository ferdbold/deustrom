using UnityEngine;
using System.Collections;

namespace Simoncouche.Chain {

    /// <summary>
    /// A Chain is a group of ChainSection game objects linked together, with one Hook object at either end.
    /// </summary>
    public class Chain : MonoBehaviour {

        /// <summary>
        /// Self-reference to the chain prefab for factory purposes
        /// </summary>
        private static GameObject _chainPrefab;

        private Hook _beginningHook;
        
		/// <summary>
        /// The section directly linked to the beginning hook
        /// </summary>
		private ChainSection _beginningLink = null;

		private Hook _endingHook;

		/// <summary>
		/// The section directly linked to the ending hook
		/// </summary>
        private ChainSection _endingLink = null;

        /// <summary>
        /// The current number of links in the beginning hook
        /// </summary>
        private int _beginningHookLinkCount;

        /// <summary>
        /// The current number of links in the ending hook
        /// </summary>
        private int _endingHookLinkCount;

        /// <summary>
        /// The current aim orientation as set by the right analog input
        /// </summary>
        [Tooltip("Maximum number of links per chain")]
        [SerializeField]
        private int _maximumLinksPerChain = 30;

		private int _linkCount = 0;

        public HookThrower thrower { get; set; }
		public float initialForce { get; set; }

		/// <summary>
		/// Spawn a new chain in the scene
		/// </summary>
		/// <param name="thrower">The game object that threw this chain</param>
		/// <param name="initialForce">The initial force to give to the first hook</param>
		public static Chain Create(HookThrower thrower, float initialForce) {
			if (_chainPrefab == null) {
				_chainPrefab = Resources.Load("Chain/Chain") as GameObject;
			}

			Chain chain = ((GameObject)Instantiate(
				_chainPrefab, 
				Vector3.zero, 
				Quaternion.identity
			)).GetComponent<Chain>();

			chain.thrower = thrower;
			chain.initialForce = initialForce;

			return chain;
		}

		public void Start() {
			_beginningHook = Hook.Create(this);
		}

        public void Update() {
			// If we need to spawn an additional chain section
            if (Vector3.Distance(transform.position, thrower.joint.connectedBody.position) > thrower.spawnChainDistanceThreshold) {
                if (_beginningHookLinkCount < _maximumLinksPerChain) {
                    thrower.joint.connectedBody.GetComponent<ChainSection>().SpawnNewSection();
                    _beginningLink = thrower.joint.connectedBody.GetComponent<ChainSection>();
                    _beginningHookLinkCount++;
                }

				if (_endingHookLinkCount < _maximumLinksPerChain) {
					thrower.joint.connectedBody.GetComponent<ChainSection>().SpawnNewSection();
					_endingLink = thrower.joint.connectedBody.GetComponent<ChainSection>();
					_endingHookLinkCount++;
				}
            }
        }

        /// <summary>
        /// Restrain the closest link of a chain with a maxDistance from the thrower's position
        /// </summary>
        /// <param name="throwerPosition"></param>
        /// <param name="maxDistance"></param>
        /// <param name="chainSection"></param>
        private void ClampDistanceWithPlayerPos(Transform throwerPosition, float maxDistance, ChainSection chainSection) {
            float currentDistance = Vector3.Distance(chainSection.GetComponent<Rigidbody2D>().position, throwerPosition.position);
            if (currentDistance > maxDistance) {
                Vector2 vect = throwerPosition.position - (Vector3) chainSection.GetComponent<Rigidbody2D>().position;
                vect = vect.normalized;
                vect *= (currentDistance - maxDistance);
                chainSection.GetComponent<Rigidbody2D>().position +=  vect;
            }
        }

		/// <summary>
		/// Raises the chain maximum length event.
		/// </summary>
		private void OnChainMaximumLength() {
			if (_beginningHook.joint.connectedBody == null) {
				DestroyChain();
			}
		}

		private void DestroyChain() {
			this.thrower.OnDestroyChain();
			Destroy(gameObject);
		}

        public void CreateSecondHook() {
            _endingHook = Hook.Create(this);
        }

		public int linkCount {
			get {
				return _linkCount;
			}
			set {
				if (_linkCount < _maximumLinksPerChain) {
					_linkCount = value;

					if (_linkCount == _maximumLinksPerChain) {
						OnChainMaximumLength();
					}
				}
			}
		}
    }
}
