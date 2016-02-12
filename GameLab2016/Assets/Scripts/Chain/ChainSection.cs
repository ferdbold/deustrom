using UnityEngine;
using System.Collections;

namespace Simoncouche.Chain {

	/// <summary>
	/// A ChainSection is an element of a chain that links to another section or a Hook and is also linked by another section.
	/// </summary>
	[RequireComponent(typeof(HingeJoint2D))]
	public class ChainSection : MonoBehaviour {

		private static GameObject _chainSectionPrefab;

		[Tooltip("Reference to the ChainSection prefab")]
		[SerializeField]
		//private ChainSection _chainSectionPrefab;

		/// <summary>
		/// The ChainSection that is linked to this section
		/// </summary>
		private ChainSection _nextChainSection;

		/// <summary>
		/// The chain this ChainSection is part of
		/// </summary>
		public Chain _chain;

		// COMPONENTS

		private HingeJoint2D _joint;
		public HingeJoint2D joint { get { return _joint; } }

		private Rigidbody2D _rigidbody;
		public new Rigidbody2D rigidbody { get { return _rigidbody; } }

		/// <summary>
		/// Spawn a new ChainSection inside a chain
		/// </summary>
		/// <param name="position">The world position for this new section</param>
		/// <param name="chain">The parent chain</param>
		/// <param name="previous">The previous link in the chain</param>
		public static ChainSection Create(Vector3 position, Chain chain, Rigidbody2D previous) {
			if (_chainSectionPrefab == null) {
				_chainSectionPrefab = Resources.Load("Chain/ChainSection") as GameObject;
			}

			ChainSection chainSection = ((GameObject)Instantiate(
                _chainSectionPrefab,
                position,
                previous.transform.rotation
            )).GetComponent<ChainSection>();

			chainSection.transform.parent = chain.transform;
			chainSection.joint.connectedBody = previous;
			chainSection.SetChain(chain);
			chain.thrower.joint.connectedBody = chainSection.rigidbody;

			return chainSection;
		}

		public void Awake() {
			_joint = GetComponent<HingeJoint2D>();
			_rigidbody = GetComponent<Rigidbody2D>();
		}

		/// <summary>
		/// Generate a new ChainSection and link it to this section
		/// </summary>
		public void SpawnNewSection() {
			Vector3 nextChainSectionPosition = transform.position - transform.up * transform.localScale.x;

			_nextChainSection = ChainSection.Create(nextChainSectionPosition, _chain, _rigidbody);
		}

		public void SetChain(Chain value) {
			_chain = value;
		}
	}
}
