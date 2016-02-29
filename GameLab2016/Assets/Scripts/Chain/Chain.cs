using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Simoncouche.Chain {

    /// <summary>
    /// A Chain is a group of ChainSection game objects linked together, 
    /// with one Hook object at either end.
    /// </summary>
    public class Chain : MonoBehaviour {

        /// <summary>Self-reference to the chain prefab for factory purposes</summary>
        private static GameObject _chainPrefab;

		[Tooltip("The length of a single chain section")]
		[SerializeField]
		private float _chainSectionLength = 1;

        /// <summary>The first hook thrown by the player</summary>
        private Hook _beginningHook;
        
        /// <summary>The second hook thrown by the player</summary>
		private Hook _endingHook;

		/// <summary>The chain sections currently generated for visual effect</summary>
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
			
        /// <summary>
        /// Sync the number of chain sections with the current 
        /// distance between the two edges of the chain
        /// </summary>
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
			
        /// <summary>Create and configure the beginning hook</summary>
		public void CreateBeginningHook() {
			_beginningHook = Hook.Create(this, true);
		}

        /// <summary>Create and configure the ending hook</summary>
        public void CreateEndingHook() {
            _endingHook = Hook.Create(this, false);
            
            // Reroute the beginning hook from the player to the ending hook
            _beginningHook.chainJoint.connectedBody = _endingHook.rigidbody;

            // Reroute the visual chain from the player to the ending hook
            _chainSections[_chainSections.Count - 1].joint.connectedBody = _endingHook.rigidbody;

            // Set up listeners
            _endingHook.attach.AddListener(this.OnEndingHookAttach);
        }

        /// <summary>React to ending hook attaching itself to an island.</summary>
        private void OnEndingHookAttach() {
            // Clamp max distance to current distance between the two hooks
            this.SetMaxChainDistance(Vector3.Distance(
                _beginningHook.transform.position,
                _endingHook.transform.position
            ));
        }

        /// <summary>Sets the max distance that the chain can have.</summary>
        /// <param name="distance">Distance.</param>
        private void SetMaxChainDistance(float distance) {
            _beginningHook.chainJoint.distance = distance;
        }
    }
}
