using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Simoncouche.Chain {

    /// A Chain is a group of ChainSection game objects linked together, with one Hook object at either end.
    public class Chain : MonoBehaviour {

        /// Self-reference to the chain prefab for factory purposes
        private static GameObject _chainPrefab;

		[Tooltip("The length of a single chain section")]
		[SerializeField]
		private float _chainSectionLength = 1;

		/// The first hook thrown by the player
        private Hook _beginningHook;
        
		/// The second hook thrown by the player
		private Hook _endingHook;

		/// The chain sections currently generated for visual effect
		private List<ChainSection> _chainSections;

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

		public void Awake() {
			this._chainSections = new List<ChainSection>();
		}

		public void Start() {
			CreateBeginningHook();
		}

        public void Update() {
			RecalculateChainSections();
        }
			
		/// Sync the number of chain sections with the current distance between the two edges of the chain
		private void RecalculateChainSections() {
			Vector3 chainBeginning = _beginningHook.transform.position;
			Vector3 chainEnding = (_endingHook != null)
				? _endingHook.transform.position
				: this.thrower.transform.position;

			int neededSections = (int)(Vector3.Distance(chainBeginning, chainEnding) / _chainSectionLength);

			// Too few sections : Create more sections until we achieve the right number
			while (_chainSections.Count < neededSections) {
				if (_chainSections.Count == 0) {
					_chainSections.Add(_beginningHook.SpawnChainSection());
				} else {
					_chainSections.Add(_chainSections[_chainSections.Count - 1].SpawnNewSection());
				}
			}

			// Too many sections : Destroy the links until we achieve the right number
			while (_chainSections.Count > neededSections) {
				_chainSections[_chainSections.Count - 1].Remove();
				_chainSections.RemoveAt(_chainSections.Count - 1);
			}
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
			
		/// Create and configure the beginning hook
		public void CreateBeginningHook() {
			_beginningHook = Hook.Create(this, true);

			Debug.Log(_beginningHook.visualChainJoint.connectedBody);
		}

		/// Create and configure the ending hook
        public void CreateEndingHook() {
            _endingHook = Hook.Create(this, false);

			// Reroute the beginning hook from the player to the ending hook
			_beginningHook.chainJoint.connectedBody = _endingHook.rigidbody;

			// Reroute the visual chain from the player to the ending hook
			_chainSections[_chainSections.Count - 1].joint.connectedBody = _endingHook.rigidbody;
        }
    }
}
