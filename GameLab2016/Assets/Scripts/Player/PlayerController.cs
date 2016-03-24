using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Simoncouche.Chain;

namespace Simoncouche.Controller {
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour {

        #region InspectorVariables

        [Header("Player Speed Properties :")]
        [SerializeField] [Tooltip("Acceleration of the player in unit per second")]
        private float playerAcceleration;
        [SerializeField] [Tooltip("Maximum velocity of the player")]
        private float maximumVelocity; 
        [SerializeField] [Tooltip("Curve of the velocity falloff when getting close to maximum speed")]
        private AnimationCurve VelocityFalloffCurve;
        [SerializeField] [Tooltip("Degrees of rotation the player can rotate per second.")]
        private float ROTATION_SPEED = 720f;


        [Header("Grab Related Properties :")]
        [SerializeField] [Tooltip("Ratio of the grabbed object's weight to take into account when slowing the player rotation when grabbing an object.")]
        private float GRAB_RATIO_ROTATION = 1f;
        [SerializeField] [Tooltip("Ratio of the grabbed object's weight to take into account when slowing the player movement speed when grabbing an object.")]
        private float GRAB_RATIO_MOVEMENT = 0.75f;

        [Header("Death : ")]
        [SerializeField] [Tooltip("Time in the spinning animation of respawn. Total death time is a combination of RESPAWN_SPIN_TIME and RESPAWN_UNDERWATER_TIME")]
        private float RESPAWN_SPIN_TIME = 2f;
        [SerializeField] [Tooltip("Speed at which the player spins when in respawn")]
        private float RESPAWN_SPIN_RATE = 270f;
        [SerializeField] [Tooltip("Time in the underwater animation of respawn. Total death time is a combination of RESPAWN_SPIN_TIME and RESPAWN_UNDERWATER_TIME")]
        private float RESPAWN_UNDERWATER_TIME = 3f;
        [SerializeField] [Tooltip("Z position under water when respawning. Can be changed independently for each character depending on their height.")]
        private float Z_UNDER_WATER = 1.25f;

        [Header("Other :")]
        [SerializeField] [Tooltip("Force to apply when bumping another player")]
        private float BUMP_FORCE = 1.5f;

        [SerializeField] [Tooltip("Is the current controller for player 1 or player 2")]
        private bool isPlayerOne = true;
        public bool IsPlayerOne { get { return isPlayerOne; } }

        #endregion

        #region PrivateVariables
        //Componentsgit 
        /// <summary>  Reference of player's rigidbody  </summary>
        private Rigidbody2D _playerRigidBody;
       /// <summary> Reference to the playerGrab which handles gravity body grabbing</summary>
        private PlayerGrab _playerGrab;
        /// <summary> Reference to the aim controller </summary>
        private AimController _aimController;
        /// <summary> Reference to the animator of the player </summary>
        private Animator _animator;
        /// <summary> Reference to the playerAudio of the player</summary>
        private PlayerAudio _playerAudio;
        /// <summary> Reference to the hook thrower attached to this object </summary>
        private HookThrower _hookThrower;
        /// <summary> Reference to the script that position the player on Z background </summary>
        private PositionZOnBackground _positionZOnBackground;

        //Inputs
        /// <summary>  Is the player moving horizontally? </summary>
        private bool _isMovingHorizontal;
        /// <summary>  Is the player moving vertical? </summary>
        private bool _isMovingVertical;
        /// <summary> Vector of player inputs </summary>
        private Vector2 _currentPlayerMovementInputs = Vector2.zero;
        /// <summary> Input of left analog at the horizontal  </summary>
        private float _leftAnalogHorizontal;
        /// <summary> Input of left analog at the vertical </summary>
        private float _leftAnalogVertical;

        //States 
        /// <summary> can the player bump right now </summary>
        private bool _canPlayerBump = true;
        /// <summary> is the player in respawn state </summary>
        public bool InRespawnState { get; private set; }

        //Private attibutes
        /// <summary> Start drag of the player's rigid body</summary>
        private float _startDrag;
        /// <summary> Start zDepth of PositionZOnBackground script</summary>
        private float _startZOffset;
        /// <summary> Cooldown for a player bump</summary>
        private float _playerBumpCooldown = 0.5f;
        /// <summary> Start Player weight </summary>
        public float _startPlayerWeight { get; private set; }

        private float _TimeSinceLastBump = 10f;
        private float _TimeSinceLastBumpNeededForChant = 1.25f;

        #endregion

        /// <summary> Getting multiple needed components (Rigidbody, ...)  </summary>
        void Awake() {
            _playerRigidBody = GetComponent<Rigidbody2D>();
            _aimController = GetComponent<AimController>();
            _hookThrower = GetComponentInChildren<HookThrower>();
            _animator = GetComponentInChildren<Animator>();
            _playerGrab = GetComponent<PlayerGrab>();
            _playerAudio = GetComponent<PlayerAudio>();
            _positionZOnBackground = GetComponent<PositionZOnBackground>();

            _startDrag = _playerRigidBody.drag;
            _startZOffset = _positionZOnBackground._zOffset;
            _startPlayerWeight = _playerRigidBody.mass;

            if (_playerGrab == null) {
                Debug.LogError("Player/PlayerGrab cannot be found!");
            }
            if(_animator == null) {
                Debug.LogError("Player/Animator cannot be found!");
            }
            if(_playerAudio == null) {
                Debug.LogError("Player/PlayerAudio cannont be found!");
            }
            if(_hookThrower == null) {
                Debug.LogError("Player/HookThrower cannont be found!");
            }
            if(_aimController == null) {
                Debug.LogError("Player/AimController cannont be found!");
            }
            if(_positionZOnBackground == null) {
                Debug.LogError("Player/PositionOnZBackground cannont be found!");
            }
        }

        /// <summary>  Initialization of variables </summary>
        void Start() {
            _playerRigidBody.interpolation = RigidbodyInterpolation2D.Interpolate; //Setting the interpolation of _playerRigidBody on to have more fluidity
            InRespawnState = false;

            //Setup input
            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Axis.p1_leftAnalog : InputManager.Axis.p2_leftAnalog,
                this.PlayerInputs
            );
            //No need to check for null since we do it in Awake
            _hookThrower.SetupInput(isPlayerOne);
            _playerGrab.SetupInput(isPlayerOne);
        }

