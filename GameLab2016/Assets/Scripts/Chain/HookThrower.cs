using UnityEngine;

namespace Simoncouche.Chain {

	[RequireComponent(typeof(SpringJoint2D))]
	public class HookThrower : MonoBehaviour {

		[Tooltip("Reference to the grappling hook prefab")]
		[SerializeField]
		private Hook _hookPrefab;

		private float _spawnRopeDistanceThreshold = 1f;

		private Hook _hookObj;

		// COMPONENTS

		private SpringJoint2D _joint;
		public SpringJoint2D joint { get { return _joint; } }

		// METHODS

		public void Awake() {
			_joint = GetComponent<SpringJoint2D>();
		}

		public void Update() {
			// TODO: Move this to input manager
			if (Input.GetButtonDown("Fire") && !isGrapplingHookActive) {
				Fire();
			}

			if (isGrapplingHookActive && Vector3.Distance(transform.position, _joint.connectedBody.position) > _spawnRopeDistanceThreshold) {
				_joint.connectedBody.GetComponent<ChainSection>().SpawnNewSection();
			}
		}

		private void Fire() {
			_hookObj = (Hook)Instantiate(_hookPrefab, transform.position, transform.rotation);
			_hookObj.thrower = this;
			_hookObj.onHit.AddListener(RecallHook);
			_joint.enabled = true;
		}

		public void RecallHook() {
			Destroy(_hookObj.gameObject);
			_hookObj = null;
		}

		private bool isGrapplingHookActive {
			get {
				return _hookObj != null;
			}
		}
	}
}
