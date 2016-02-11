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

		[Header("Anchor Points Attributes")]
		[SerializeField]
		[Tooltip("The distance from the origin to the side of the hexagon")]
		private float _anchorPointRadius = 1f;

        /// <summary> Gravity Body associated with this island chunk </summary>
        public GravityBody gravityBody {get; private set;}

		private static GameObject _anchorPointObject = null;

        protected virtual void Awake() {
            gravityBody = GetComponent<GravityBody>();
			if (_anchorPointObject == null) {
				_anchorPointObject = Resources.Load("Island/AnchorPoints") as GameObject;
			}
			SpawnAnchorPoints();
        }

		#region Anchor Points Handling

		/// <summary> Spawn Every Anchor points around the island </summary>
		void SpawnAnchorPoints () {
			for (int angle=0; angle <= 300; angle+=60) {
				Transform anchor = (Instantiate(_anchorPointObject) as GameObject).transform;
				anchor.SetParent(transform);
				anchor.localPosition = new Vector3(_anchorPointRadius * Mathf.Cos(angle * Mathf.PI / 180f),
												   _anchorPointRadius * Mathf.Sin(angle * Mathf.PI / 180f),
												   0);
				anchor.name = "AnchorPoints angle: " + angle;
			}
		}

		/// <summary>
		/// Handle a trigger collision with the anchors
		/// </summary>
		/// <param name="other">The object that collided with the anchor</param>
		/// <param name="anchor">The anchor sending the event</param>
		public void HandleAnchorPointCollision(Collider2D other, IslandAnchorPoints anchor) {
			IslandChunk chunk = other.GetComponent<IslandChunk>();

			if (chunk != null && chunk.color == _color) {
				GameManager.islandManager.HandleChunkCollision(this, chunk);
				Debug.Log("Collision between " + transform.name + " and " + other.name + ". They Assemble.");
				//TODO AUDIO : ISLAND ASSEMBLE SOUND
			}
		}

		#endregion

		void OnCollisionEnter2D(Collision2D col) {
            Collider2D other = col.collider;
			IslandChunk chunk = other.GetComponent<IslandChunk>();

			
			if (chunk != null && chunk.color == _color) {
  
                //TODO AUDIO : ISLAND COLLISION SOUND
            }
		}

		#region Legacy
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
		#endregion
	}
}
