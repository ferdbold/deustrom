using UnityEngine;

namespace Simoncouche.Chain {

	[RequireComponent(typeof(SpringJoint2D))]
	public class HookThrower : MonoBehaviour {

		[Tooltip("Reference to the grappling hook prefab")]
		[SerializeField]
		private Hook _hookPrefab;

		[Tooltip("The initial force sent to the hook upon throwing it")]
		[SerializeField]
		private float _initialForceAmount = 10f;

		private float _spawnChainDistanceThreshold = 4f;

		private Hook _hookObj;

		// COMPONENTS

		private SpringJoint2D _joint;
		public SpringJoint2D joint { get { return _joint; } }

		// METHODS

		public void Awake() {
			_joint = GetComponent<SpringJoint2D>();
		}

		public void Update() {
			// TODO: Move this to InputManager
			if (Input.GetButtonDown("Fire Hook") && !isGrapplingHookActive) {
				Fire();
			}

			if (isGrapplingHookActive && _joint.connectedBody != null) {
				if (Vector3.Distance(transform.position, _joint.connectedBody.position) > _spawnChainDistanceThreshold) {
					_joint.connectedBody.GetComponent<ChainSection>().SpawnNewSection();
				}
			}
		}

		private void Fire() {
			_hookObj = (Hook)Instantiate(_hookPrefab, transform.position, transform.rotation);
			_hookObj.SetInitialForce(_initialForceAmount);
			_hookObj.thrower = this;
			_joint.enabled = true;
		}

		private bool isGrapplingHookActive {
			get {
				return _hookObj != null;
			}
		}
	}
}
