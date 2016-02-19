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
		
		/// <summary>
		/// The angle difference from the last link in the chain
		/// </summary>
		[Tooltip("The angle difference from the last link in the chain")]
		private static float chainAngleDiff = 90f;

		// COMPONENTS

		public HingeJoint2D joint { get; private set; }
		public Rigidbody2D rigidbody { get; private set; }

		public Transform pivot { get; private set; }
		public Transform mesh { get; private set; }

        /// <summary>
        /// Spawn a new ChainSection inside a chain
        /// </summary>
        /// <param name="position">The world position for this new section</param>
        /// <param name="chain">The parent chain</param>
        /// <param name="previousRigidbody">The previous link in the chain</param>
		/// <param name="previousLinkRotation">The previous link local rotation</param>
		public static ChainSection Create(Vector3 position, Chain chain, Rigidbody2D previousRigidbody, Quaternion previousLinkRotation = default(Quaternion)) {
			if (_chainSectionPrefab == null) {
				_chainSectionPrefab = Resources.Load("Chain/ChainSection") as GameObject;
			}

            ChainSection chainSection = ((GameObject)Instantiate(
                _chainSectionPrefab,
                position,
				previousRigidbody.transform.rotation
            )).GetComponent<ChainSection>();
				
			chainSection.transform.parent = chain.transform;
			chainSection.joint.connectedBody = previousRigidbody;
			chainSection.SetChain(chain);
			chainSection.mesh.localRotation = previousLinkRotation * Quaternion.Euler(0, chainAngleDiff, 0);
			chain.thrower.joint.connectedBody = chainSection.rigidbody;

			return chainSection;
		}

		public void Awake() {
			this.joint = GetComponent<HingeJoint2D>();
			this.rigidbody = GetComponent<Rigidbody2D>();

			this.pivot = transform.Find("Pivot");
			this.mesh = transform.Find("Pivot/Mesh");
		}

		public void Update() {
			this.pivot.LookAt(this.joint.connectedBody.transform.position);
			this.pivot.Rotate(-90, 0, 0);
		}

		/// <summary>
		/// Generate a new ChainSection and link it to this section
		/// </summary>
		public void SpawnNewSection() {
            Vector3 nextChainSectionPosition = transform.position - transform.right * transform.localScale.x;
			_nextChainSection = ChainSection.Create(nextChainSectionPosition, _chain, this.rigidbody, this.mesh.localRotation);
            _chain.endingLink = _nextChainSection;
        }

		private void SetChain(Chain value) {
			_chain = value;
		}
	}
}
