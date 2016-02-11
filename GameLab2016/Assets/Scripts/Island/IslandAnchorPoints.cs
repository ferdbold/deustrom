using UnityEngine;
using System.Collections;

namespace Simoncouche.Islands {
	public class IslandAnchorPoints : MonoBehaviour {

		/// <summary> The associated angle of this anchor point </summary>
		public float angle { get; private set; }

		/// <summary> The local position of the anchor points </summary>
		public Vector3 position {
			get {
				return transform.localPosition;
			}
		}

		private IslandChunk _parentRef;

		void Awake() {
			//GetComponentInChildren("");
		}

		void OnTriggerEnter2D(Collider2D other) {

		}
	}
}
