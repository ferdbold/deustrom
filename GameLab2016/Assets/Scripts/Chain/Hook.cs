using UnityEngine;
using Simoncouche.Islands;

namespace Simoncouche.Chain {

	/// <summary>
	/// A Hook is an ending element of a Chain that can snap itself either to a character or an IslandAnchorPoint.
	/// </summary>
	[RequireComponent(typeof(FixedJoint2D))]
	public class Hook : MonoBehaviour {

		[Tooltip("Reference to the ChainSection prefab")]
		[SerializeField]
		private ChainSection _chainSectionPrefab;

		/// <summary>
		/// The ChainSection linked to this hook
		/// </summary>
		private ChainSection _nextChain;

		private float _initialForce;

		/// <summary>
		/// The character who throwed this hook
		/// </summary>
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

		public void OnTriggerEnter2D(Collider2D coll) {
			if (_joint.connectedBody == null) {
				if (coll.gameObject.GetComponent<IslandAnchorPoints>() != null) {
					_joint.enabled = true;
					_joint.connectedBody = coll.GetComponent<Rigidbody2D>();
				}
			}
		}

		public void SetInitialForce(float value) {
			_initialForce = value;
		}
	}
}
