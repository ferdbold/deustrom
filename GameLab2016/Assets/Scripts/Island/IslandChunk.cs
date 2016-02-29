using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

namespace Simoncouche.Islands {
    public class IslandEvent : UnityEvent<Island> {}

	/// <summary>
	/// The structure related to an island chunk
	/// </summary>
    [RequireComponent(typeof(AudioSource))]
	[RequireComponent(typeof(GravityBody))]
	public class IslandChunk : MonoBehaviour {

		//VARIABLES
		#region Inspector Variables
		[Header("Island Property")]

        [SerializeField][Tooltip("Current Island this islandChunk is attached to. Only modify this to create group of island chunks before in the editor.")]
        public Island parentIsland = null;
        
        [SerializeField] [Tooltip("The Assign color of the Island")]
		private IslandUtils.color _color;	
		public IslandUtils.color color {
			get { return _color; }
            protected set { _color = value; }
		}
		
		[SerializeField] [Tooltip("Weight of the island chunk")] [Range(1, 10)]
		private int _weight = 1;
		public int weight {
			get { return _weight; }
			protected set { _weight = value; }
		}

		[Header("Anchor Points Attributes")] [SerializeField] [Tooltip("The distance from the origin to the side of the hexagon")]
		private float _anchorPointDistance = 0.5f;

		[SerializeField] [Tooltip("The radius of the anchor trigger zone")]
		private float _anchorPointRadius = 0.2f;
		#endregion

        #region Events

        /// <summary>Invoked when this chunk is merged into a larger island</summary>
        /// <param>The merge into island.</value>
        public IslandEvent MergeIntoIsland { get; private set; }

        #endregion

		#region Component Ref
		public List<IslandAnchorPoints> anchors { get; private set; }

        public List<IslandChunk> connectedChunk { get; private set; }

        /// <summary> Gravity Body associated with this island chunk </summary>
        public GravityBody gravityBody {get; private set;}

		/// <summary> The anchor point type object (prefab ref) </summary>
		private static GameObject _anchorPointObject = null;

        /// <summary> audio source of the chunk </summary>
        private AudioSource _audioSource;
        /// <summary> island visual randomizer </summary>
        private RandomizeIslandVisual _randomizeIslandVisual;
		#endregion
		
		void Awake() {
            gravityBody = GetComponent<GravityBody>();
			if (_anchorPointObject == null) {
				_anchorPointObject = Resources.Load("Island/AnchorPoints") as GameObject;
			}
            _audioSource = GetComponent<AudioSource>();
            _randomizeIslandVisual = GetComponent<RandomizeIslandVisual>();

            connectedChunk = new List<IslandChunk>();
			SpawnAnchorPoints();

            this.MergeIntoIsland = new IslandEvent();
        }

        void Start() {
            _randomizeIslandVisual.SetIslandColorVisual(color);
        }


		#region Connection

        /// <summary>
        /// Add a target chunk from connected chunk
        /// </summary>
        /// <param name="other">target chunk</param>
        public void AddConnectedChunk(IslandChunk other) {
            connectedChunk.Add(other);
        }

        /// <summary>
		/// Remove a target chunks from connected chunk, also remove this chunk from target chunk connected chunk
        /// </summary>
        /// <param name="chunks">chunk</param>
        public void RemoveConnectedChunk(List<IslandChunk> chunks) {
			foreach (IslandChunk chunk in chunks) {
				connectedChunk.Remove(chunk);
				if (chunk.connectedChunk.Contains(this)) {
					chunk.RemoveConnectedChunk(this);
				}
			}
        }

		/// <summary>
		/// Remove a target chunk from connected chunk, also remove this chunk from target chunk connected chunk
		/// </summary>
		/// <param name="chunk">chunk</param>
		public void RemoveConnectedChunk(IslandChunk chunk) {
			connectedChunk.Remove(chunk);
			if (chunk.connectedChunk.Contains(this)) {
				chunk.RemoveConnectedChunk(this);
			}
		}

		/// <summary>
		/// Start the connection between 2 chunk/island
		/// </summary>
		/// <param name="targetPos">the target position</param>
		/// <param name="targetRot">the target rotation</param>
		/// <param name="time">the time taken</param>
		/// <param name="targetChunk">the other chunk</param>
		public void ConnectChunk(Vector3 targetPos, Vector3 targetRot, IslandChunk targetChunk, Island targetIsland, float time = 0.5f) {
            ChangeCollisionBetweenChunk(targetChunk);
            //Debug.Log(targetPos + " " + targetRot);
			transform.DOLocalRotate(targetRot, time);
			transform.DOLocalMove(targetPos, time);
            //StartCoroutine(Delay_CenterIslandRoot(time, targetIsland, targetChunk));
        }

