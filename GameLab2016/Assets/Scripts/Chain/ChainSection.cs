using UnityEngine;
using System.Collections;

namespace Simoncouche.Chain {

	[RequireComponent(typeof(SpringJoint2D))]
	public class ChainSection : MonoBehaviour {

		[SerializeField]
		private ChainSection _chainSectionPrefab;

		private ChainSection _nextChainSection;
		public HookThrower thrower { get; set; }

		// COMPONENTS

		private SpringJoint2D _joint;
		public SpringJoint2D joint { get { return _joint; } }

		private Rigidbody2D _rigidbody;
		public new Rigidbody2D rigidbody { get { return _rigidbody; } }

		// METHODS

		public void Awake() {
			_joint = GetComponent<SpringJoint2D>();
			_rigidbody = GetComponent<Rigidbody2D>();
		}

		public void Start() {
			thrower.joint.connectedBody = _rigidbody;
		}

		public void SpawnNewSection() {
			Vector3 nextChainSectionPosition = transform.position;
			nextChainSectionPosition -= transform.right * transform.localScale.x;

			_nextChainSection = (ChainSection)Instantiate(_chainSectionPrefab, nextChainSectionPosition, transform.rotation);
			_nextChainSection.joint.connectedBody = _rigidbody;
			_nextChainSection.thrower = thrower;
		}
	}
}
