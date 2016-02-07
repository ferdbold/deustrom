using UnityEngine;

namespace Simoncouche.Chain {

	[RequireComponent(typeof(FixedJoint2D))]
	public class Hook : MonoBehaviour {

		[SerializeField]
		private ChainSection _chainSectionPrefab;

		[SerializeField]
		private float _forceAmount = 50f;

		private ChainSection _nextChain;

		public HookThrower thrower { get; set; }

		// COMPONENTS

		private Rigidbody2D _rigidbody;

		private FixedJoint2D _joint;
		public FixedJoint2D joint { get { return _joint; } }

		// METHODS

		public void Awake() {
			_rigidbody = GetComponent<Rigidbody2D>();
			_joint = GetComponent<FixedJoint2D>();

			_rigidbody.AddForce(transform.rotation * new Vector2(_forceAmount, 0), ForceMode2D.Impulse);
		}

		public void Start() {
			_nextChain = (ChainSection)Instantiate(_chainSectionPrefab, transform.position, transform.rotation);
			_nextChain.joint.connectedBody = _rigidbody;
			_nextChain.thrower = thrower;
		}

		public void OnCollisionEnter2D(Collision2D collision) {
			if (collision.gameObject.GetComponent<Hookable>() != null) {
				_joint.enabled = true;
				_joint.connectedBody = collision.rigidbody;
				_joint.connectedAnchor = collision.transform.InverseTransformPoint(collision.contacts[0].point);
			}
		}
	}
}
