using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

namespace Simoncouche.Islands {
    public class Rigidbody2DEvent : UnityEvent<Rigidbody2D> {}
    public class IslandEvent : UnityEvent<Island> {}
    public class PlayerGrabEvent : UnityEvent<Simoncouche.Controller.PlayerGrab> {}
    
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
        public Island _parentIsland = null;

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
        /// <param>The resulting parent island</param>
        public IslandEvent MergeIntoIsland { get; private set; }

        /// <summary>Invoked when this chunk (or its parent island) gets grabbed by a player</summary>
        /// <param>The grabbing player</param>
        public PlayerGrabEvent GrabbedByPlayer { get; private set; }

        /// <summary>Invoked when this chunk (or its parent island) is released by a player</summary>
        public Rigidbody2DEvent ReleasedByPlayer { get; private set; }

        #endregion

        #region Component Ref
        public List<IslandAnchorPoints> anchors { get; private set; }

        public List<IslandChunk> connectedChunk;/* { get; private set; }*/

        /// <summary> Gravity Body associated with this island chunk </summary>
        public GravityBody gravityBody {get; private set;}

        /// <summary> The anchor point type object (prefab ref) </summary>
        private static GameObject _anchorPointObject = null;

        /// <summary> audio source of the chunk </summary>
        private AudioSource _audioSource;
        public AudioSource audioSource { get { return _audioSource; } }

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
            this.GrabbedByPlayer = new PlayerGrabEvent();
            this.ReleasedByPlayer = new Rigidbody2DEvent();
        }

        void Start() {
            _randomizeIslandVisual.SetIslandColorVisual(color);
            StartCoroutine(DebugPoint());
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
            parentIsland.ChangeCollisionInIsland(this, false);
            //Debug.Log(targetPos);
            /*transform.localPosition = targetPos;
            transform.localRotation = Quaternion.Euler(targetRot);*/
            transform.DOLocalRotate(targetRot, time);
            transform.DOLocalMove(targetPos, time);
            //StartCoroutine(Delay_CenterIslandRoot(0f, targetIsland, targetChunk));
        }

        /// <summary>
        /// Change Island Collision and it's anchor collision
        /// </summary>
        /// <param name="targetChunk">the target chunk</param
        /// <param name="colliding">is the target colliding with this chunk</param>
        public void ChangeCollisionBetweenChunk(IslandChunk targetChunk, bool colliding = false) {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), targetChunk.GetComponent<Collider2D>(), !colliding);
            foreach (IslandAnchorPoints point in anchors) {
                Physics2D.IgnoreCollision(point.GetComponent<Collider2D>(), targetChunk.GetComponent<Collider2D>(), !colliding);
                foreach (IslandAnchorPoints targetPoint in targetChunk.anchors) {
                    Physics2D.IgnoreCollision(point.GetComponent<Collider2D>(), targetPoint.GetComponent<Collider2D>(), !colliding);
                }
            }

        }


        /// <summary> Calls Center Island root function on a delay t in seconds </summary>
        private IEnumerator Delay_CenterIslandRoot(float t, Island targetIsland, IslandChunk targetChunk) {
            yield return new WaitForSeconds(t);
            //targetIsland.CenterIslandRoot();
        }

        #endregion

        #region Anchor Points

        /// <summary>
        /// Check every anchor point to create the 
        /// </summary>
        public void CheckConnection() {
            List<IslandChunk> connected = new List<IslandChunk>();
            foreach (IslandAnchorPoints anchor in anchors) {
                IslandChunk chunk = anchor.GetConnectedIsland();
                if (chunk != null && chunk != this && !connected.Contains(chunk)) {
                    connected.Add(chunk);
                }
            }
            connectedChunk = connected;
        }

        /// <summary>
        /// Make a second check of the connection to make sure they are correct
        /// </summary>
        public void CheckLinkConnection() {
            foreach (IslandChunk chunk in connectedChunk) {
                if (!chunk.connectedChunk.Contains(this)) {
                    chunk.connectedChunk.Add(this);
                }
            }
        }

        /// <summary> Spawn Every Anchor points around the island </summary>
        void SpawnAnchorPoints () {
            anchors = new List<IslandAnchorPoints>();
            GameObject anchorParent = new GameObject();
            anchorParent.name = "Anchors";
            anchorParent.transform.SetParent(transform);
            anchorParent.transform.localPosition = Vector3.zero;
            anchorParent.transform.localScale = Vector3.one;

            for (int angle=40; angle <= 340; angle+=60) {
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

        #endregion

        #region Collision Handling

        /// <summary>
        /// Handle a trigger collision with the anchors
        /// Raises the MergeIntoIsland event.
        /// </summary>
        /// <param name="other">The object that collided with the anchor</param>
        /// <param name="anchor">The anchor sending the event</param>
        public void HandleAnchorPointCollision(Collider2D other, IslandAnchorPoints anchor) {
            IslandAnchorPoints otherAnchor = other.GetComponent<IslandAnchorPoints>();
            IslandChunk chunk = other.GetComponentInParent<IslandChunk>();

            if (otherAnchor != null && otherAnchor.transform.parent.gameObject != gameObject) {

                if (IslandUtils.CheckIfOnSameIsland(chunk, this)) {
                    return;
                }

                if (!chunk.gravityBody.inDestroyMode && !gravityBody.inDestroyMode) {
                    //Debug.Log("Collision between " + transform.name + " and " + other.name + ". They Assemble.");
                    GameManager.islandManager.HandleChunkCollision(this, anchor, chunk, otherAnchor);
                    _audioSource.PlayOneShot(GameManager.audioManager.islandSpecificSound.mergeSound);
                }
            }
        }

        void OnCollisionEnter2D(Collision2D col) {
            Collider2D other = col.collider;
            IslandChunk chunk = other.GetComponent<IslandChunk>();

            if (chunk != null && chunk.parentIsland != null && chunk.parentIsland.gravityBody.inDestroyMode) {
                chunk.parentIsland.gravityBody.Velocity = 5 * Vector3.Normalize(transform.localPosition - chunk.transform.localPosition);
                chunk.parentIsland.gravityBody.RemoveInDestroyMode();
                TakeDamage(1);
            } else if (chunk != null && chunk.gravityBody.inDestroyMode) {
                chunk.gravityBody.Velocity = 5 * Vector3.Normalize(transform.localPosition - chunk.transform.localPosition);
                chunk.gravityBody.RemoveInDestroyMode();
                TakeDamage(1);
            }

            //Collide with chunk of other color
                //Debug.Log("Collision between " + transform.name + " and " + col.collider.name + ". They Collide.");
                //_audioSource.PlayOneShot(GameManager.audioManager.islandSpecificSound.collisionSound);
        }

        /// <summary>
        /// The damage taken by the island connected to this chunk. A chunk not part of an island does not take damage
        /// </summary>
        /// <param name="damage">The number of chunk affected by the division</param>
        public void TakeDamage(int damage) {
            if (damage > 0) {
                GameManager.islandManager.TakeDamageHandler(this, damage);
            } else {
                Debug.LogWarning("The damage sent to the island chunk should be higher than 0");
            }
        }

        /// <summary>
        /// Method called when entering the maelstrom
        /// </summary>
        public void OnMaelstromEnter() {
            if (parentIsland != null) {
                parentIsland.OnMaelstromEnter(this);
            } else {
                GameManager.islandManager.DestroyChunk(this);
            }
        }
        
        private void OnParentIslandGrabbedByPlayer(Simoncouche.Controller.PlayerGrab playerGrab) {
            // Bubble up player grab event
            this.GrabbedByPlayer.Invoke(playerGrab);
        }

        private void OnParentIslandReleasedByPlayer(Rigidbody2D rb) {
            // Bubble up player release event
            this.ReleasedByPlayer.Invoke(rb);
        }

        #endregion

        #region Properties

        public Island parentIsland {
            get {
                return _parentIsland;
            }
            set {
                if (_parentIsland != null) {
                    _parentIsland.GrabbedByPlayer.RemoveListener(this.OnParentIslandGrabbedByPlayer);
                    _parentIsland.ReleasedByPlayer.RemoveListener(this.OnParentIslandReleasedByPlayer);
                }
                _parentIsland = value;

                if (_parentIsland != null) {
                    _parentIsland.GrabbedByPlayer.AddListener(this.OnParentIslandGrabbedByPlayer);
                    _parentIsland.ReleasedByPlayer.AddListener(this.OnParentIslandReleasedByPlayer);
                }
            }
		}

		#endregion

        #region Conversion

        public void ConvertChunkToAnotherColor(IslandUtils.color newColor) {
            _color = newColor;
            _randomizeIslandVisual.SetIslandColorVisual(newColor);
        }

        #endregion

        IEnumerator DebugPoint () {
            while (true) {
                yield return new WaitForSeconds(1);
                if (GameManager.levelManager != null && (gravityBody.Velocity != Vector2.zero || (parentIsland != null && parentIsland.gravityBody.Velocity != Vector2.zero))) {
                    GameManager.levelManager.AddScore(color == IslandUtils.color.red ? LevelManager.Player.sobek : LevelManager.Player.cthulu, 1, transform.position);
                }
            }
        }
    }
}