        void Update() {
            _TimeSinceLastBump += Time.deltaTime;
        }

        /// <summary> FixedUpdate pour le character avec rigidbody (sujet à changements)  </summary>
        void FixedUpdate() {
            if (InRespawnState) return; //dont do logic if player is in respawn
            CharacterMovement();
            UpdateGrabDrag();
        }

        /// <summary> modifies the player's drag based on the grabbed object </summary>
        private void UpdateGrabDrag() {
            _playerRigidBody.drag = Mathf.Min(_playerGrab.GetGrabbedWeight() * GRAB_RATIO_MOVEMENT + _startDrag, 10f); //max drag to 3.5
            
        }

        //Death when entering maelstrom and respawn on map edges
        #region Death and Respawn
        public void OnMaelstromEnter(Vector3 deathPosition) {
            if(!InRespawnState) {
                StartRespawnState();
                StartCoroutine(Respawn_Spin(deathPosition));
                _hookThrower.RemoveChainOnPlayerMaelstromEnter();
                Chain.Chain[] chains = GameObject.FindObjectsOfType<Chain.Chain>();
                foreach(Chain.Chain chain in chains){
                    chain.SendMessage("OnMaelstromEnterOfPlayer", this);
                }
            }
        }

        private void StartRespawnState() {
            InRespawnState = true;
            GetComponent<CircleCollider2D>().enabled = false;
            _playerRigidBody.isKinematic = true;

            //Audio
            _playerAudio.PlaySound(PlayerSounds.PlayerDeath);
            if (_TimeSinceLastBump <= _TimeSinceLastBumpNeededForChant) _playerAudio.PlaySound(PlayerSounds.OtherPlayerChant);
            //Animation
            HandleKnockedOutStartAnimation();
        }

        private void StopRespawnState() {
            InRespawnState = false;
            GetComponent<CircleCollider2D>().enabled = true;
            _playerRigidBody.isKinematic = false;
            _playerRigidBody.velocity = Vector2.zero;
            StartCoroutine(Respawn_Resurface());

            //Audio
            _playerAudio.PlaySound(PlayerSounds.PlayerRespawn);
            //Animation
            HandleKnockedOutStopAnimation();
        }

