using UnityEngine;
using System.Collections.Generic;
using Simoncouche.Controller;

namespace Simoncouche.Chain {



    /// <summary>
    /// A HookThrower controls a character's aiming and spawns hooks and chains upon user input.
    /// </summary>
    [RequireComponent(typeof(SpringJoint2D))]
	public class HookThrower : MonoBehaviour {

		[Tooltip("Reference to the grappling hook prefab")]
		[SerializeField]
		private Hook _hookPrefab;

		[Tooltip("The initial force sent to the hook upon throwing it")]
		[SerializeField]
		private float _initialForceAmount = 10f;

		/// <summary>
		/// The minimum distance needed between the thrower and a chain's last ChainSection to spawn a new ChainSection
		/// </summary>
		private float _spawnChainDistanceThreshold = 4f;

		/// <summary>
		/// The list of all the chains thrown by this thrower currently in play
		/// </summary>
		private List<Chain> _chains = new List<Chain>();

		// COMPONENTS

		private SpringJoint2D _joint;
		public SpringJoint2D joint { get { return _joint; } }
        
        private AimController _aimController;

        //Getters & Setters
        public float AimOrientation() { return _aimController.aimOrientation; }



        // METHODS

        public void Awake() {
			_joint = GetComponent<SpringJoint2D>();
            _aimController = GetComponent<AimController>();

			if (_aimController == null) {
				Debug.LogError("Player/AimController cannot be found!");
			}
		}

		public void Start() {
			GameManager.inputManager.AddEvent(InputManager.Button.fireHook, this.Fire);
		}

		public void Update() {
			// Generate new sections if the distance to the linked section exceeds the threshold
			// TODO: Unfuck this
			if (_chains.Count > 0 && _joint.connectedBody != null) {
				if (Vector3.Distance(transform.position, _joint.connectedBody.position) > _spawnChainDistanceThreshold) {
					_joint.connectedBody.GetComponent<ChainSection>().SpawnNewSection();
				}
			}

			
		}

		/// <summary>
		/// Handle user input to throw a new chain and hook
		/// </summary>
		private void Fire() {
			_chains.Add(Chain.Create(this, _initialForceAmount));
			_joint.enabled = true;
		}

		
	}
}
