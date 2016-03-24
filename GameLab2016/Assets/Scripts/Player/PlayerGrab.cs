using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Simoncouche.Islands;
using Simoncouche.Chain;

namespace Simoncouche.Controller {
    
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerGrab : MonoBehaviour {

        //Attributes
        [SerializeField] [Tooltip("Magnitude of the force applied to the thrown gravity body. This is reduces by the grabbed object's weight")]
        private float THROW_FORCE = 15f;
        [SerializeField] [Tooltip("Minimum force to apply to object no matter the grabbed Object weight. ")]
        private float MIN_THROW_FORCE = 9f;
        [SerializeField] [Tooltip("Maximum multiplier of the force when charged")]
        private float CHARGED_MAX_MULTIPLIER = 2f;
        [SerializeField] [Tooltip("Time to reach full charge")]
        private float CHARGE_TIME = 3f;

       

        
        [SerializeField][Tooltip("Angle in front of player at which the player can grab an island. If not in this angle, the island will just bump in the player.")]
        private float MAX_GRAB_ANGLE = 75f;

        [SerializeField] [Tooltip("Cooldown before a player can grab again after releasing")]
        private float GRAB_COOLDOWN = 0.5f;

        /// <summary> Parent of grabbed gravity body. Used to reposition  body at the right place when releasing. </summary>
        private Transform _grabbedBodyParent = null;
        /// <summary> Wheter the player is currently in grab cooldown or not</summary>
        private bool _inGrabCooldown = false;
        /// <summary> Is the grab trigger currently held or not</summary>
        private bool _triggerIsHeld = false;
        /// <summary> current charge force multiplier. Goes up the more trigger is held.</summary>
        private float _curChargeMultiplier = 1f;
        /// <summary> Has maxed charge been reached </summary>
        private bool _maxChargeReached = false;

        //Components
        /// <summary> Currently Grabbed GravityBody</summary>
        public GravityBody grabbedBody { get; private set; }
        public new Rigidbody2D rigidbody { get; private set; }

        /// <summary> Reference to the aim controller </summary>
        private AimController _aimController;
        /// <summary> Reference to the player controller </summary>
        private PlayerController _playerController;
        /// <summary> Reference to the playerAudio of the player</summary>
        private PlayerAudio _playerAudio;
        /// <summary> Reference to the gravityBody of the player</summary>
        private GravityBody _playerGravityBody;
        /// <summary> Reference to the hookThrower of the player</summary>
        private HookThrower _hookThrower;
        /// <summary> List of all particle systems used for charge animation </summary>
        private List<ParticleSystem> _chargeParticles;
        private List<ParticleSystem> _chargeParticlesMax;

        /// <summary> List of references to all playerGrabs. Used to avoid Searching the map everytime we grab.</summary>
        private static List<PlayerGrab> _allPlayerGrabs = new List<PlayerGrab>();

        /// <summary> List of currently running coroutines. Used to keep track of running coroutine that re-enable collision in case we need to stop them. </summary>
        private Dictionary<string, Coroutine> collisionCoroutines = new Dictionary<string, Coroutine>();


        void Awake() {
            _aimController = GetComponent<AimController>();
            _playerController = GetComponent<PlayerController>();
            _playerAudio = GetComponent<PlayerAudio>();
            _playerGravityBody = GetComponent<GravityBody>();
            _hookThrower = GetComponent<HookThrower>();
            _chargeParticles = new List<ParticleSystem>(transform.Find("P_ChargeParticles").GetComponentsInChildren<ParticleSystem>());
            _chargeParticlesMax = new List<ParticleSystem>(transform.Find("P_MaxChargeParticles").GetComponentsInChildren<ParticleSystem>());
            grabbedBody = null;
            this.rigidbody = GetComponent<Rigidbody2D>();

            //Add to static playergrab list
            _allPlayerGrabs.Add(this);

            if (_aimController == null) {
                Debug.LogError("Player/AimController cannot be found!");
            }
            if(_playerController == null) {
                Debug.LogError("Player/PlayerController cannot be found!");
            }
            if (_playerAudio == null) {
                Debug.LogError("Player/PlayerAudio cannont be found!");
            }
            if (_playerGravityBody == null) {
                Debug.LogError("Player/PlayerGravityBody cannont be found!");
            }
        }

        void Start() {
            ToggleChargeParticles(false); // toggle off particles
            ToggleMaxChargeParticles(false); //toggle off max particles
        }

        void Update() {
            //Charge if trigger is held
            if (_triggerIsHeld) {
                Charge();
            }
        }

        void OnDestroy() {
            //Remove ref to in static playergrab
            _allPlayerGrabs.Remove(this);
        }

        public void SetupInput(bool isPlayerOne) {
            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Axis.p1_rightTrigger : InputManager.Axis.p2_rightTrigger,
                this.CheckPlayerInputs
            );

            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_cutLinkWithChainButton : InputManager.Button.p2_cutLinkWithChainButton,
                this.Release
            );

