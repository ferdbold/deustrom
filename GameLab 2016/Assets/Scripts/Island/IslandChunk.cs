using UnityEngine;
using System.Collections;

namespace Simoncouche.Islands {
	/// <summary>
	/// The component attached to a Island chunk
	/// </summary>
	[RequireComponent(typeof(GravityBody))]
	public class IslandChunk : MonoBehaviour {

		[Header("Island Property")]

		/// <summary> The Color of the Island  </summary>
		[SerializeField]
		[Tooltip("The Assign color of the Island")]
		private IslandUtils.color _color;	
		public IslandUtils.color color {
			get { return _color; }
            protected set { _color = value; }
		}

		/// <summary>The weigth of the island chunk</summary>
		[SerializeField]
		[Tooltip("Weight of the island chunk")]
		[Range(1, 10)]
		private int _weight = 1;
		public int weight {
			get { return _weight; }
			protected set { _weight = value; }
		}

        /// <summary> Gravity Body associated with this island chunk </summary>
        public GravityBody gravityBody {get; private set;}

        protected void Awake() {
            gravityBody = GetComponent<GravityBody>();
        }

		/*/// <summary> Associated Chunk Letter </summary>
		[SerializeField]
		[Tooltip("The associated chunk letter")]
		private IslandUtils.ChunkLetter _chunkLetter;

		/// <summary> Accessors of _chunkLetter </summary>
		public IslandUtils.ChunkLetter chunkLetter {
			get {
				return _chunkLetter;
			}
			private set { }
		}*/

		void OnCollisionEnter2D(Collision2D col) {
            Collider2D other = col.collider;
			IslandChunk chunk = other.GetComponent<IslandChunk>();
			if (chunk != null && chunk.color == _color) {
				IslandManager.Instance.HandleChunkCollision(this, chunk);
			}
		}
	}
}
