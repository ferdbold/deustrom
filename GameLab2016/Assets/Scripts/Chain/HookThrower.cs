using UnityEngine;
using System.Collections.Generic;

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

		[Tooltip("Input axis threshold before applying aiming")]
		[SerializeField]
		private float _aimDeadzone = 0.01f;

		/// <summary>
		/// The minimum distance needed between the thrower and a chain's last ChainSection to spawn a new ChainSection
		/// </summary>
		private float _spawnChainDistanceThreshold = 4f;

		/// <summary>
		/// The list of all the chains thrown by this thrower currently in play
		/// </summary>
		private List<Chain> _chains = new List<Chain>();

		/// <summary>
		/// The current aim orientation as set by the right analog input
		/// </summary>
		public float aimOrientation { get; private set; }

        [Tooltip("Maximum number of links per chain")]
        [SerializeField]
        private int maximumLinksPerChain=30;

        [Tooltip("The distance the first hook is in front of the player")]
        [SerializeField]
        private float distanceHookInFrontOfPlayer = 3f;


        // COMPONENTS

        private SpringJoint2D _joint;
		public SpringJoint2D joint { get { return _joint; } }

		private Transform _aimIndicator;

		// METHODS

		public void Awake() {
			_joint = GetComponent<SpringJoint2D>();
			_aimIndicator = transform.Find("AimIndicator");

			if (_aimIndicator == null) {
				Debug.LogError("Player/AimIndicator cannot be found!");
			}
		}

		public void Start() {
			GameManager.inputManager.AddEvent(InputManager.Button.fireHook, this.Fire);
			GameManager.inputManager.AddEvent(InputManager.Axis.rightAnalog, this.Aim);
		}

		public void Update() {
			// Generate new sections if the distance to the linked section exceeds the threshold
			// TODO: Unfuck this
			if (_chains.Count > 0 && _joint.connectedBody != null) {
				if (Vector3.Distance(transform.position, _joint.connectedBody.position) > _spawnChainDistanceThreshold) {
                    foreach(Chain chain in _chains){
                        if (chain.currentLinkNumber <= maximumLinksPerChain) {
                            chain.IncrementLinkNumber();
                            _joint.connectedBody.GetComponent<ChainSection>().SpawnNewSection();
                        }
                    }
				}
			}
            foreach (Chain chain in _chains)
            {
                if (chain.endingLink) chain.endingLink.GetComponent<Rigidbody2D>().position = this.transform.position + transform.right * distanceHookInFrontOfPlayer;
            }

                // Apply rotation continously to the aimIndicator to prevent character rotation from updating the indicator
                _aimIndicator.transform.rotation = Quaternion.Euler(0, 0, this.aimOrientation);
		}

		/// <summary>
		/// Handle user input to throw a new chain and hook
		/// </summary>
		private void Fire() {
			_chains.Add(Chain.Create(this, _initialForceAmount));
			_joint.enabled = true;
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
