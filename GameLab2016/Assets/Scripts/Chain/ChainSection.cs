using UnityEngine;
using System.Collections;

namespace Simoncouche.Chain {

	/// A ChainSection is an element of a chain that links to another section or a Hook and is also linked by another section.
	[RequireComponent(typeof(HingeJoint2D))]
	public class ChainSection : MonoBehaviour {

		/// Self-reference to the chain section prefab for factory purposes
		private static GameObject _chainSectionPrefab;

		/// The ChainSection that is linked to this section
		private ChainSection _nextChainSection;

		/// The chain this ChainSection is part of
		public Chain chain { get; private set; }

		/// The angle difference from the last link in the chain
		private const float CHAIN_ANGLE_DIFF = 90f;

		// COMPONENTS

		public HingeJoint2D joint { get; private set; }
		public new Rigidbody2D rigidbody { get; private set; }

		public Transform pivot { get; private set; }
		public Transform mesh { get; private set; }

        /// <summary>Spawn a new ChainSection inside a chain</summary>
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
			chainSection.chain = chain;
			chainSection.mesh.localRotation = previousLinkRotation * Quaternion.Euler(0, CHAIN_ANGLE_DIFF, 0);
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
			
		/// Generate a new ChainSection and link it to this section
		public void SpawnNewSection() {
            Vector3 nextChainSectionPosition = transform.position - transform.right * transform.localScale.x;
			_nextChainSection = ChainSection.Create(nextChainSectionPosition, this.chain, this.rigidbody, this.mesh.localRotation);
        }
	}
}