        IEnumerator Respawn_Spin(Vector3 tP) {
            Vector2 sP = transform.position;
            float sZ = _positionZOnBackground._zOffset;
            float tZ = Z_UNDER_WATER;

            for(float i =0f; i < 1f; i += Time.deltaTime / RESPAWN_SPIN_TIME) {
                Vector2 lerpV2 = Vector2.Lerp(sP, (Vector2)tP, i);
                transform.position = new Vector3(lerpV2.x, lerpV2.y, transform.position.z);
                transform.Rotate(new Vector3(0, 0, RESPAWN_SPIN_RATE * Time.deltaTime));
                _positionZOnBackground._zOffset = Mathf.Lerp(sZ, tZ, i);
                yield return null;
            }
            StartCoroutine(Respawn_ReachEdgeUnderwater());
        }

        IEnumerator Respawn_ReachEdgeUnderwater() {
            Vector2 startPosition = (Vector2)transform.position;
            Vector2 targetPosition = Random.insideUnitCircle.normalized * 25f;

            for (float i = 0f; i < 1f; i += Time.deltaTime / RESPAWN_UNDERWATER_TIME) {
                Vector2 lerpV2 = Vector2.Lerp(startPosition, targetPosition, i);
                transform.position = new Vector3(lerpV2.x, lerpV2.y, transform.position.z);
                transform.Rotate(new Vector3(0, 0, RESPAWN_SPIN_RATE * Time.deltaTime));
                yield return null;
            }
            StopRespawnState();
        } 

        IEnumerator Respawn_Resurface() {
            float sZ = _positionZOnBackground._zOffset;
            for (float i = 0f; i < 1f; i += Time.deltaTime / 1f) {
                _positionZOnBackground._zOffset = Mathf.Lerp(sZ, _startZOffset, i);
                yield return null;
            }
            _positionZOnBackground._zOffset = _startZOffset;
        }

        #endregion

        //Player Movements
        #region movement
        /// <summary>
        /// Function that is called right after PlayerInputs inside Update in order to apply movement to our character
        /// </summary>
        private void CharacterMovement() {

            //Add Velocity based on speed
            VelocityCalculation();

            //Orientation modification
            RotateTowardTargetRotation();

        }

        /// <summary>
        /// Calculates the velocity to add based on player inputs and current Velocity.
        /// Intensify acceleration if going against current velocity and reduce acceleration if going towards current velocity.
        /// </summary>
        private void VelocityCalculation() {
            if (_isMovingHorizontal || _isMovingVertical) {
                Vector2 movementDirection = new Vector2(_leftAnalogHorizontal, _leftAnalogVertical); //direction of the analogs
                Vector2 addedAcceleration = movementDirection * playerAcceleration * Time.fixedDeltaTime; //Acceleration to add 
                Vector2 projection = (Vector2)(Vector3.Project(_playerRigidBody.velocity + addedAcceleration, movementDirection * maximumVelocity)); //projection of player's velocity on max movement

                //If we're trying to move in the direction of the player's velocity, reduce the movement by a factor of the current speed divided by max speed
                float speedMult = Mathf.Max(0f, (1f - (projection.magnitude / maximumVelocity)));
                speedMult = VelocityFalloffCurve.Evaluate(speedMult);
                //if we're trying to go backward, add speed.
                if (!(movementDirection.normalized == projection.normalized)) {
                    speedMult += 0.5f;
                }

                //Multiply acceleration by calculated mutliplier
                addedAcceleration = addedAcceleration * speedMult;
                //Debug.Log("mov :" + movementDirection * maximumVelocity + "      vel : " + _playerRigidBody.velocity + "    proj : " + projection + "     factor : " + speedMult + "     final acc : " + (addedAcceleration * speedMult));

                //Apply transformations
                _playerRigidBody.velocity += addedAcceleration;

                //Check for Idle animation
                if (movementDirection.x == 0 && movementDirection.y == 0) _animator.SetBool("Idle", true);
                else _animator.SetBool("Idle", false);
            }
        }


