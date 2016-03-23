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
        private static GameObject _hookPrefabSobek;
        /// <summary>Self-reference to the hook prefab for factory purposes</summary>
        private static GameObject _hookPrefabCthulu;
        /// <summary>Self-reference to the hook prefab for factory purposes</summary>
        private static GameObject _baseHookPrefab;

        /// <summary>The chain this hook is part of</summary>
        public Chain chain { get; private set; }

        /// <summary>The ChainSection linked to this hook</summary>
        private ChainSection _nextChain;

        public IslandAnchorPoints currentAnchorPoint { get; private set; }

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

        public Island connectedIsland { get; private set; }

        public bool islandIsGrabbedEnemy { get; private set; }

        /// <summary>Spawn a new hook inside a chain</summary>
        /// <param name="chain">The parent chain</param>
        /// <param name="isBeginningHook">Is this hook the beginning hook of a chain</param>
        /// <param name="orientation">The angle (in degrees) to apply to the hook</param> 
        public static Hook Create(Chain chain, bool isBeginningHook, bool isSobek, float orientation, bool characterMesh) {
            Vector3 initialPos;
            if (characterMesh) {
                if (isSobek) {
                    if (_hookPrefabSobek == null) {
                        _hookPrefabSobek = Resources.Load("Chain/HookSobek") as GameObject;
                    }
                } else if (_hookPrefabCthulu == null) {
                    _hookPrefabCthulu = Resources.Load("Chain/HookCthulhu") as GameObject;
                }
                initialPos = chain.thrower.transform.position + (isSobek ? new Vector3(0, 0, -1.5f) : new Vector3(0, 0, -1.5f)); //Elevates a tiny bit the elevation in order to see the hook on top of island
            } else {
                _baseHookPrefab = Resources.Load("Chain/Hook") as GameObject;
                initialPos = chain.thrower.transform.position;
            }

            Hook hook = ((GameObject)Instantiate(
                characterMesh?  isSobek?_hookPrefabSobek:_hookPrefabCthulu  :_baseHookPrefab,
                initialPos, 
                Quaternion.Euler(0, 0, orientation)
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
            this.islandIsGrabbedEnemy = false;
        }

        public void Start() {
            attachedToTarget = false;
            this.rigidbody.AddForce(transform.rotation * new Vector2(chain.initialForce, 0));
        }

        public void OnTriggerEnter2D(Collider2D coll) {
            if (!attachedToTarget) {
                IslandAnchorPoints anchorPoint = coll.gameObject.GetComponent<IslandAnchorPoints>();

                if (anchorPoint != null) {
                    this.currentAnchorPoint = anchorPoint;
                    this.AttachToIsland(anchorPoint);
                }
            }
        }

        /// <summary>Spawns the first chain section and attach to it</summary>
        /// <returns>The chain section</returns>
        public ChainSection SpawnChainSection(bool isSobek) {
            ChainSection chainSection = ChainSection.Create(
                transform.position, 
                this.rigidbody.transform.rotation, 
                this.chain, 
                this.gameObject, 
                isSobek,
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
            if (!this.chain._beginningHookIsSet) this.chain.islandChunkBeginningHook = anchor.GetIslandChunk();//must set the beginning hook island chunk
            else this.chain.islandChunkEndingHook = anchor.GetIslandChunk();//must set the ending hook island chunk

            if (this.chain.islandChunkBeginningHook != null) {
                if (this.chain.islandChunkEndingHook != null) {
                    if (this.chain.islandChunkEndingHook == this.chain.islandChunkBeginningHook) this.chain.DestroyChain(false); //IF THE TWO HOOKS ARE ON THE SAME ISLAND CHUNK -> DELETE THE CHAIN!
                    if (this.chain.islandChunkEndingHook._parentIsland != null && this.chain.islandChunkBeginningHook.parentIsland != null) {
                        if (this.chain.islandChunkEndingHook._parentIsland == this.chain.islandChunkBeginningHook.parentIsland) this.chain.DestroyChain(false); //IF THE TWO HOOKS ARE ON THE SAME ISLAND -> DELETE THE CHAIN!
                    }
                }
            }
                
            Island parentIsland = anchor.GetIslandChunk().parentIsland;

            // Must check if there is already a hook on this joint.
            // If so, we replace it with this one.
            chain.thrower.HookAlreadyOnIslandCheck(anchor);

            this.rigidbody.velocity = Vector2.zero;
            this.rigidbody.mass = this.ATTACHED_MASS;

            this.targetJoint.enabled = true;

            // Must check if there is already a player attached to the island
            bool wasAttachedToIsland = this.CheckOtherPlayerOnNewIsland(anchor);

            // Attach the joint to either the chunk or its parent island if it has one
            if (parentIsland == null && !wasAttachedToIsland) {
                    this.targetJoint.connectedBody = anchor.GetIslandChunk().GetComponent<Rigidbody2D>();
                if (chain.endingHook != null) chain.beginningHook.chainJoint.connectedBody = chain.endingHook.rigidbody;
            } 
            else if(!wasAttachedToIsland) {
                this.connectedIsland = parentIsland;
                this.targetJoint.connectedBody = parentIsland.rigidbody;
                if (this == this.chain.endingHook) {
                    chain.beginningHook.chainJoint.connectedBody = chain.endingHook.rigidbody;
                }
            }

            // Add listeners
            anchor.GetIslandChunk().MergeIntoIsland.AddListener(this.OnAttachedChunkMerge);
            anchor.GetIslandChunk().GrabbedByPlayer.AddListener(this.OnAttachedChunkPlayerGrab);
            anchor.GetIslandChunk().ReleasedByPlayer.AddListener(this.OnAttachedChunkPlayerRelease);

            this.Attach.Invoke();

            // The hook is now attached to a target
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

            // If the two hooks are united to the same Island, we destroy the hook
            bool endingIslandChunkFound = false;
            bool beginningIslandChunkFound = false;
            foreach (IslandChunk iChunk in newIsland.chunks) {
                if (iChunk == this.chain.islandChunkBeginningHook) beginningIslandChunkFound = true;
                else if (iChunk == this.chain.islandChunkEndingHook) endingIslandChunkFound = true;
            }
            if (beginningIslandChunkFound && endingIslandChunkFound) this.chain.DestroyChain(false);

        }

        /// <summary>React to attached chunk being grabbed by a player</summary>
        /// <param name="playerGrab">The player who grabbed the chunk</param> 
        private void OnAttachedChunkPlayerGrab(PlayerGrab playerGrab) {
            islandIsGrabbedEnemy = true;
            // Reroute the chain to the player only if both hooks exist
            if (chain.beginningHook != null) {
                if (this == chain.beginningHook || this == chain.endingHook) this.targetJoint.connectedBody = playerGrab.rigidbody;//AJOUT
            }
            // Otherwise, deactivate chain physics while the player is grabbing
            else {
                this.chainJoint.enabled = false;
            }
        }

        /// <summary>
        /// Check if there is already a player grabbing the island and set the target joint on the player if so 
        /// </summary>
        /// <param name="anchor">anchor of the attached island</param>
        /// <returns></returns>
        private bool CheckOtherPlayerOnNewIsland(IslandAnchorPoints anchor) {
            Island parentIsland = anchor.GetIslandChunk().parentIsland;
            // Attach the joint to either the chunk or its parent island if it has one
            if (parentIsland == null) {
                if (LevelManager.sobekPlayer.GetComponentInChildren<PlayerGrab>().grabbedBody != null) {
                    if (LevelManager.sobekPlayer.GetComponentInChildren<PlayerGrab>().grabbedBody.GetComponent<IslandChunk>() == anchor.GetIslandChunk()) { //IF SOBEK ATTACHED TO THE CHUNK SECTION
                        this.targetJoint.connectedBody = LevelManager.sobekPlayer.GetComponentInChildren<Rigidbody2D>();
                        islandIsGrabbedEnemy = true;
                    }
                } else if (LevelManager.cthulhuPlayer.GetComponentInChildren<PlayerGrab>().grabbedBody != null) {
                    if (LevelManager.cthulhuPlayer.GetComponentInChildren<PlayerGrab>().grabbedBody.GetComponent<IslandChunk>() == anchor.GetIslandChunk()) { //IF CTHULU ATTACHED TO THE CHUNK SECTION
                        this.targetJoint.connectedBody = LevelManager.cthulhuPlayer.GetComponentInChildren<Rigidbody2D>();
                        islandIsGrabbedEnemy = true;
                    }
                }
            } else if (LevelManager.sobekPlayer.GetComponentInChildren<PlayerGrab>().grabbedBody != null) {
                if (LevelManager.sobekPlayer.GetComponentInChildren<PlayerGrab>().
                    grabbedBody.gameObject.
                    GetComponentInChildren<IslandChunk>().
                    parentIsland == parentIsland) { //IF SOBEK ATTACHED TO THE ISLAND SECTION
                    if (this == this.chain.beginningHook) {
                        this.targetJoint.connectedBody = LevelManager.sobekPlayer.GetComponentInChildren<Rigidbody2D>();
                        islandIsGrabbedEnemy = true;
                    }
                }
            } else if (LevelManager.cthulhuPlayer.GetComponentInChildren<PlayerGrab>().grabbedBody != null) {
                if (LevelManager.cthulhuPlayer.GetComponentInChildren<PlayerGrab>().
                    grabbedBody.gameObject.
                    GetComponentInChildren<IslandChunk>()
                    .parentIsland == parentIsland) { //IF CTHULU ATTACHED TO THE ISLAND SECTION
                    if (this == this.chain.beginningHook) {
                        this.targetJoint.connectedBody = LevelManager.cthulhuPlayer.GetComponentInChildren<Rigidbody2D>();
                        islandIsGrabbedEnemy = true;
                    }
                }
            }
            return islandIsGrabbedEnemy;
        }

        /// <summary>React to attached chunk being released by a player</summary>
        /// <param name="rb">The rigidbody the player was holding</param> 
        private void OnAttachedChunkPlayerRelease(Rigidbody2D rb) {
            islandIsGrabbedEnemy = false;
            // Reroute the chain back to the island or chunk only if both hooks exist
            if (chain._beginningHookIsSet) {
                this.targetJoint.connectedBody = rb;
            }
            // Otherwise, reactivate chain physics when the player releases.
            // NOTE: This will cause a bug if the player throws the second hook of a chain 
            // before releasing the island, but this is not something that should be possible 
            // in the final build, so we don't account for it
            else {
                this.chainJoint.enabled = true;
            }
        }
    }
}