        /// <summary>
        /// Change Island Collision and it's anchor collision
        /// </summary>
        /// <param name="targetChunk">the target chunk</param
        /// <param name="colliding">is the target colliding with this chunk</param>
        private void ChangeCollisionBetweenChunk(IslandChunk targetChunk, bool colliding = false) {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), targetChunk.GetComponent<Collider2D>(), !colliding);
            foreach (IslandAnchorPoints point in anchors) {
                foreach (IslandAnchorPoints targetPoint in targetChunk.anchors) {
                    Physics2D.IgnoreCollision(point.GetComponent<Collider2D>(), targetChunk.GetComponent<Collider2D>(), !colliding);
                }
            }

        }


        /// <summary> Calls Center Island root function on a delay t in seconds </summary>
        private IEnumerator Delay_CenterIslandRoot(float t, Island targetIsland, IslandChunk targetChunk) {
            yield return new WaitForSeconds(t);
            targetIsland.CenterIslandRoot();
            Debug.Log(Vector3.Distance(transform.position, targetChunk.transform.position));
        }

		#endregion

		#region Anchor Points Handling

		/// <summary>
		/// Check every anchor point to create the 
		/// </summary>
		public void CheckConnection() {
			List<IslandChunk> connected = new List<IslandChunk>();
			foreach (IslandAnchorPoints anchor in anchors) {
				IslandChunk chunk = anchor.GetConnectedIsland();
				if (chunk != null && chunk != this) {
					connected.Add(chunk);
				}
			}
			connectedChunk = connected;
		}

		/// <summary> Spawn Every Anchor points around the island </summary>
		void SpawnAnchorPoints () {
			anchors = new List<IslandAnchorPoints>();
            GameObject anchorParent = new GameObject();
            anchorParent.name = "Anchors";
            anchorParent.transform.SetParent(transform);
            anchorParent.transform.localPosition = Vector3.zero;
            anchorParent.transform.localScale = Vector3.one;

			for (int angle=0; angle <= 300; angle+=60) {
				Transform anchor = (Instantiate(_anchorPointObject) as GameObject).transform;
				anchor.SetParent(anchorParent.transform);
				anchor.localPosition = new Vector3(_anchorPointDistance * Mathf.Cos(angle * Mathf.PI / 180f),
												   _anchorPointDistance * Mathf.Sin(angle * Mathf.PI / 180f),
												   0);
				anchor.name = "AnchorPoints " + angle;
				anchor.GetComponent<CircleCollider2D>().radius = _anchorPointRadius;
                IslandAnchorPoints point = anchor.GetComponent<IslandAnchorPoints>();
                point.Setup(angle);
                anchors.Add(point);
			}
		}

		/// <summary>
		/// Handle a trigger collision with the anchors
        /// Raises the MergeIntoIsland event.
		/// </summary>
		/// <param name="other">The object that collided with the anchor</param>
		/// <param name="anchor">The anchor sending the event</param>
		public void HandleAnchorPointCollision(Collider2D other, IslandAnchorPoints anchor) {
			IslandAnchorPoints otherAnchor = other.GetComponent<IslandAnchorPoints>();
			IslandChunk chunk = other.GetComponentInParent<IslandChunk>();

			if (otherAnchor != null && otherAnchor.transform.parent.gameObject != gameObject ) {
                
				if (IslandUtils.CheckIfOnSameIsland(chunk, this)) {
					return;
				}
                
				//Debug.Log("Collision between " + transform.name + " and " + other.name + ". They Assemble.");
				GameManager.islandManager.HandleChunkCollision(this, anchor, chunk, otherAnchor);
                _audioSource.PlayOneShot(GameManager.audioManager.islandSpecificSound.mergeSound);
            }
		}

		#endregion

		void OnCollisionEnter2D(Collision2D col) {
            Collider2D other = col.collider;
			IslandChunk chunk = other.GetComponent<IslandChunk>();

			//Collide with chunk of other color
			if (chunk != null && chunk.color != _color) {

                //Debug.Log("Collision between " + transform.name + " and " + col.collider.name + ". They Collide.");
				_audioSource.PlayOneShot(GameManager.audioManager.islandSpecificSound.collisionSound);
            }
		}

        /// <summary>
        /// The damage taken by the island connected to this chunk. A chunk not part of an island does not take damage
        /// </summary>
        /// <param name="damage">The number of chunk affected by the division</param>
        public void TakeDamage(int damage) {
            GameManager.islandManager.TakeDamageHandler(this, damage);
        }

        /// <summary>
        /// Method called when entering the maelstrom
        /// </summary>
        public void OnMaelstromEnter() {
            if (parentIsland != null) parentIsland.OnMaelstromEnter(this);
            else {
                GameManager.islandManager.DestroyChunk(this);
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
