using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Simoncouche.Chain {

    /// <summary>
    /// A Chain is a group of ChainSection game objects linked together, 
    /// with one Hook object at either end.
    /// </summary>
    public class Chain : MonoBehaviour {

        /// <summary>Self-reference to the chain prefab for factory purposes</summary>
        private static GameObject _chainPrefab;

        [Tooltip("The length of a single chain section")]
        [SerializeField]
        private float _chainSectionLength = 1;

        [Tooltip("Time until an attached (both hooks are attached to an island chain expires)")]
        [SerializeField]
        private float _timeUntilChainExpires = 10.0f;

        /// <summary>The first hook thrown by the player</summary>
        private Hook _beginningHook;
        
        /// <summary>The second hook thrown by the player</summary>
        private Hook _endingHook;

        /// <summary>The chain sections currently generated for visual effect</summary>
        private List<ChainSection> _chainSections;

        /// <summary> This is the maximum distance between two hooks</summary>
        private float _maxDistanceBetweenTwoHooks;

        /// <summary>Position of the thrower when he throws a hook</summary>
        private Vector3 throwerThrowPosition;

        /// <summary>In order to know if the beginning and the ending hook are set</summary>
        public bool _beginningHookIsSet { get; private set; }
        public bool _endingHookIsSet {  get;  private set; }  
       
        /// <summary>We have to disable the update when we play the sound on destroy (we disable the rendering and just play a sound here) </summary>
        private bool _isPlayingSoundOnDestroy = false;

        /// <summary>A reference to an islandChunk in order to take care of the case were 2 hooks are now on the same island</summary>
        public Islands.IslandChunk islandChunkBeginningHook;
        public Islands.IslandChunk islandChunkEndingHook;

        [Tooltip("Color flickered when the chain has done more than 50% of it's duration")]
        [SerializeField]
        private Color _chainFlickerColor;
        [Tooltip("Color when the chain has done 100% of it's duration")]
        [SerializeField]
        private Color _chainDamagedColor;

        [Tooltip("The color starts a slow danger flickering when the chain lasts this ratio of time out of the chain life time")]
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float _slowFlickerBeginningRatio = 0.5f;

        [Tooltip("The color starts a high danger flickering when the chain lasts this ratio of time out of the chain life time")]
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float _highFlickerBeginsAtRatio = 0.75f;

        [Tooltip("When the chain is flickering starts flickering at _slowFlickerBeginningPercentage of it's duration, it pass this amount of time between each cycle")]
        [SerializeField]
        private float flickerTime = 0.25f;

        [Tooltip("When the chain is at _highFlickerBeginsAtRatio of it's duration, we divide the flicker time by this factor to accelerate the rythm")]
        [SerializeField]
        private float flickerTimeDivider = 2f;

        public HookThrower thrower { get; set; }
        public float initialForce { get; set; }

        /// <summary>This is the sound which will be played on destroy of our chain over time</summary>
        private AudioSource _destroySoundSource;

        /// <summary>Spawn a new chain in the scene</summary>
        /// <param name="thrower">The game object that threw this chain</param>
        /// <param name="initialForce">The initial force to give to the first hook</param>
        public static Chain Create(HookThrower thrower, float initialForce) {
            if (_chainPrefab == null) {
                _chainPrefab = Resources.Load("Chain/Chain") as GameObject;
            }

            Chain chain = ((GameObject)Instantiate(
                _chainPrefab, 
                Vector3.zero, 
                Quaternion.identity
            )).GetComponent<Chain>();

            chain.thrower = thrower;
            chain.initialForce = initialForce;

            return chain;
        }

        public void Awake() {
            this._chainSections = new List<ChainSection>();
            this._destroySoundSource = this.GetComponentInChildren<AudioSource>();
            
        }

        public void Start() {
            this._endingHookIsSet = false;
            CreateBeginningHook();
            this._maxDistanceBetweenTwoHooks = _beginningHook.chainJoint.distance;
        }

        public void Update() {
            if (!_isPlayingSoundOnDestroy) {
                RecalculateChainSections();
                ChainMissAndHitUpdate();
                AttachedHookToIslandsUpdate();
            }
        }
            
        /// <summary>
        /// Sync the number of chain sections with the current 
        /// distance between the two edges of the chain
        /// </summary>
        private void RecalculateChainSections() {
            Vector3 chainBeginning = _beginningHook.transform.position;
            Vector3 chainEnding = (_endingHook != null)
                ? _endingHook.transform.position
                : this.thrower.transform.position;

            int neededSections = (int)(Vector3.Distance(chainBeginning, chainEnding) / _chainSectionLength);

            // Too few sections : Create more sections until we achieve the right number
            while (_chainSections.Count < neededSections) {
                if (_chainSections.Count == 0) {
                    _chainSections.Add(_beginningHook.SpawnChainSection(thrower.isPlayerOne));
                } else {
                    _chainSections.Add(_chainSections[_chainSections.Count - 1].SpawnNewSection());
                }
            }

            // Too many sections : Destroy the links until we achieve the right number
            while (_chainSections.Count > neededSections) {
                _chainSections[_chainSections.Count - 1].Remove();
                _chainSections.RemoveAt(_chainSections.Count - 1);
            }
        }
            
        /// <summary>Create and configure the beginning hook</summary>
        public void CreateBeginningHook() {
            _beginningHook = Hook.Create(this, true, this.thrower.isPlayerOne);

            // Position where the player threw the hook
            throwerThrowPosition = this.thrower.transform.position;
        }

        /// <summary>Create and configure the ending hook</summary>
        public void CreateEndingHook() {
            _endingHook = Hook.Create(this, false, this.thrower.isPlayerOne); 

            // Reroute the visual chain from the player to the ending hook
            _chainSections[_chainSections.Count - 1].joint.connectedBody = _endingHook.rigidbody;

            // Set up listeners
            _endingHook.Attach.AddListener(this.OnEndingHookAttach);

            // Position where the player threw the hook
            throwerThrowPosition = this.thrower.transform.position;
        } 

        /// <summary>React to ending hook attaching itself to an island.</summary>
        private void OnEndingHookAttach() {
            // Clamp max distance to current distance between the two hooks
            this.SetMaxChainDistance(Vector3.Distance(
                _beginningHook.transform.position,
                _endingHook.transform.position
            ));
        }

        /// <summary>Sets the max distance that the chain can have.</summary>
        /// <param name="distance">Distance.</param>
        private void SetMaxChainDistance(float distance) {
            _beginningHook.chainJoint.distance = distance;
        }

        /// <summary>
        /// Check if the connected islands of the hooks are null.  If they are, we destroy the chain.
        /// TODO: CHANGE THIS TO AN EVENT CALL
        /// </summary>
        private void AttachedHookToIslandsUpdate() {
            bool mustDestroyChain = false;
            if (_beginningHookIsSet) {
                if(_beginningHook.targetJoint.connectedBody == null) {
                    mustDestroyChain = true;
                }else if (_endingHookIsSet) {
                    if(_endingHook.targetJoint.connectedBody == null) {
                        mustDestroyChain = true;
                    }
                }
            }
            if (mustDestroyChain) DestroyChain(true);
        }

        /// <summary>
        /// We check check if our hooks are beyond a certain distance from the player, if so they are considered as a miss if they didnt hit a island
        /// </summary>
        private void ChainMissAndHitUpdate() {
            if (!this._beginningHookIsSet) {
                if (this._beginningHook.attachedToTarget) {
                    //Tells to the thrower he hit the hook
                    this.thrower.OnBeginningHookHit();

                    //Apply the maximum distance of the first hook (maxDistanceBetweenTwoHooks divided by 2)
                    this._beginningHook.chainJoint.distance = this._maxDistanceBetweenTwoHooks/2;

                    //Set this bool to true in order to stop the calls of thrower.BeginningHookHit
                    this._beginningHookIsSet = true;
                }

                //If the hook returns AND if it isn't attached (cause it can attach to an island when it's returning!) AND the distance between the hook and player is lower then a certain amount
                    //Then we destroy the chain
                else if (!_beginningHook.attachedToTarget &&
                    Vector2.Distance(_beginningHook.transform.position, throwerThrowPosition) > _maxDistanceBetweenTwoHooks / 2) {
                    DestroyBeginningHook(false);
                }
            }

            if (_endingHook != null && !_endingHookIsSet) {
                if (_endingHook.attachedToTarget) {
                    ///Tells to the thrower he hit the hook
                    this.thrower.OnEndingHookHit();

                    // Reroute the beginning hook from the player to the ending hook
                    this._beginningHook.chainJoint.connectedBody = _endingHook.rigidbody;
                    //this._beginningHook.chainJoint.distance = _maxDistanceBetweenTwoHooks;

                    //Set this bool to true in order to stop the calls of thrower.EndingHookHit
                    _endingHookIsSet = true;

                    StartCoroutine(DestroyTimer());
                }

                //Check if the ending hook is further than 
                else if (!_endingHook.attachedToTarget &&
                    Vector2.Distance(_endingHook.transform.position, throwerThrowPosition) > _maxDistanceBetweenTwoHooks / 2) {
                    DestroyEndingHook();
                }
            }
        }

        /// <summary>
        /// Destroy the chain when we miss the first throw
        /// </summary>
        private void DestroyBeginningHook(bool mustPlaySound) {
            //Tells the owner he missed the beginning hook
            this.thrower.OnBeginningHookMissed();

            this.DestroyChain(mustPlaySound);
        }

        /// <summary>
        /// Destroy the second part of the chain on second hook throw miss
        /// </summary>
        private void DestroyEndingHook() {
            _chainSections[_chainSections.Count - 1].AttachVisualJointTo(this.thrower.rigidbody);
            
            Destroy(_endingHook.gameObject);

            //Tells the thrower he missed the ending hook throw
            this.thrower.OnEndingHookMissed();

            //Must reset the connected rigidbody!
            this._beginningHook.chainJoint.connectedBody = this.thrower.rigidbody;
        }

        /// <summary>
        /// When an attached chain has passed a certain amount of time, we destroy it using this function
        /// </summary>
        public void DestroyChain(bool playDestroySound) {
            foreach (ChainSection section in _chainSections) {
                Destroy(section.gameObject);
            }

            if (playDestroySound) this.PlayDestroySound();

            Destroy(_beginningHook.gameObject);

            if(_endingHook!=null) Destroy(_endingHook.gameObject);

            this.thrower.RemoveChainFromChains(this);

            if (this._isPlayingSoundOnDestroy) { //IF the hook is destroyed and is playing a sound
                Destroy(this.gameObject, GameManager.audioManager.chainSound.chainDestruction.length); 
            } else Destroy(this.gameObject);
        }

        /// <summary>
        /// Update the color of the chain sections and destroy the chain after a certain amount of time
        /// </summary>
        /// <returns></returns>
        IEnumerator DestroyTimer() {
            float elapsedTime = 0.0f;
            float elapsedTimeFlickering = 0.0f;
            bool flickerRythmAccelerated = false;
            while (elapsedTime < _timeUntilChainExpires) {
                if (elapsedTime > _timeUntilChainExpires * _highFlickerBeginsAtRatio && !flickerRythmAccelerated) {
                    flickerRythmAccelerated = true;
                    flickerTime /= flickerTimeDivider;
                }

                if (elapsedTime > _timeUntilChainExpires * _slowFlickerBeginningRatio && elapsedTimeFlickering < flickerTime/2) {
                    foreach (ChainSection section in _chainSections) {
                        if (section != null) {
                            MeshRenderer mr = section.GetComponentInChildren<MeshRenderer>();
                            mr.material.color = _chainFlickerColor;
                        }
                    }
                    elapsedTimeFlickering += Time.deltaTime;
                }

                if (elapsedTimeFlickering > flickerTime/2 || elapsedTime < _timeUntilChainExpires * _slowFlickerBeginningRatio) {
                    elapsedTimeFlickering += Time.deltaTime;
                    if (elapsedTimeFlickering > flickerTime) elapsedTimeFlickering = 0.0f;
                    UpdateChainSectionsColor(elapsedTime);
                }
                elapsedTime += Time.deltaTime;
                
                yield return null;
            }
            DestroyChain(true);
        }

        /// <summary>Plays a destroy sound</summary>
        private void PlayDestroySound() {
            this._destroySoundSource.PlayOneShot(GameManager.audioManager.chainSound.chainDestruction);
            this._isPlayingSoundOnDestroy = true;
        }

        /// <summary>
        /// Simple function in order to lerp the color of the chain to red the longer it stays attached
        /// </summary>
        /// <param name="time"></param>
        private void UpdateChainSectionsColor(float time) {
            foreach (ChainSection section in _chainSections) {
                if (section != null) {
                    MeshRenderer mr = section.GetComponentInChildren<MeshRenderer>();
                    mr.material.color = Color.Lerp(Color.white, _chainDamagedColor, time / _timeUntilChainExpires);
                }
            }
        }

        ///<summary>Retracts the chain modifying chainJoint.distance</param>
        ///<param name="retractDistance">The distance to retract per tick of this function</param>
        /// <returns>If the island connected to the player was destroyed</returns>
        public bool RetractChain(float retractDistance) {
            if (_beginningHook.chainJoint != null) {
                if (!_endingHookIsSet) {
                    _beginningHook.chainJoint.distance = 
                        Vector2.Distance(_beginningHook.chainJoint.connectedBody.transform.position, _beginningHook.transform.position);
                }

                float tempDistance = _beginningHook.chainJoint.distance;
                tempDistance = Mathf.Clamp(tempDistance - retractDistance, 0f,_beginningHook.chainJoint.distance);
                _beginningHook.chainJoint.distance = tempDistance;

                //We first check with a single chunk if it collides with the player
                if (islandChunkBeginningHook != null) {
                    if (islandChunkBeginningHook.gravityBody.rigidbody.GetComponent<Collider2D>().IsTouching(thrower.rigidbody.GetComponent<Collider2D>())) {
                        return true;
                    } else if (islandChunkBeginningHook.parentIsland != null) {//We Then check with all the chunks of the Island if it collides with the player
                        if (islandChunkBeginningHook.parentIsland.gravityBody.rigidbody.GetComponent<Collider2D>().IsTouching(thrower.rigidbody.GetComponent<Collider2D>())) {
                            return true;
                        }
                        foreach (Islands.IslandChunk chunk in islandChunkBeginningHook.parentIsland.chunks) {
                            if (chunk.gravityBody.rigidbody.GetComponent<Collider2D>().IsTouching(thrower.rigidbody.GetComponent<Collider2D>())) return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// In order to give the player some place to move around when he ends the retraction of a chain connected to the player
        /// </summary>
        public void RetractChainReleaseBehaviour() {
            if (_beginningHookIsSet && !_endingHookIsSet) {
                _beginningHook.chainJoint.distance = _maxDistanceBetweenTwoHooks/2; //Cause only 1 chain is set
            }
        }

        /// <summary>
        /// Cut the link of the beginning hook if and only if the ending hook is null
        /// </summary>
        public void CutLinkBeginningHook() {
            if (_beginningHook != null) {
                if (_beginningHook.chainJoint.connectedBody == thrower.rigidbody) {
                    this.PlayDestroySound();
                    DestroyChain(true);
                }
            }
        }

        public bool bothHooksExist {
            get {
                return (_beginningHook != null && _endingHook != null);
            }
        }

        /// <summary>
        /// Allows us to check if the anchorPoint of a new hook is already present on an older hook
        /// </summary>
        /// <param name="anchorPoint">the anchor point of a new hook</param>
        /// <returns>boolean that specifies if the passed anchorPoint corresponds to the connected anchor Point of one of our two hooks</returns>
        public bool CheckAnchorPointInHooks(Islands.IslandAnchorPoints anchorPoint){
            bool sameAnchorPoint = ((anchorPoint == _beginningHook.currentAnchorPoint && anchorPoint.GetIslandChunk()==_beginningHook.currentAnchorPoint.GetIslandChunk())
                || (anchorPoint == _endingHook.currentAnchorPoint && anchorPoint.GetIslandChunk() == _endingHook.currentAnchorPoint.GetIslandChunk()));
            return sameAnchorPoint;
        }

        /// <summary>
        /// Attach beginning hook's target to the player grab
        /// </summary>
        public void AttachBeginningHookTargetToPlayer() {
            thrower.playerGrab.AttemptGrabOnHookRetraction(islandChunkBeginningHook.gameObject.GetComponent<GravityBody>()); //MAKES THE PLAYER GRAB THE ISLAND
        }
    }
}
