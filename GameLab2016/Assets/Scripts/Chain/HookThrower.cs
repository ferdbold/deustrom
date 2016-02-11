using UnityEngine;

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
		/// A reference to the thrown hook
		/// </summary>
		private Hook _hookObj;

		/// <summary>
		/// The current aim orientation as set by the right analog input
		/// </summary>
		private float _aimOrientation;

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
			if (isGrapplingHookActive && _joint.connectedBody != null) {
				if (Vector3.Distance(transform.position, _joint.connectedBody.position) > _spawnChainDistanceThreshold) {
					_joint.connectedBody.GetComponent<ChainSection>().SpawnNewSection();
				}
			}

			// Apply rotation continously to the aimIndicator to prevent character rotation from updating the indicator
			_aimIndicator.transform.rotation = Quaternion.Euler(0, 0, _aimOrientation);
		}

		/// <summary>
		/// Handle user input to throw a new chain and hook
		/// </summary>
		private void Fire() {
			// Prevent player from throwing multiple hooks (for now)
			if (!isGrapplingHookActive) {
				_hookObj = (Hook)Instantiate(_hookPrefab, transform.position, Quaternion.Euler(0, 0, _aimOrientation));
				_hookObj.SetInitialForce(_initialForceAmount);
				_hookObj.thrower = this;
				_joint.enabled = true;
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
				_aimOrientation = Vector2.Angle(Vector2.right, orientation);

				if (axisValues[1] < 0) {
					_aimOrientation = 360f - _aimOrientation;
				}
			}
		}

		private bool isGrapplingHookActive {
			get {
				return _hookObj != null;
			}
		}
	}
}
