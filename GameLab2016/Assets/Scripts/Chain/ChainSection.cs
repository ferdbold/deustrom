using UnityEngine;
using System.Collections;

namespace Simoncouche.Chain {

	/// A ChainSection is an element of a chain that links to another section or a Hook and is also linked by another section.
	[RequireComponent(typeof(HingeJoint2D))]
	public class ChainSection : MonoBehaviour {

		/// Self-reference to the chain section prefab for factory purposes
		private static GameObject _chainSectionPrefab;

		/// The Hook or ChainSection that precedes this section in the chain
		public GameObject prev { get; private set; }

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
		/// <param name="rotation">The world rotation for this new section</param> 
        /// <param name="chain">The parent chain</param>
		/// <param name="previous">The previous element in the chain (either Hook or ChainSection)</param>
		/// <param name="previousLinkRotation">The previous link local rotation (for 90° alternance)</param>
		public static ChainSection Create(Vector3 position, Quaternion rotation, Chain chain, GameObject previous, Quaternion previousLinkRotation = default(Quaternion)) {
			if (_chainSectionPrefab == null) {
				_chainSectionPrefab = Resources.Load("Chain/ChainSection") as GameObject;
			}

            ChainSection chainSection = ((GameObject)Instantiate(
                _chainSectionPrefab,
                position,
				rotation
            )).GetComponent<ChainSection>();
				
			chainSection.transform.parent = chain.transform;
			chainSection.chain = chain;
			chainSection.prev = previous;

			chainSection.mesh.localRotation = previousLinkRotation * Quaternion.Euler(0, CHAIN_ANGLE_DIFF, 0);

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
		public ChainSection SpawnNewSection() {
            Vector3 nextChainSectionPosition = transform.position - transform.right * transform.localScale.x;

			ChainSection nextChainSection = ChainSection.Create(
				nextChainSectionPosition, 
				this.rigidbody.transform.rotation, 
				this.chain, 
				this.gameObject, 
				this.mesh.localRotation
			);

			// Connect the new section to the end of the chain
			nextChainSection.joint.connectedBody = this.joint.connectedBody;

			// Connect to this new section instead of the end of the chain
			this.joint.connectedBody = nextChainSection.rigidbody;

			return nextChainSection;
		}

		/// Remove this section from the chain and ensure proper linkage between the previous and the next.
		public void Remove() {
			this.prev.SendMessage("AttachVisualJointTo", this.joint.connectedBody, SendMessageOptions.RequireReceiver);
			Destroy(this.gameObject);
		}

		/// <summary>
		/// Attach this section's joint to another rigidbody. 
		/// This is useful for deleting sections of a chain
		/// </summary>
		/// <param name="rb">The new rigidbody to attach to</param>
		public void AttachVisualJointTo(Rigidbody2D rb) {
			this.joint.connectedBody = rb;
		}
	}
}
