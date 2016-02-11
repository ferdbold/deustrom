using UnityEngine;

namespace Simoncouche.Chain {

	[RequireComponent(typeof(FixedJoint2D))]
	public class Hook : MonoBehaviour {

		[SerializeField]
		private ChainSection _chainSectionPrefab;

		private ChainSection _nextChain;

		private float _initialForce = 0f;

		public HookThrower thrower { get; set; }

		// COMPONENTS

		private Rigidbody2D _rigidbody;

		private FixedJoint2D _joint;
		public FixedJoint2D joint { get { return _joint; } }

		// METHODS

		public void Awake() {
			_rigidbody = GetComponent<Rigidbody2D>();
			_joint = GetComponent<FixedJoint2D>();
		}

		public void Start() {
			_nextChain = (ChainSection)Instantiate(_chainSectionPrefab, transform.position, transform.rotation);
			_nextChain.joint.connectedBody = _rigidbody;
			_nextChain.thrower = thrower;

			_rigidbody.AddForce(transform.rotation * new Vector2(_initialForce, 0));
		}

		public void OnCollisionEnter2D(Collision2D collision) {
			if (collision.gameObject.GetComponent<Hookable>() != null) {
				_joint.enabled = true;
				_joint.connectedBody = collision.rigidbody;
				_joint.connectedAnchor = collision.transform.InverseTransformPoint(collision.contacts[0].point);
			}
		}

		public void SetInitialForce(float value) {
			_initialForce = value;
		}
	}
}
