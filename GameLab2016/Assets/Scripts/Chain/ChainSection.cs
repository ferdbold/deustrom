using UnityEngine;
using System.Collections;

namespace Simoncouche.Chain {

	[RequireComponent(typeof(HingeJoint2D))]
	public class ChainSection : MonoBehaviour {

		[SerializeField]
		private ChainSection _chainSectionPrefab;

		private ChainSection _nextChainSection;
		public HookThrower thrower { get; set; }

		// COMPONENTS

		private HingeJoint2D _joint;
		public HingeJoint2D joint { get { return _joint; } }

		private Rigidbody2D _rigidbody;
		public new Rigidbody2D rigidbody { get { return _rigidbody; } }

		// METHODS

		public void Awake() {
			_joint = GetComponent<HingeJoint2D>();
			_rigidbody = GetComponent<Rigidbody2D>();
		}

		public void Start() {
			thrower.joint.connectedBody = _rigidbody;
		}

		public void SpawnNewSection() {
			Vector3 nextChainSectionPosition = transform.position;
			nextChainSectionPosition -= transform.up * transform.localScale.x;

			_nextChainSection = (ChainSection)Instantiate(_chainSectionPrefab, nextChainSectionPosition, transform.rotation);
			_nextChainSection.joint.connectedBody = _rigidbody;
			_nextChainSection.thrower = thrower;
		}
	}
}
