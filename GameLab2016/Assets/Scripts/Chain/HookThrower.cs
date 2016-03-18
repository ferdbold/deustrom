using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Simoncouche.Controller;

namespace Simoncouche.Chain {

    /// <summary>A HookThrower controls a character's aiming and spawns hooks and chains upon user input.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(AimController))]
    [RequireComponent(typeof(AutoAimController))]
    [RequireComponent(typeof(AudioSource))]
    public class HookThrower : MonoBehaviour {

        enum State { NoHook, OneHook, Waiting }

        private State _currentState;

        [Header("Hook throw properties:")]
        [Tooltip("The initial force sent to the hook upon throwing it")]
        [SerializeField]
        private float _initialForceAmount = 10f;

        [Tooltip("Minimum time in seconds between each throw action")]
        [SerializeField]
        private float _throwCooldown = 1.0f;

        [Header("Hook retraction properties:")]
        [Tooltip("The retracted distance in each tick of the retraction")]
        [SerializeField]
        private float _distanceRetractionValue = 1.0f;

        [Tooltip("The time between each tick of retraction of the chains")]
        [SerializeField]
        private float _timeBetweenChainLengthRetraction = 0.5f;

        [Tooltip("Check this to replace hook already present on an island by new hook")]
        [SerializeField]
        private bool _doesHookReplacePresentHookOnIsland = false;

        /// <summary>
        /// The minimum distance needed between the thrower and a chain's 
        /// last ChainSection to spawn a new ChainSection
        /// </summary>
        private float _spawnChainDistanceThreshold = 4f;
        public float spawnChainDistanceThreshold { get { return _spawnChainDistanceThreshold; } }

        /// <summary>The list of all the chains thrown by this thrower currently in play</summary>
        private List<Chain> _chains = new List<Chain>();

        /// <summary>Remaining time before the player can attempt a throw again</summary>
        private float _throwCooldownRemaining;

        // COMPONENTS
        /// <summary>The kinematic rigidbody to hook the visual chain to during OneHook state</summary>
        public Rigidbody2D chainLinker { get; private set; }
        public new Rigidbody2D rigidbody { get; private set; }
        public AimController aimController { get; private set; }
        public AutoAimController autoAimController { get; private set; }
        public PlayerController playerController { get; private set; }
        public PlayerAudio playerAudio { get; private set; }

        // In order to know if we can throw a hook
        public PlayerGrab playerGrab { get; private set; }
        public bool isHookAttachedToPlayer { get; private set; }

        public bool isPlayerOne { get; private set; }

        // PROPERTIES
        private bool _triggerIsHeld = false;
        private bool _isRetracting = false;

        public void Awake() {
            this.rigidbody = GetComponent<Rigidbody2D>();
            this.aimController = GetComponent<AimController>();
            this.autoAimController = GetComponent<AutoAimController>();
            this.playerController = GetComponent<PlayerController>();
            this.playerAudio = GetComponent<PlayerAudio>();
            playerGrab = GetComponent<PlayerGrab>();

            this.chainLinker = transform.Find("ChainLinker").GetComponent<Rigidbody2D>();
        }

        public void SetupInput(bool isPlayerOne) {
            this.isPlayerOne = isPlayerOne;
            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Axis.p1_rightTrigger : InputManager.Axis.p2_rightTrigger,
                this.CheckPlayerInputs
            );

            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_retractHooksButtonDown : InputManager.Button.p2_retractHooksButtonDown,
                this.RetractChainsEngaged
            );

            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Axis.p1_leftTrigger : InputManager.Axis.p2_leftTrigger,
                this.RetractLeftTrigger
            );

            /* DEPRECATED: USED WHEN RETRACTS WAS WHILE BUTTON PRESSED
            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_retractHooksButtonUp : InputManager.Button.p2_retractHooksButtonUp,
                this.RetractChainsReleased
            );*/

            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_cutLinkWithChainButton : InputManager.Button.p2_cutLinkWithChainButton,
                this.CutChainLinkWithThrower
                );

            //For keyboard use
#if UNITY_EDITOR
            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_fireHookDown : InputManager.Button.p2_fireHookDown,
                this.OnDebugFireDown
            );

            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_fireHookUp : InputManager.Button.p2_fireHookUp,
                this.OnDebugFireUp
            );