            //For Keyboard use
#if UNITY_EDITOR
            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_pushButton : InputManager.Button.p2_pushButton,
                this.Throw
            );
            #endif
        }

        /// <summary> Get weight of currently grabbed object </summary>
        /// <returns> float representign the weight</returns>
        public float GetGrabbedWeight() {
            if (grabbedBody == null) return 0f;
            IslandChunk c = grabbedBody.GetComponent<IslandChunk>();
            Island i = c.parentIsland;
            if (i == null) return c.weight;
            else return i.weight;

        }

        #region Grab & Release

        /// <summary> Attemps to grab gravity body if one is not already grabbed</summary>
        /// <param name="targetBody">target gravity body to grab</param>
        public void AttemptGrab(GravityBody targetBody) {
            if (grabbedBody == null && _inGrabCooldown == false) {
                //Check if the grab andle isn't too big
                //Get angle between playerfacing and relative target position
                float playerTargetAngle = Vector2.Angle(transform.right, targetBody.transform.position - transform.position);
                //Debug.Log(gameObject.name + "  " + playerTargetAngle);
                if (playerTargetAngle <= MAX_GRAB_ANGLE) {  
                    //Make sure we are trying to grab an IslandChunk
                    IslandChunk targetChunk = targetBody.gameObject.GetComponent<IslandChunk>();
                    if (targetChunk != null && !_hookThrower.isHookAttachedToPlayer) { 
                        //Initiate Grab
                        Grab(targetBody, targetChunk, 0f, true);


                    }
                }
            }
        }

        /// <summary> Attemps to grab gravity body if one is not already grabbed BUT DOESNT CHECK THE ANGLE</summary>
        /// <param name="targetBody">target gravity body to grab</param>
        public void AttemptGrabOnHookRetraction(GravityBody targetBody) {
            if (grabbedBody == null && _inGrabCooldown == false) {
                //Make sure we are trying to grab an IslandChunk
                IslandChunk targetChunk = targetBody.gameObject.GetComponent<IslandChunk>();
                if (targetChunk != null && !_hookThrower.isHookAttachedToPlayer) {
                    //Initiate Grab
                    Grab(targetBody, targetChunk, 0f, true);
                }
            }
        }

        private void Grab(GravityBody targetBody, IslandChunk targetChunk, float repositionDelay, bool rumble) {
            //Before grabbing, make the other players release this chunk
            MakeOtherPlayerRelease(targetChunk);
            grabbedBody = targetBody;
            _hookThrower.OnPlayerIslandGrab();

            if (targetChunk.parentIsland == null) {
                //Set parent
                _grabbedBodyParent = targetChunk.transform.parent;
                targetChunk.transform.parent = transform;
                //Ignore Collision
                DeactivateCollision(targetChunk.GetComponent<Collider2D>());
                //Deactivate gravity body
                grabbedBody.DeactivateGravityBody();
                //Set weight
                _playerGravityBody.Weight = grabbedBody.Weight + _playerController._startPlayerWeight;

                //Move object in player's arm 
                StartCoroutine(RepositionGrabbedBody(targetChunk.transform, repositionDelay));

                targetChunk.GrabbedByPlayer.Invoke(this);

            } else {
                Island parentIsland = targetChunk.parentIsland;
                //Set parent
                _grabbedBodyParent = parentIsland.transform.parent;
                parentIsland.transform.parent = transform;
                //Ignore Collision
                foreach (IslandChunk iChunk in parentIsland.GetComponentsInChildren<IslandChunk>()) {
                    DeactivateCollision(iChunk.GetComponent<Collider2D>());
                }
                //Deactivate gravity body
                parentIsland.GetComponent<GravityBody>().DeactivateGravityBody();
                //Set weight
                _playerGravityBody.Weight = parentIsland.GetComponent<GravityBody>().Weight + _playerController._startPlayerWeight;
                //Move object in player's arm 
                StartCoroutine(RepositionGrabbedBody(parentIsland.transform, repositionDelay));

                parentIsland.GrabbedByPlayer.Invoke(this);
                parentIsland.islandColliders.MoveCollisionToPlayer(transform);
            }

            //Check grab particles 
            if(_triggerIsHeld) {
                ToggleMaxChargeParticles(true);
            }
            //Animation
            _playerController.HandleGrabStartAnimation();
            //Sounds
            _playerAudio.PlaySound(PlayerSounds.PlayerGrab);
            //Rumble
            if (rumble) {
                if (_playerController.IsPlayerOne) GameManager.inputManager.RumbleGamepad_Light(0);
                else GameManager.inputManager.RumbleGamepad_Light(1);
            }


        }

        /// <summary>
        /// Repositions grabbed object in front of player to give grabbinh visual
        /// </summary>
        /// <param name="transformToMove"> Transform that was grabbed </param>
        /// <param name="delay">delay before starting repositioning. Used in cases where island get merged at the same time</param>
        /// <returns></returns>
        IEnumerator RepositionGrabbedBody(Transform transformToMove, float delay) {
            yield return new WaitForSeconds(delay);
            if (transformToMove == null) Debug.LogWarning("Attempted to reposition grabbed transform after " + delay + " seconds but it was null.");
            else if(grabbedBody == null) Debug.LogWarning("Attempted to reposition grabbed transform after " + delay + " seconds but grabbedBody was null.");
            else {
                //Get positions
                float i = 0f;
                float repositionTime = 1f;
                Vector2 startPosition = transformToMove.localPosition;
                Vector2 targetPosition = new Vector2(1.2f, 0);
                if (grabbedBody.transform != transformToMove) targetPosition -= (Vector2)(transformToMove.localRotation * (grabbedBody.transform.localPosition * transformToMove.localScale.x));

                //Lerp to target position
                while (i < 1f && grabbedBody != null && transformToMove != null) {   
                    transformToMove.localPosition = Vector3.Lerp(startPosition, targetPosition, i);
                    i += Time.deltaTime / repositionTime;
                    yield return null;
                }
                //finish movement
                if (grabbedBody != null && transformToMove != null) transformToMove.localPosition = targetPosition;
            }
        }

        /// <summary> Charges the throw multiplier </summary>
        private void Charge() {
            float rate = (CHARGED_MAX_MULTIPLIER - 1f) / CHARGE_TIME;
            _curChargeMultiplier += rate * Time.deltaTime;
            _curChargeMultiplier = Mathf.Clamp(_curChargeMultiplier, 1f, CHARGED_MAX_MULTIPLIER);

            if (!_maxChargeReached && _curChargeMultiplier == CHARGED_MAX_MULTIPLIER) {
                _maxChargeReached = true;
                ToggleMaxChargeParticles(true); //emit ready particles
                ToggleChargeParticles(false); //stop emitting charge particles
            }
        }

        /// <summary> Toggle the emission of charge particles </summary>
        /// <param name="on"></param>
        private void ToggleChargeParticles(bool on) {
            foreach (ParticleSystem ps in _chargeParticles) {
                ParticleSystem.EmissionModule em = ps.emission;

                if (on) {
                    ps.Simulate(0,false,true);
                    ps.Play();
                }

                em.enabled = on;
            }
        }

        private void ToggleMaxChargeParticles(bool on) {
            foreach (ParticleSystem ps in _chargeParticlesMax) {
                ParticleSystem.EmissionModule em = ps.emission;

                if (on) {
                    ps.Simulate(0, false, true);
                    ps.Play();
                }

                em.enabled = on;
            }
        }

        /// <summary> Throw gravity body in direction of player's aim controller</summary>
        private void Throw() {
            if(grabbedBody != null) {
                //Get Body to add force to
                GravityBody bodyToAddForce = grabbedBody;
                IslandChunk targetChunk = grabbedBody.gameObject.GetComponent<IslandChunk>();
                if (targetChunk.parentIsland != null) {
                    bodyToAddForce = targetChunk.parentIsland.gravityBody;
                }
                //Release
                Release();
                //Add Force
                Vector2 forceDirection = _aimController.aimOrientationVector2.normalized;
                float finalThrowForce = Mathf.Max(MIN_THROW_FORCE, THROW_FORCE / Mathf.Max(1, bodyToAddForce.Weight / 10f));
                bodyToAddForce.Velocity = forceDirection * finalThrowForce * _curChargeMultiplier;
                if (targetChunk.color == IslandUtils.color.volcano) {
                    targetChunk.volcanoActive = true;
                }
                //Remove Force from player
                _playerGravityBody.Velocity /= 4f;

                //Animation
                _playerController.HandlePushAnimation();
                //Sounds
                _playerAudio.PlaySound(PlayerSounds.PlayerPush);
                //Rumble
                if (_playerController.IsPlayerOne) GameManager.inputManager.RumbleGamepad_Light(0);
                else GameManager.inputManager.RumbleGamepad_Light(1);
            } else {
                Debug.LogWarning("Attempted to throw when grabbedBody is null.");             
            }

            //Reset charge multiplier
            StartCoroutine(ResetChargeMultiplier());

        }

        /// <summary>
        /// Releases the gravity body.
        /// Raises the ReleasedByPlayer event in the targeted Island or IslandChunk.
        /// </summary>
        public void Release() {
            if (grabbedBody != null) {
                IslandChunk targetChunk = grabbedBody.gameObject.GetComponent<IslandChunk>();

                //If chunk has no parent island
                if (targetChunk.parentIsland == null) {
                    //Unparent chunk & activate GravBody
                    targetChunk.transform.parent = _grabbedBodyParent;
                    _grabbedBodyParent = null;
                    grabbedBody.ActivateGravityBody();
                    //UnIgnore Collision
                    ReactivateCollision(targetChunk.GetComponent<Collider2D>(), 0.5f);

                    targetChunk.ReleasedByPlayer.Invoke(targetChunk.gravityBody.rigidbody);
                }

                //If chunk has a parent island
                else {
                    //Get island and reactivate it
                    Island parentIsland = targetChunk.parentIsland;
                    parentIsland.gravityBody.ActivateGravityBody();
                    //Reparent island
                    parentIsland.transform.parent = _grabbedBodyParent;
                    _grabbedBodyParent = null;
                    //UnIgnore Collision
                    foreach (IslandChunk iChunk in parentIsland.GetComponentsInChildren<IslandChunk>()) {
                        ReactivateCollision(iChunk.GetComponent<Collider2D>(), 0.5f);
                    }

                    parentIsland.ReleasedByPlayer.Invoke(parentIsland.rigidbody);
                    parentIsland.islandColliders.MoveCollisionBackToIsland();
                }

                //Mark grabbed body as null
                grabbedBody = null;

                //Set weight
                _playerGravityBody.Weight =  _playerController._startPlayerWeight;
                //Start Cooldown
                StartCoroutine(GrabCooldown());

                //Animation
                _playerController.HandleGrabStopAnimation();
            } else {
                Debug.LogWarning("Grabbed body is null and trying to release !");
            }

            //Reset charge multiplier
            StartCoroutine(ResetChargeMultiplier());

        }

        /// <summary>
        /// Reset charge multiplier
        /// Use a coroutine to deactivate charge multiplier since it can be deactivated in both release and Throw but the values are still needed for throw.
        /// </summary>
        IEnumerator ResetChargeMultiplier() {
            yield return new WaitForEndOfFrame();
            //reset charge multiplier        
            _curChargeMultiplier = 1f;
            _maxChargeReached = false;
            ToggleChargeParticles(false);
            ToggleMaxChargeParticles(false);
        }

        /// <summary> Handles the cooldown of the grab </summary>
        IEnumerator GrabCooldown() {
            _inGrabCooldown = true;
            yield return new WaitForSeconds(GRAB_COOLDOWN);
            _inGrabCooldown = false;
        }

        public static void ReactivateCollisionForBothPlayer(Collider2D otherCol, IslandChunk chunk) {
            ReactivateCollisionForBothPlayer(otherCol, chunk, 0);
        }

        public static void ReactivateCollisionForBothPlayer(Collider2D otherCol, IslandChunk chunk, float time) {
            foreach (PlayerGrab grab in _allPlayerGrabs) {
                if (grab.grabbedBody != chunk.gravityBody && chunk.parentIsland != null && grab.grabbedBody != chunk.parentIsland.gravityBody)
                    grab.ReactivateCollision(otherCol, time);
            }
        }

        /// <summary> Calls the coroutine that resume collisions and add its reference to the coroutine list</summary>
        /// <param name="otherCol">collider to unIgnore</param>
        /// <param name="time">time before unignore</param>
        void ReactivateCollision(Collider2D otherCol, float time) {

            if (time <= 0f) Physics2D.IgnoreCollision(GetComponent<Collider2D>(), otherCol, false);
            else {
                Coroutine disableCoroutine = StartCoroutine(ResumeCollision(otherCol, time));

                //Add coroutine to a dictionnary until is done in case it needs to be interrupted
                string colID = otherCol.GetInstanceID().ToString();
                if (collisionCoroutines.ContainsKey(colID)) collisionCoroutines.Remove(colID);
                collisionCoroutines.Add(colID, disableCoroutine);
            }
        }

        /// <summary> Resume collision with given collider2D after a amount of time </summary>
        /// <param name="otherCol">collider to unIgnore</param>
        /// <param name="time">time before unignore</param>
        /// <returns></returns>
        IEnumerator ResumeCollision(Collider2D otherCol, float time) {
            yield return new WaitForSeconds(time);
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), otherCol, false);

            //Remove coroutine from dictionnary since it is finished
            string colID = otherCol.GetInstanceID().ToString();
            if (collisionCoroutines.ContainsKey(colID)) {
                collisionCoroutines.Remove(colID);
            }
        }

        private void DeactivateCollision(Collider2D otherCol) {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), otherCol.GetComponent<Collider2D>(), true);

            //if collider was pending for reactivation of collision, stop that coroutine since we juste deactivated its collision
            string colID = otherCol.GetComponent<Collider2D>().GetInstanceID().ToString();
            if (collisionCoroutines.ContainsKey(colID)) {
                StopCoroutine(collisionCoroutines[colID]);
                collisionCoroutines.Remove(colID);
            }
        }

        #endregion

        #region Static Utils

        /// <summary>
        /// Is the body grabbed by a player
        /// </summary>
        /// <param name="body">the body to check</param>
        /// <returns></returns>
        public static bool BodyIsGrabbed(GravityBody body) {
            foreach (PlayerGrab player in _allPlayerGrabs) {
                if (body == player.grabbedBody) {
                    return true;
                }
            }
            return false;
        }

        /// <summary> Make other player release the target chunk if they're holding it.</summary>
        /// <param name="targetChunk">Chunk to release</param>
        private static void MakeOtherPlayerRelease(IslandChunk targetChunk) {
            //Get list of chunks to try and make release
            List<IslandChunk> ChunksToTest = new List<IslandChunk>();
            if (targetChunk.parentIsland == null) ChunksToTest.Add(targetChunk);
            else foreach(IslandChunk childChunk in targetChunk.parentIsland.chunks) ChunksToTest.Add(childChunk);

            //Check each player to see if they are grabbing predetermined chunk
            foreach (PlayerGrab pg in _allPlayerGrabs) {
                foreach (IslandChunk chunks in ChunksToTest) {
                    if (pg.grabbedBody == chunks.gravityBody) {                    
                        //Debug.Log(pg.name + " ungrabbed " + chunks.name + " because someone else grabbed it.");
                        pg.Release();
                    }
                }
            }
        }

        /// <summary> Check if any player is currently grabbing bodyToMerge. If so, make that player release the object </summary>
        /// <param name="bodyToMerge">Gravity body of the body to merge</param>
        public static void UngrabBody(GravityBody bodyToMerge) {
            UngrabBody(bodyToMerge, false);
        }

        /// <summary> Check if any player is currently grabbing bodyToMerge. If so, make that player release the object </summary>
        /// <param name="bodyToMerge">Gravity body of the body to merge</param>
        /// <param name="regrab">Regrab body after drop or not</param>
        public static void UngrabBody(GravityBody bodyToMerge, bool regrab) {
            foreach (PlayerGrab pg in _allPlayerGrabs) {
                //Debug.Log("checking if " + pg.name + " is grabbing " + bodyToMerge + "      he's grabbing " + pg.grabbedBody);
                if (pg.grabbedBody == bodyToMerge) {
                    //Debug.Log(pg.name + " ungrabbed " + bodyToMerge.name + " because it was merged.");
                    pg.Release();
                    if (regrab) pg.Grab(bodyToMerge, bodyToMerge.gameObject.GetComponent<IslandChunk>(), GameManager.islandManager.GetMergeTime(), false);
                }
            }
        }

        /// <summary> Check if any player is currently grabbing bodyToMerge. If so, make that player release the object </summary>
        /// <param name="bodyToMerge">Gravity body of the body to merge</param>
        /// <param name="regrab">Regrab body after drop or not</param>
        /// <param name="regrabDelay">Delay before regrabbing</param>
        public static void UngrabBody(GravityBody bodyToMerge, bool regrab, float regrabDelay) {
            foreach (PlayerGrab pg in _allPlayerGrabs) {
                //Debug.Log("checking if " + pg.name + " is grabbing " + bodyToMerge + "      he's grabbing " + pg.grabbedBody);
                if (pg.grabbedBody == bodyToMerge) {
                    Debug.Log(pg.name + " ungrabbed " + bodyToMerge.name + " with regrab delay of " + regrabDelay);
                    pg.Release();
                    if (regrab) pg.StartRegrabCoroutine(bodyToMerge, regrabDelay);
                }
            }
        }

        /// <summary> Starts a coroutine that regrab a gravity body adter a delay </summary>
        /// <param name="bodyToMerge">body to grab</param>
        /// <param name="delay">delay before grab</param>
        private void StartRegrabCoroutine(GravityBody bodyToGrab, float delay) {
            StartCoroutine(RegrabCoroutine(bodyToGrab, delay));
        }
        /// <summary> Coroutine that regrab after a delay</summary>
        private IEnumerator RegrabCoroutine( GravityBody bodyToGrab, float delay) {
            yield return new WaitForSeconds(delay);
            if (bodyToGrab != null) {
                Grab(bodyToGrab, bodyToGrab.gameObject.GetComponent<IslandChunk>(), GameManager.islandManager.GetMergeTime(), false);
            } else Debug.LogWarning("Attempted to regrab an body which has been destroyed. No grab occured.");
        }


        /// <summary> Check if anyplayer is grabbing island, if so make him ignore chunk </summary>
        /// <param name="island"></param>
        /// <param name="Chunk"></param>
        public static void RemoveCollisionIfGrabbed(Island island, IslandChunk Chunk) {
            foreach (PlayerGrab pg in _allPlayerGrabs) {
                if (pg.grabbedBody != null && pg.grabbedBody.GetComponent<IslandChunk>().parentIsland == island) {
                    //Debug.Log(pg.name + " ignored " + Chunk);
                    pg.DeactivateCollision(Chunk.GetComponent<Collider2D>());
                }
            }
        }

        #endregion


        private void CheckPlayerInputs(params float[] input) {
            bool isCurrentlyHeld = (input[0] == 1);
            if ((_triggerIsHeld && !isCurrentlyHeld) || (_triggerIsHeld && grabbedBody ==null)) { //If just stop pressing OR there is no island grabbed
                Debug.Log("gb : " + grabbedBody + "  tri : " + _triggerIsHeld);
                ToggleChargeParticles(false);
                ToggleMaxChargeParticles(false);
                _triggerIsHeld = false;
                Throw();
            } else if (!_triggerIsHeld && isCurrentlyHeld && grabbedBody != null) {//If just started pressing
                ToggleChargeParticles(true);
                _triggerIsHeld = true; 
            }
        }
    }
}
