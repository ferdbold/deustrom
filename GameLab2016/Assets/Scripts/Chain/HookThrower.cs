using UnityEngine;
using System.Collections.Generic;
using Simoncouche.Controller;

namespace Simoncouche.Chain {
    /// <summary>
    /// A HookThrower controls a character's aiming and spawns hooks and chains upon user input.
    /// </summary>
    [RequireComponent(typeof(SpringJoint2D))]
	[RequireComponent(typeof(AimController))]
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
		        
		public SpringJoint2D joint { get; private set; }
		public AimController aimController { get; private set; }

        // METHODS

        public void Awake() {
			this.joint = GetComponent<SpringJoint2D>();
            this.aimController = GetComponent<AimController>();
		}

		public void Start() {
			GameManager.inputManager.AddEvent(InputManager.Button.fireHook, this.Fire);
		}

		public void Update() {
			// Generate new sections if the distance to the linked section exceeds the threshold
			// TODO: Unfuck this
			if (_chains.Count > 0 && this.joint.connectedBody != null) {
				if (Vector3.Distance(transform.position, this.joint.connectedBody.position) > _spawnChainDistanceThreshold) {
					this.joint.connectedBody.GetComponent<ChainSection>().SpawnNewSection();
				}
			}
		}

		/// <summary>
		/// Handle user input to throw a new chain and hook
		/// </summary>
		private void Fire() {
			_chains.Add(Chain.Create(this, _initialForceAmount));
			this.joint.enabled = true;
		}
	}
}
