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

        enum State
        {
            NoHook,
            OneHook,
            TwoHook
        }

        private State _currentState;

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

        public float spawnChainDistanceThreshold { get { return _spawnChainDistanceThreshold; }}

        /// <summary>
        /// The list of all the chains thrown by this thrower currently in play
        /// </summary>
        private List<Chain> _chains = new List<Chain>();


        [Tooltip("The distance the first hook is in front of the player")]
        [SerializeField]
        private float distanceHookInFrontOfPlayer = 3f;

		// COMPONENTS
		        
		public SpringJoint2D joint { get; private set; }
		public AimController aimController { get; private set; }

  
 		private Transform _aimIndicator;

        public void Awake() {
			this.joint = GetComponent<SpringJoint2D>();
            this.aimController = GetComponent<AimController>();
		}

		public void SetupInput(bool isPlayerOne) {
			GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_fireHook : InputManager.Button.p2_fireHook, 
                this.Fire
            );
		}

		/// <summary>
		/// Handle user input to throw a new chain and hook
		/// </summary>
		private void Fire() {
            if (_currentState == State.NoHook) //If we press fire when we don't have any hook, we create a hook and switch the currentState to OneHook
            {
                _chains.Add(Chain.Create(this, _initialForceAmount));
                joint.enabled = true;
                _currentState = State.OneHook;
            }
            else if(_currentState == State.OneHook) //If we press fire when we have 1 hook, we create a hook and switch the currentState to NoHook
            {
                _chains[_chains.Count - 1].CreateSecondHook();
                _currentState = State.NoHook;
                //_currentState = State.TwoHook;
            }
            else if(_currentState == State.TwoHook)
            {
                _currentState = State.NoHook;
            }
			
		}
	}
}
