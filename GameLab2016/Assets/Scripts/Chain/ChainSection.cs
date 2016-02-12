using UnityEngine;
using System.Collections;

namespace Simoncouche.Chain {

	/// <summary>
	/// A ChainSection is an element of a chain that links to another section or a Hook and is also linked by another section.
	/// </summary>
	[RequireComponent(typeof(HingeJoint2D))]
	public class ChainSection : MonoBehaviour {

		[Tooltip("Reference to the ChainSection prefab")]
		[SerializeField]
		private ChainSection _chainSectionPrefab;

		/// <summary>
		/// The ChainSection that is linked to this section
		/// </summary>
		private ChainSection _nextChainSection;

		/// <summary>
		/// The character that generated this ChainSection
		/// </summary>
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

		/// <summary>
		/// Generate a new ChainSection and link it to this section
		/// </summary>
		public void SpawnNewSection() {
			Vector3 nextChainSectionPosition = transform.position - transform.up * transform.localScale.x;

			_nextChainSection = (ChainSection)Instantiate(_chainSectionPrefab, nextChainSectionPosition, transform.rotation);
			_nextChainSection.joint.connectedBody = _rigidbody;
			_nextChainSection.thrower = thrower;
		}
	}
}