#endif
        }

        private void Update() {
            _throwCooldownRemaining = Mathf.Max(0, _throwCooldownRemaining - Time.deltaTime);
        }

        private void OnDebugFireDown() {
            this.autoAimController.enabled = true;
        }

        private void OnDebugFireUp() {
            this.autoAimController.enabled = false;
            this.Fire();
        }

        /// <summary>Handle user input to throw a new chain and hook</summary>
        private void Fire() {
            // Exit early if currently respawning
            if (playerController.InRespawnState) return;

            // Exit early if currently in cooldown
            if (_throwCooldownRemaining > 0) {
                this.playerAudio.PlaySound(PlayerSounds.PlayerCooldown);
                return;
            }

            // Exit early if the player is currently grabbing an island
            if (playerGrab.grabbedBody != null) {
                return;
            }

            float orientation = this.aimController.aimOrientation;
            if (this.autoAimController.target != null) {
                orientation = this.autoAimController.targetOrientation;
            }

            switch (_currentState) {
            // If we press fire when we don't have any hook,
            // we create a hook and switch the currentState to OneHook
            case State.NoHook:
                _chains.Add(Chain.Create(this, _initialForceAmount, orientation));
                _currentState = State.Waiting;

                // Animation handling
                playerController.HandleFirstHookAnimation();

                // Audio
                playerAudio.PlaySound(PlayerSounds.PlayerChainFirst);

                break;

            // If we press fire when we have 1 hook, 
            // we create a hook and switch the currentState to NoHook
            case State.OneHook: 
                _chains[_chains.Count - 1].CreateEndingHook(orientation);
                _currentState = State.Waiting;

                // Animation handling
                playerController.HandleSecondHookAnimation();

                // Audio
                playerAudio.PlaySound(PlayerSounds.PlayerChainSecond);

                break;
            }

            // Apply cooldown
            _throwCooldownRemaining = _throwCooldown;
        }

        private void CheckPlayerInputs(params float[] input) {
            if (playerController.InRespawnState == true) return; //Deactivate hook if currently respawning
            bool isCurrentlyHeld = (input[0] == 1);

            if (_triggerIsHeld && !isCurrentlyHeld) { //If just stop pressing
                _triggerIsHeld = false;
                Fire();
                //animation
                this.autoAimController.enabled = false;
                playerController.HandleAimStopAnimation();
            } else if (!_triggerIsHeld && isCurrentlyHeld) {//If just started pressing
                _triggerIsHeld = true;
                //animation
                this.autoAimController.enabled = true;
                playerController.HandleAimStartAnimation();
            }
        }

        /// <summary>
        /// For Debug Purpose to for input with left trigger
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void RetractLeftTrigger(params float[] a) {
            if (a[0] > 0) {
                RetractChainsEngaged();
            }
        }

        /// <summary>
        /// This retracts the chains using by starting a coroutine
        /// </summary>
        private void RetractChainsEngaged() {
            if (!_isRetracting) { //If just stop pressing
                _isRetracting = true;
                StartCoroutine(RetractChains(_timeBetweenChainLengthRetraction));
            }
        }
        /// <summary>
        /// DEPRECATED
        /// This put a stop to the retraction of our chains by stoping the coroutine
        /// </summary>
        private void RetractChainsReleased() {
            StopCoroutine("RetractChains");
            if (_chains.Count > 0) _chains[_chains.Count - 1].RetractChainReleaseBehaviour();
        }

        /// <summary>
        /// If the current chain is attached to the player, we cut the link with the player using this function
        /// </summary>
        private void CutChainLinkWithThrower() {
            if (_chains.Count > 0) {
                _chains[_chains.Count - 1].CutLinkBeginningHook();
                this.isHookAttachedToPlayer = false;
            }
        }

        /// <summary>
        /// Retracts all the chains of the player and checks if the connected hook to the player is destroyed (cause too clause),
        /// we attach the island to the player then cut the player link to the hook
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        IEnumerator RetractChains(float time) {
            while (_chains.Count > 0) {
                bool mustPlaySound = false;
                for (int i = _chains.Count - 1; i >= 0; i--) {
                    if (_chains[i]._beginningHookIsSet) {
                        mustPlaySound = true; //Parce qu'on ne rétracte pas les chaînes qui viennent tout juste d'être lancé
                        GravityBody mustAttachedToBody = _chains[i].RetractChain(_distanceRetractionValue);
                        if (mustAttachedToBody != null) {
                            this.OnCutLinkWithPlayer();
                            this.AttachTouchedChunkToPlayer(mustAttachedToBody);
                            _chains[i].CutLinkBeginningHook();
                        }
                    }
                }
                if (mustPlaySound) playerAudio.PlaySound(PlayerSounds.PlayerRetractChains);
                yield return new WaitForSeconds(time);
            }
            _isRetracting = false;
        }

        /// <summary>
        /// Function called by a chain when the first throw missed
        /// Here we remove the current chain from the list and set back the state to NoHook
        /// </summary>
        public void OnBeginningHookMissed() {
            _currentState = State.NoHook;
            playerController.HandleSecondHookAnimation();
        }

        /// <summary>
        /// Function called by a chain when the first throw missed
        /// In case we miss our second throw, we set back our state to OneHook
        /// </summary>
        public void OnEndingHookMissed() {
            _currentState = State.OneHook;
            playerController.HandleFirstHookAnimation();
        }

        /// <summary>
        /// Function called to update the state of our hookthrower when the beginning hook hit something
        /// </summary>
        public void OnBeginningHookHit() {
            _currentState = State.OneHook;
            this.isHookAttachedToPlayer = true;
        }

        /// <summary>
        /// Function called to update the state of our hookthrower when the ending hook hit something
        /// </summary>
        public void OnEndingHookHit() {
            _currentState = State.NoHook;
            this.isHookAttachedToPlayer = false;
        }

        private void OnCutLinkWithPlayer() {
            _currentState = State.NoHook;
            this.isHookAttachedToPlayer = false;
        }

        /// <summary>
        /// This function is called in the hookThrower in order to remove chains that are destroyed
        /// </summary>
        /// <param name="chain"></param>
        public void RemoveChainFromChains(Chain chain) {
            if (!chain._endingHookIsSet) { //IF SO, it means that the chain being destroyed is the one attached to the player
                this.OnCutLinkWithPlayer();
                playerController.HandleSecondHookAnimation();
            }
            _chains.Remove(chain);
        }

        /// <summary>
        /// Check if there is already a hook on an island or a chunk.  If so, we destroy the precedent chain which posses the hook in question.
        /// </summary>
        public void HookAlreadyOnIslandCheck(Islands.IslandAnchorPoints anchorPoint) {
            if (_chains.Count > 1 && _doesHookReplacePresentHookOnIsland) {
                bool mustDestroyChain = false;
                int i = 0;

                while (i < _chains.Count - 1 && !mustDestroyChain) { //Count-1 cause we dont have to check the added hook 
                    if (_chains[i].CheckAnchorPointInHooks(anchorPoint)) {
                        mustDestroyChain = true;
                    }
                    if (!mustDestroyChain) i++;
                }

                if (mustDestroyChain) {
                    _chains[i].DestroyChain(true);
                }
            }
        }

        /// <summary>
        /// Removes the chain if the player enters the maelstrom AND also prevents the player from shooting while exiting the maelstrom
        /// </summary>
        public void RemoveChainOnPlayerMaelstromEnter() {
            if (_chains.Count > 0) {
                if (_chains[_chains.Count - 1] != null && _currentState == State.OneHook) {
                    _chains[_chains.Count - 1].DestroyChain(true);
                    this.OnCutLinkWithPlayer();
                }
            }

            _triggerIsHeld = false;
            playerController.HandleAimStopAnimation();
        }

        /// <summary>
        /// Attach beginning hook's target to the player grab
        /// </summary>
        private void AttachTouchedChunkToPlayer(GravityBody gBody) {
            Vector3 vectorPlayerChunk = Vector3.Normalize(gBody.transform.position - this.transform.position);
            Debug.Log("VectorPlayerChunk" + vectorPlayerChunk);
            float angle = Vector3.Angle(this.transform.right, vectorPlayerChunk);
            Debug.Log("Angle variation" + angle);
            
            this.rigidbody.transform.Rotate(this.transform.forward, angle, Space.Self);
            //this.rigidbody.transform.rotation = Quaternion.FromToRotation(transform.forward , vectorPlayerChunk) * this.rigidbody.transform.rotation;
            this.playerGrab.AttemptGrabOnHookRetraction(gBody); //MAKES THE PLAYER GRAB THE ISLAND
        }


    }
}
