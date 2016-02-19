﻿using UnityEngine;
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


        [Tooltip("Maximum number of links per chain")]
        [SerializeField]
        private int maximumLinksPerChain=30;

        [Tooltip("The distance the first hook is in front of the player")]
        [SerializeField]
        private float distanceHookInFrontOfPlayer = 3f;

		// COMPONENTS
		        
		public SpringJoint2D joint { get; private set; }
		public AimController aimController { get; private set; }



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

		public void Update() {
            _aimIndicator.transform.rotation = Quaternion.Euler(0, 0, this.aimOrientation);
		}

		/// <summary>
		/// Handle user input to throw a new chain and hook
		/// </summary>
		private void Fire() {
            if (_currentState == State.NoHook)
            {
                _chains.Add(Chain.Create(this, _initialForceAmount));
                _joint.enabled = true;
                _currentState = State.OneHook;
            }
            else if(_currentState == State.OneHook)
            {
                _chains[_chains.Count - 1].CreateSecondHook();
                _currentState = State.TwoHook;
            }
            else if(_currentState == State.TwoHook)
            {
                _currentState = State.NoHook;
            }
			
		}

		/// <summary>
		/// Handle user input to update the aim indicator
		/// </summary>
		/// <param name="axisValues">Axis values.</param>
		private void Aim(float[] axisValues) {
			Vector2 orientation = new Vector2(axisValues[0], axisValues[1]);

			// Only apply aiming if the user input is relevant (higher than the deadzone)
			if (orientation.magnitude > _aimDeadzone) {
				this.aimOrientation = Vector2.Angle(Vector2.right, orientation);

				if (axisValues[1] < 0) {
					this.aimOrientation = 360f - this.aimOrientation;
				}
			}
		}
	}
}