        /// <summary>
        /// Lerp the player's rotation toward its target rotation based on his rotation speed 
        /// </summary>
        private void RotateTowardTargetRotation() {
            //Check if idle
            if (_leftAnalogHorizontal == 0 && _leftAnalogVertical == 0) {
                _animator.SetBool("Idle", true);
                _animator.SetInteger("TurnAnim", 0);
            } else {
                _animator.SetBool("Idle", false);

                float target = Mathf.Atan2(_leftAnalogVertical, _leftAnalogHorizontal) * Mathf.Rad2Deg;
                float current = transform.eulerAngles.z;

                //Get Quaternion values
                Quaternion targetQ = Quaternion.Euler(0, 0, target);
                Quaternion currentQ = Quaternion.Euler(0, 0, current);
                Quaternion diffQ = targetQ * Quaternion.Inverse(currentQ);
                Quaternion diffQ2 = currentQ * Quaternion.Inverse(targetQ);

                //Calculate step of the lerp Based on angle to turn and turnspeed
                float rotationSpeed = ROTATION_SPEED / (1 + _playerGrab.GetGrabbedWeight() * GRAB_RATIO_ROTATION);
                float lerpStep = Mathf.Clamp(rotationSpeed / Mathf.Min(diffQ.eulerAngles.z, diffQ2.eulerAngles.z) * Time.deltaTime, 0f, 1f);
                //Lerp rotation
                transform.rotation = Quaternion.Lerp(currentQ, targetQ, lerpStep);

                //Handle Player Animation
                //Get angle difference between player orientation and player velocity
                float velocityAngle = Vector3.Angle(_playerRigidBody.velocity, Vector2.right);
                if (Vector3.Cross(_playerRigidBody.velocity, Vector2.right).z > 0) velocityAngle = 360 - velocityAngle;     
                float diffAngle = (transform.eulerAngles.z - velocityAngle);
                //Change swim anim state
                HandleRotateAnimation(Mathf.Abs(diffAngle), diffAngle < 0);
            }
        }

        /// <summary>  Returns the ratio of the maximum speed the player currently travels at </summary>
        /// <returns> Ratio of currentSpeed / MaxSpeed between 0 and 1 </returns>
        public float GetRatioOfMaxSpeed() {
            return (_playerRigidBody.velocity.magnitude / maximumVelocity);
        }

        #endregion

        //Methods called by various scripts to configure the variable of the player's animator
        #region animations handling

        /// <summary>
        /// Set the turn anim of the animator.
        /// if 0 : Swim
        /// if 1 : TurnLeftSmooth
        /// if 2 : TurnLeft90
        /// if 3 : TurnLeft180
        /// if 4 : TurnRightSmooth
        /// if 5 : TurnRight90
        /// if 6 : TurnRight180
        /// </summary>
        /// <param name="rotateRate">Difference between current angle and target angle</param>
        /// <param name="right">Are we turning right</param>
        private void HandleRotateAnimation(float rotateRate, bool right) {
            if (_animator == null) return;
            int animState = 0;
            if (right) {
                if (rotateRate > 120) animState = 6;
                else if (rotateRate > 60) animState = 5;
                else if (rotateRate > 15) animState = 4;
            } else {
                if (rotateRate > 120) animState = 3;
                else if (rotateRate > 60) animState = 2;
                else if (rotateRate > 15) animState = 1;
            }

            _animator.SetInteger("TurnAnim", animState);
        }

        /// <summary> Activate Push animation trigger</summary>
        public void HandlePushAnimation() {
            _animator.SetTrigger("Push");
            HandleGrabStopAnimation();
        }

        /// <summary> Handles the animations when player shoots first hook </summary>
        public void HandleFirstHookAnimation() {
            _animator.SetTrigger("Throw");
            _animator.SetBool("State_ChainGrab", true);
        }
        /// <summary> Handles the animations when player shoots second hook </summary>
        public void HandleSecondHookAnimation() {
            _animator.SetTrigger("Throw");
            _animator.SetBool("State_ChainGrab", false);
        }

        /// <summary> Handles the animations when player grabs an island </summary>
        public void HandleGrabStartAnimation() {
            _animator.SetBool("State_IslandGrab", true);
        }
        /// <summary> Handles the animations when player releases an island </summary>
        public void HandleGrabStopAnimation() {
            _animator.SetBool("State_IslandGrab", false);
        }

        /// <summary> Handles the animation when player gets hit (bumped) by other player </summary>
        public void HandleHitAnimation() {
            _animator.SetTrigger("GotBumped");
        }

        /// <summary> Handles the animation when the player starts aiming </summary>
        public void HandleAimStartAnimation() {
            _animator.SetBool("State_Aiming", true);
        }
        /// <summary> Handles the animation when the player stops aiming </summary>
        public void HandleAimStopAnimation() {
            _animator.SetBool("State_Aiming", false);
        }

