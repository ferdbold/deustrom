using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Simoncouche.Controller;

namespace Simoncouche.Chain {

    /// <summary>A HookThrower controls a character's aiming and spawns hooks and chains upon user input.</summary>
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(AimController))]
	public class HookThrower : MonoBehaviour {

        enum State { NoHook, OneHook, Waiting}

        private State _currentState;

        [Header("Hook throw properties:")]
		[Tooltip("The initial force sent to the hook upon throwing it")]
		[SerializeField]
		private float _initialForceAmount = 10f;

        [Header("Hook retraction properties:")]
        [Tooltip("The retracted distance in each tick of the retraction")]
        [SerializeField]
        private float _distanceRetractionValue = 1.0f;

        [Tooltip("The time between each tick of retraction of the chains")]
        [SerializeField]
        private float _timeBetweenChainLengthRetraction = 0.5f;

        /// <summary>
        /// The minimum distance needed between the thrower and a chain's 
        /// last ChainSection to spawn a new ChainSection
        /// </summary>
		private float _spawnChainDistanceThreshold = 4f;
        public float spawnChainDistanceThreshold { get { return _spawnChainDistanceThreshold; } }

        /// <summary>The list of all the chains thrown by this thrower currently in play</summary>
        private List<Chain> _chains = new List<Chain>();

		// COMPONENTS
        /// <summary>The kinematic rigidbody to hook the visual chain to during OneHook state</summary>
        public Rigidbody2D chainLinker { get; private set; }        
		public new Rigidbody2D rigidbody { get; private set; }
		public AimController aimController { get; private set; }
        public PlayerController playerController { get; private set; }
        public PlayerAudio playerAudio { get; private set; }

        //PROPERTIES
        private bool _triggerIsHeld = false;
        private bool _retractButtonIsHeld = false;


        public void Awake() {
			this.rigidbody = GetComponent<Rigidbody2D>();
            this.aimController = GetComponent<AimController>();
            this.playerController = GetComponent<PlayerController>();
            this.playerAudio = GetComponent<PlayerAudio>();

            this.chainLinker = transform.Find("ChainLinker").GetComponent<Rigidbody2D>();
		}

		public void SetupInput(bool isPlayerOne) {
			GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Axis.p1_leftTrigger : InputManager.Axis.p2_leftTrigger, 
                this.CheckPlayerInputs
            );

            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_retractHooksButtonDown : InputManager.Button.p2_retractHooksButtonDown,
                this.RetractChainsEngaged
            );

            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_retractHooksButtonUp : InputManager.Button.p2_retractHooksButtonUp,
                this.RetractChainsReleased
            );



            //For keyboard use
            #if UNITY_EDITOR
            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_fireHook : InputManager.Button.p2_fireHook,
                this.Fire
            );
            #endif
        }
			
        /// <summary>Handle user input to throw a new chain and hook</summary>
		private void Fire() {
            if (playerController.InRespawnState == true) return; //Deactivate hook if currently respawning

			switch (_currentState) {
			// If we press fire when we don't have any hook,
			// we create a hook and switch the currentState to OneHook
			case State.NoHook:
				_chains.Add(Chain.Create(this, _initialForceAmount));
                _currentState = State.Waiting;

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
                _currentState = State.Waiting;

				// Animation handling
				playerController.HandleSecondHookAnimation();

                // Audio
                playerAudio.PlaySound(PlayerSounds.PlayerChainSecond);

				break;
            }
		}


        private void CheckPlayerInputs(params float[] input) {
            if (playerController.InRespawnState == true) return; //Deactivate hook if currently respawning
            bool isCurrentlyHeld = (input[0] == 1);

            if(_triggerIsHeld && !isCurrentlyHeld) { //If just stop pressing
                _triggerIsHeld = false;
                Fire();
                //animation
                aimController.ToggleAimIndicator(false);
                playerController.HandleAimStopAnimation();
            } else if(!_triggerIsHeld && isCurrentlyHeld) {//If just started pressing
                _triggerIsHeld = true;
                //animation
                aimController.ToggleAimIndicator(true);
                playerController.HandleAimStartAnimation();

            }
        }

        private void RetractChainsEngaged() {

            if (!_retractButtonIsHeld) { //If just stop pressing
                _retractButtonIsHeld = true;
                StartCoroutine(RetractChains(_timeBetweenChainLengthRetraction));
            }
        }

        private void RetractChainsReleased() {
            _retractButtonIsHeld = false;
            StopCoroutine("RetractChains");
        }

        IEnumerator RetractChains(float time) {

            while (_retractButtonIsHeld) {
                if (_chains.Count > 0) {
                    if(_chains[0]._endingHookIsSet) playerAudio.PlaySound(PlayerSounds.PlayerRetractChains);
                    foreach (Chain chain in _chains) {
                        chain.RetractChain(_distanceRetractionValue);
                    }
                    yield return new WaitForSeconds(time);
                }
            }
            
        }


        /// <summary>
        /// Function called by a chain when the first throw missed
        /// Here we remove the current chain from the list and set back the state to NoHook
        /// </summary>
        public void BeginningHookMissed() {
            _chains.RemoveAt(_chains.Count - 1);
            _currentState = State.NoHook;
            playerController.HandleSecondHookAnimation();
        }

        /// <summary>
        /// Function called by a chain when the first throw missed
        /// In case we miss our second throw, we set back our state to OneHook
        /// </summary>
        public void EndingHookMissed() {
            _currentState = State.OneHook;
            playerController.HandleSecondHookAnimation();
        }

        /// <summary>
        /// Function called to update the state of our hookthrower when the beginning hook hit something
        /// </summary>
        public void BeginningHookHit() {
            _currentState = State.OneHook;
        }

        /// <summary>
        /// Function called to update the state of our hookthrower when the ending hook hit something
        /// </summary>
        public void EndingHookHit() {
            _currentState = State.NoHook;
        }
	}
}
