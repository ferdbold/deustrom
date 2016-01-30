using UnityEngine;
using System.Collections;

namespace Simoncouche.Islands {
	/// <summary>
	/// The component attached to a Island chunk
	/// </summary>
	[RequireComponent(typeof(PolygonCollider2D))]
	public class IslandChunk : MonoBehaviour {

		[Header("Island Property")]

		/// <summary>
		/// The Color of the Island
		/// </summary>
		[SerializeField]
		[Tooltip("The Assign color of the Island")]
		private IslandUtils.color _color;
		/// <summary>
		/// Accessor of _color
		/// </summary>
		public IslandUtils.color color {
			get {
				return _color;
			}
			private set { }
		}

		/*/// <summary>
		/// Associated Chunk Letter
		/// </summary>
		[SerializeField]
		[Tooltip("The associated chunk letter")]
		private IslandUtils.ChunkLetter _chunkLetter;
		/// <summary>
		/// Accessors of _chunkLetter
		/// </summary>
		public IslandUtils.ChunkLetter chunkLetter {
			get {
				return _chunkLetter;
			}
			private set { }
		}*/

		void OnTriggerEnter2D(Collider2D other) {
			IslandChunk chunk = other.GetComponent<IslandChunk>();
			if (chunk != null && chunk.color == _color) {
				IslandManager.Instance.HandleChunkCollision(this, chunk);
			}
		}
	}
}
