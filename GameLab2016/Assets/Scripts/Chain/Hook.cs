using UnityEngine;
using UnityEngine.Events;
using Simoncouche.Islands;
using Simoncouche.Controller;

namespace Simoncouche.Chain {

    /// <summary>
    /// A Hook is an ending element of a Chain that can snap itself 
    /// either to a character or an IslandAnchorPoint.
    /// </summary>
	[RequireComponent(typeof(DistanceJoint2D))]
	[RequireComponent(typeof(FixedJoint2D))]
	[RequireComponent(typeof(HingeJoint2D))]
	public class Hook : MonoBehaviour {

		[Tooltip("The mass of this hook once attached to an island")]
		[SerializeField]
		private float ATTACHED_MASS = 10f;

        /// <summary>Self-reference to the hook prefab for factory purposes</summary>
		private static GameObject _hookPrefab;

        /// <summary>The chain this hook is part of</summary>
		public Chain chain { get; private set; }

        /// <summary>The ChainSection linked to this hook</summary>
		private ChainSection _nextChain;

        /// <summary>Whether this hook is currently attached to a target</summary>
		public bool attachedToTarget { get; private set;}

        // EVENTS

        /// <summary>Invoked when this hook attaches itself to an island</summary>
        public UnityEvent Attach { get; private set; }

		// COMPONENTS

        /// <summary>The joint linking this hook to its target (an island)</summary>
		public FixedJoint2D targetJoint { get; private set; }

        /// <summary>
        /// The joint linking this hook to the other edge of the chain,
		/// (which will be the thrower or the second Hook of a chain). 
        /// Only used in the beginning hook.
        /// </summary>
		public DistanceJoint2D chainJoint { get; private set; }

        /// <summary>
        /// The joint used to hang visual chain sections to this hook.
		/// Only used in the beginning hook to hang the first chain section.
        /// </summary>
		public HingeJoint2D visualChainJoint { get; private set; }

		public new Rigidbody2D rigidbody { get; private set; }

		/// <summary>Spawn a new hook inside a chain</summary>
		/// <param name="chain">The parent chain</param>
		/// <param name="isBeginningHook">Is this hook the beginning hook of a chain</param> 
		public static Hook Create(Chain chain, bool isBeginningHook, bool isPlayerOne) {
			if (_hookPrefab == null) {
                if (isPlayerOne) _hookPrefab = Resources.Load("Chain/Hook") as GameObject;
                else _hookPrefab = Resources.Load("Chain/Hook") as GameObject;
			}

			Hook hook = ((GameObject)Instantiate(
				_hookPrefab, 
				chain.thrower.transform.position, 
				Quaternion.Euler(0, 0, chain.thrower.aimController.aimOrientation)
			)).GetComponent<Hook>();

			hook.name = (isBeginningHook) ? "BeginningHook" : "EndingHook";
			hook.transform.parent = chain.transform;
			hook.chain = chain;

			hook.chainJoint.enabled = isBeginningHook;

			if (isBeginningHook) {
				// Hook the beginning hook's chain joint to the player to enable physics tension
				hook.chainJoint.connectedBody = chain.thrower.rigidbody;
			}

			return hook;
		}

		public void Awake() {
			this.chainJoint = GetComponent<DistanceJoint2D>();
			this.targetJoint = GetComponent<FixedJoint2D>();
			this.visualChainJoint = GetComponent<HingeJoint2D>();
			this.rigidbody = GetComponent<Rigidbody2D>();

            this.Attach = new UnityEvent();
		}

		public void Start() {
            attachedToTarget = false;
			this.rigidbody.AddForce(transform.rotation * new Vector2(chain.initialForce, 0));
		}

		public void OnTriggerEnter2D(Collider2D coll) {
			if (!attachedToTarget) {
				IslandAnchorPoints anchorPoint = coll.gameObject.GetComponent<IslandAnchorPoints>();

				if (anchorPoint != null) {
					this.AttachToIsland(anchorPoint);
				}
			}
		}

		/// <summary>Spawns the first chain section and attach to it</summary>
		/// <returns>The chain section</returns>
		public ChainSection SpawnChainSection(bool isPlayerOne) {
			ChainSection chainSection = ChainSection.Create(
                transform.position, 
				this.rigidbody.transform.rotation, 
				this.chain, 
				this.gameObject, 
                isPlayerOne,
				Quaternion.identity
			);

			// Connect the new chain section to the player
            chainSection.joint.connectedBody = this.chain.thrower.chainLinker;

			// Attach the visual joint to this first chain section
			this.visualChainJoint.enabled = true;
			this.visualChainJoint.connectedBody = chainSection.rigidbody;

			return chainSection;
		}

		/// <summary>
        /// Attach this Hook to the Island associated to an anchor point.
        /// Raises the Attach event.
        /// </summary>
		/// <param name="anchor">The anchor point</param>
		public void AttachToIsland(IslandAnchorPoints anchor) {
            Island parentIsland = anchor.GetIslandChunk().parentIsland;

            this.rigidbody.velocity = Vector2.zero;
            this.rigidbody.mass = this.ATTACHED_MASS;

            this.targetJoint.enabled = true;

            // Attach the joint to either the chunk or its parent island it it has one
            if (parentIsland == null) {
                this.targetJoint.connectedBody = anchor.GetIslandChunk().GetComponent<Rigidbody2D>();
            } else {
                this.targetJoint.connectedBody = parentIsland.rigidbody;
            }

            // Add listeners
            anchor.GetIslandChunk().MergeIntoIsland.AddListener(this.OnAttachedChunkMerge);
            anchor.GetIslandChunk().GrabbedByPlayer.AddListener(this.OnAttachedChunkPlayerGrab);
            anchor.GetIslandChunk().ReleasedByPlayer.AddListener(this.OnAttachedChunkPlayerRelease);

            this.Attach.Invoke();

            //The hook is now attached to a target
            attachedToTarget = true;
		}

		/// <summary>
		/// Attach this section's joint to another rigidbody. 
		/// This is useful for deleting sections of a chain
		/// </summary>
		/// <param name="rb">The new rigidbody to attach to</param>
		public void AttachVisualJointTo(Rigidbody2D rb) {
			this.visualChainJoint.connectedBody = rb;
		}

        /// <summary>React to attached chunk being merged into island</summary>
        /// <param name="newIsland">The resultant isl+and</param>
        private void OnAttachedChunkMerge(Island newIsland) {
            // Attach the joints to its parent island
            this.targetJoint.connectedBody = newIsland.rigidbody;
        }

        /// <summary>React to attached chunk being grabbed by a player</summary>
        /// <param name="playerGrab">The player who grabbed the chunk</param> 
        private void OnAttachedChunkPlayerGrab(PlayerGrab playerGrab) {
            // Reroute the chain to the player
            this.targetJoint.connectedBody = playerGrab.rigidbody;
        }

        /// <summary>React to attached chunk being released by a player</summary>
        /// <param name="rb">The rigidbody the player was holding</param> 
        private void OnAttachedChunkPlayerRelease(Rigidbody2D rb) {
            // Reroute the chain back to the island or chunk
            this.targetJoint.connectedBody = rb;
        }
	}
}
