using UnityEngine;
using System.Collections.Generic;
using Simoncouche.Controller;

namespace Simoncouche.Chain {

    /// A HookThrower controls a character's aiming and spawns hooks and chains upon user input.
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(AimController))]
	public class HookThrower : MonoBehaviour {

        enum State {
            NoHook,
            OneHook
        }

        private State _currentState;

		[Tooltip("The initial force sent to the hook upon throwing it")]
		[SerializeField]
		private float _initialForceAmount = 10f;

		/// The minimum distance needed between the thrower and a chain's last ChainSection to spawn a new ChainSection
		private float _spawnChainDistanceThreshold = 4f;
        public float spawnChainDistanceThreshold { get { return _spawnChainDistanceThreshold; } }

        /// The list of all the chains thrown by this thrower currently in play
        private List<Chain> _chains = new List<Chain>();

		// COMPONENTS
            
		public new Rigidbody2D rigidbody { get; private set; }
		public AimController aimController { get; private set; }
        public PlayerController playerController { get; private set; }
        public PlayerAudio playerAudio { get; private set; }

        public void Awake() {
			this.rigidbody = GetComponent<Rigidbody2D>();
            this.aimController = GetComponent<AimController>();
            this.playerController = GetComponent<PlayerController>();
            this.playerAudio = GetComponent<PlayerAudio>();
		}

		public void Update() {
			foreach (Chain chain in _chains) {
				chain.Update();
			}
		}

		public void SetupInput(bool isPlayerOne) {
			GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_fireHook : InputManager.Button.p2_fireHook, 
                this.Fire
            );
		}
			
		/// Handle user input to throw a new chain and hook
		private void Fire() {
			switch (_currentState) {

			// If we press fire when we don't have any hook,
			// we create a hook and switch the currentState to OneHook
			case State.NoHook:
				_chains.Add(Chain.Create(this, _initialForceAmount));
				_currentState = State.OneHook;

				// Animation handling
				playerController.HandleFirstHookAnimation();

                // Audio
                playerAudio.PlaySound(PlayerSounds.PlayerChainFirst);

				break;
			
			// If we press fire when we have 1 hook, 
			// we create a hook and switch the currentState to NoHook
			// TODO: MODIFIER POUR LIER LES 2 CHAINES
			case State.OneHook: 
				_chains[_chains.Count - 1].CreateEndingHook();
				_currentState = State.NoHook;

				// Animation handling
				playerController.HandleSecondHookAnimation();

                // Audio
                playerAudio.PlaySound(PlayerSounds.PlayerChainSecond);

				break;
            }
		}
	}
}