        /// <summary> Handles the animation when the player starts being knocked out (entering maelstrom) </summary>
        public void HandleKnockedOutStartAnimation() {
            _animator.SetBool("State_KnockedOut", true);
        }
        /// <summary> Handles the animation when the player stops being knocked out (leaving maelstrom stun) </summary>
        public void HandleKnockedOutStopAnimation() {
            _animator.SetBool("State_KnockedOut", false);
        }

        #endregion

        //Collision Methods. Includes Unity collision methods
        #region Collision

        public void OnCollisionEnter2D(Collision2D col) {
            GravityBody otherGB = col.collider.gameObject.GetComponent<GravityBody>();
            if(otherGB != null) {
               
                if (otherGB.inDestroyMode) {  //Check for destroy mode
                    GetBumped(otherGB.Velocity / 1.5f);
                    otherGB.Velocity = Vector2.zero;
                } else {  //else Check for grab
                    CheckGrab(col, otherGB);
                }

                //Check for player bump
                CheckPlayerBump(col, otherGB);


            }
        }

        /// <summary> Check if conditions are met for the player to grab the collided gravity body </summary>
        /// <param name="col"> Collision2D </param>
        /// <param name="otherGB"> Gravity body collided with </param>
        private void CheckGrab(Collision2D col, GravityBody otherGB) {
            if (_playerGrab.gameObject.activeInHierarchy) {
                _playerGrab.AttemptGrab(otherGB);
            }
        }

        /// <summary> Checks if conditions for bump are met </summary>
        /// <param name="col"> Collision2D </param>
        /// <param name="otherGB"> Gravity body collided with </param>
        private void CheckPlayerBump(Collision2D col, GravityBody otherGB) {
            PlayerController otherPlayer = otherGB.GetComponent<PlayerController>();
            if (otherPlayer != null) {
                //If this is the player with the highest velocity, bump other
                if (_canPlayerBump && _playerRigidBody.velocity.magnitude > otherGB.Velocity.magnitude) {
                    //Bump other player, start other Bump cooldown and start Bump animation
                    otherPlayer.GetBumped(col.relativeVelocity * BUMP_FORCE);
                    //Decrease speed and start Bump cooldown
                    _playerRigidBody.velocity = _playerRigidBody.velocity - col.relativeVelocity;
                    StartPlayerBumpCooldown();                   
                }
            }
        }

        /// <summary> Bumps the player with given force</summary>
        /// <param name="bumpForce">Vector2 representing the force of the push to be made</param>
        public void GetBumped(Vector2 bumpForce) {
            _playerRigidBody.velocity += bumpForce;
            StartPlayerBumpCooldown();
            HandleHitAnimation();
            _TimeSinceLastBump = 0f; // reset time since last bump
            //Release object
            _playerGrab.Release();
            //Play Audio
            _playerAudio.PlaySound(PlayerSounds.PlayerBump);
        }

        /// <summary> Starts the player bump cooldown </summary>
        private void StartPlayerBumpCooldown() {
            _canPlayerBump = false;
            StartCoroutine(PlayerBumpCooldown());
        }
        /// <summary> Coroutine that manages the bump cooldown </summary>
        IEnumerator PlayerBumpCooldown() {
            yield return new WaitForSeconds(_playerBumpCooldown);
            _canPlayerBump = true;

        }

        #endregion

        //Methods linked to player inputs
        #region PlayerInputs

        /// <summary> Function called in Update to register player inputs </summary>
        private void PlayerInputs(params float[] input) {
            _leftAnalogHorizontal = input[0];
            _leftAnalogVertical = input[1];

            if (Mathf.Abs(_leftAnalogHorizontal) > 0.0f)  _isMovingHorizontal = true;
            else _isMovingHorizontal = false;

            if (Mathf.Abs(_leftAnalogVertical) > 0.0f) _isMovingVertical = true;
            else _isMovingVertical = false;

            _currentPlayerMovementInputs.x = _leftAnalogHorizontal;
            _currentPlayerMovementInputs.y = _leftAnalogVertical;
        }



        public Vector2 GetCurrentPlayerMovementInputs() {
            return _currentPlayerMovementInputs;
        }

        /// <summary>  Fonction which returns the right analog horizontal input </summary>
        public float GetLeftAnalogHorizontal() {
            return _leftAnalogHorizontal;
        }

        /// <summary> Fonction which returns the left analog vertical input </summary>
        public float GetLeftAnalogVertical() {
            return _leftAnalogVertical;
        }
        #endregion

    }
}
