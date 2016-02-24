﻿using UnityEngine;
using System.Collections.Generic;
using Simoncouche.Controller;

namespace Simoncouche.Chain {
    /// <summary>
    /// A HookThrower controls a character's aiming and spawns hooks and chains upon user input.
    /// </summary>
    [RequireComponent(typeof(HingeJoint2D))]
	[RequireComponent(typeof(AimController))]
	public class HookThrower : MonoBehaviour {

        enum State
        {
            NoHook,
            OneHook,
            TwoHook
        }

        private State _currentState;

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

		// COMPONENTS   
            
		public HingeJoint2D joint { get; private set; }
		public AimController aimController { get; private set; }

        public void Awake() {
			this.joint = GetComponent<HingeJoint2D>();
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
                //_currentState = State.OneHook;
            }
            //TODO: MODIFIER POUR LIER LES 2 CHAINES
            else if(_currentState == State.OneHook) //If we press fire when we have 1 hook, we create a hook and switch the currentState to NoHook
            {
                _chains[_chains.Count - 1].CreateSecondHook();
                joint.enabled = false;
                //_currentState = State.NoHook;
                //_currentState = State.TwoHook;
            }
            else if(_currentState == State.TwoHook)
            {
                _currentState = State.NoHook;
            }
			
		}
	}
}
