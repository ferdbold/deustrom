using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Simoncouche.Chain {

    /// A Chain is a group of ChainSection game objects linked together, with one Hook object at either end.
    public class Chain : MonoBehaviour {

        /// Self-reference to the chain prefab for factory purposes
        private static GameObject _chainPrefab;

		/// The first hook thrown by the player
        private Hook _beginningHook;
        
		/// The second hook thrown by the player
		private Hook _endingHook;

		/// The chain sections currently generated for visual effect
		private List<ChainSection> _chainSections;

        // [Tooltip("Maximum number of links per chain")]
        // [SerializeField]
        // private int _maximumLinksPerChain = 30;

        public HookThrower thrower { get; set; }
		public float initialForce { get; set; }

		/// <summary>Spawn a new chain in the scene</summary>
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
			CreateBeginningHook();
		}

        public void Update() {
            /*if (Vector3.Distance(transform.position, thrower.joint.connectedBody.position) > thrower.spawnChainDistanceThreshold) {
                if (_beginningHookLinkCount < _maximumLinksPerChain) {
                    thrower.joint.connectedBody.GetComponent<ChainSection>().SpawnNewSection();
                    _beginningLink = thrower.joint.connectedBody.GetComponent<ChainSection>();
                    _beginningHookLinkCount++;
                }
                //ClampDistanceWithPlayerPos(thrower.transform, _maximumDistanceBetweenPlayer, _endingLinkBeginningHook); //We clamp the distance of the closet link to the player to a maximum distance from the player
            }

            if (Vector3.Distance(transform.position, thrower.joint.connectedBody.position) > thrower.spawnChainDistanceThreshold) {
                if (_endingHookLinkCount < _maximumLinksPerChain) {
                    thrower.joint.connectedBody.GetComponent<ChainSection>().SpawnNewSection();
                    _endingLink = thrower.joint.connectedBody.GetComponent<ChainSection>();
                    _endingHookLinkCount++;
                }
            }*/
        }

        /// <summary>Restrain the closest link of a chain with a maxDistance from the thrower's position</summary>
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

		public void CreateBeginningHook() {
			_beginningHook = Hook.Create(this, true);
			_beginningHook.chainJoint.connectedBody = this.thrower.rigidbody;
		}

        public void CreateSecondHook() {
            _endingHook = Hook.Create(this, false);
			_beginningHook.chainJoint.connectedBody = _endingHook.rigidbody;
        }
    }
}
