using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Simoncouche.Chain;

namespace Simoncouche.Controller {
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour {

	    #region InspectorVariables
        [Header("Player Speed Properties")]

        [SerializeField] [Tooltip("Acceleration of the player in unit per second")]
	    private float playerAcceleration;

        [SerializeField] [Tooltip("Maximum velocity of the player")]
	    private float maximumVelocity;
    
        [SerializeField] [Tooltip("Curve of the velocity falloff when getting close to maximum speed")]
        private AnimationCurve VelocityFalloffCurve;

        [SerializeField] [Tooltip("Degrees of rotation the player can rotate per second.")]
        private float ROTATION_SPEED = 720f;

        [SerializeField] [Tooltip("Is the current controller for player 1 or player 2")]
        private bool isPlayerOne = true;

        [SerializeField] [Tooltip("Ratio of the grabbed object's weight to take into account when slowing the player rotation when grabbing an object.")]
        private float GRAB_RATIO_ROTATION = 1f;

        [SerializeField] [Tooltip("Ratio of the grabbed object's weight to take into account when slowing the player movement speed when grabbing an object.")]
        private float GRAB_RATIO_MOVEMENT = 0.75f;

        #endregion

        #region PrivateVariables
        //Componentsgit 
        /// <summary>  Reference of player's rigidbody  </summary>
        private Rigidbody2D _playerRigidBody;

        /// <summary> Reference to the playerGrab which handles gravity body grabbing</summary>
        private PlayerGrab _playerGrab;

        /// <summary> Reference to the aim controller </summary>
        private AimController _aimController;

        private Animator _animator;

        //Inputs
        /// <summary>  Is the player moving horizontally? </summary>
        private bool _isMovingHorizontal;

        /// <summary>  Is the player moving vertical? </summary>
        private bool _isMovingVertical;

        /// <summary> Start drag of the player's rigid body</summary>
        private float _startDrag;



	    /// <summary>
	    /// Vector of player inputs
	    /// </summary>
	    private Vector2 _currentPlayerMovementInputs=Vector2.zero;

        /// <summary> Reference to the hook thrower attached to this object </summary>
        private HookThrower _hookThrower;

        /// <summary> Input of left analog at the horizontal  </summary>
        private float _leftAnalogHorizontal;

        /// <summary> Input of left analog at the vertical </summary>
        private float _leftAnalogVertical;

        #endregion

        /// <summary>
        /// Getting multiple needed components (Rigidbody, ...)
        /// </summary>
        void Awake() {
            _playerRigidBody = GetComponent<Rigidbody2D>();
            _aimController = GetComponent<AimController>();
            _hookThrower = GetComponentInChildren<HookThrower>();
            _animator = GetComponentInChildren<Animator>();
            _playerGrab = GetComponent<PlayerGrab>();
            _startDrag = _playerRigidBody.drag;

            if (_playerGrab == null) {
                Debug.LogError("Player/PlayerGrab cannot be found!");
            }
            if(_animator == null) {
                Debug.LogError("Player/Animator cannot be found!");
            }

        }

        /// <summary>
        /// Initialization of variables
        /// </summary>
        void Start() {
            _playerRigidBody.interpolation = RigidbodyInterpolation2D.Interpolate; //Setting the interpolation of _playerRigidBody on to have more fluidity

            //Setup input
            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Axis.p1_leftAnalog : InputManager.Axis.p2_leftAnalog,
                this.PlayerInputs
            );

            if (_hookThrower != null) {
                _hookThrower.SetupInput(isPlayerOne);
            } else {
                Debug.LogError("Their is no hook thrower attached or child of this object");
            }

            if (_aimController != null) {
                _aimController.SetupInput(isPlayerOne);
            }

            if (_playerGrab != null) {
                _playerGrab.SetupInput(isPlayerOne);
            }
	    }

        /// <summary>
        /// FixedUpdate pour le character avec rigidbody (sujet à changements)
        /// </summary>
        void FixedUpdate() {
            CharacterMovement();
            UpdateGrabDrag();
        }

        /// <summary>
        /// modifies the player's drag based on the grabbed object
        /// </summary>
        private void UpdateGrabDrag() {
            _playerRigidBody.drag = _playerGrab.GetGrabbedWeight() * GRAB_RATIO_MOVEMENT + _startDrag;

        }

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

        #endregion

        #region animations handling

        /// <summary>
        /// Set the turn anim of the animator.
        /// if 0 : Swim
        /// if 1 : TurnLeft90
        /// if 2 : TurnLeft180
        /// if 3 : TurnRight90
        /// if 4 : TurnRight180
        /// </summary>
        /// <param name="rotateRate">Difference between current angle and target angle</param>
        /// <param name="right">Are we turning right</param>
        private void HandleRotateAnimation(float rotateRate, bool right) {
            if (_animator == null) return;
            int animState = 0;
            if (right) {
                if (rotateRate > 90) animState = 4;
                else if (rotateRate > 45) animState = 3;
            } else {
                if (rotateRate > 90) animState = 2;
                else if (rotateRate > 45) animState = 1;
            }

            _animator.SetInteger("TurnAnim", animState);
        }

        /// <summary> Activate Push animation trigger</summary>
        public void HandlePushAnimation() {
            _animator.SetTrigger("Push");
        }

        /// <summary> Activate Throw animation trigger</summary>
        public void HandleThrowAnimation() {
            _animator.SetTrigger("Throw");
        }

        #endregion

        #region Collision

        public void OnCollisionEnter2D(Collision2D col) {
            GravityBody otherGB = col.collider.gameObject.GetComponent<GravityBody>();
            if(otherGB != null) {
                if (_playerGrab.gameObject.activeInHierarchy) _playerGrab.AttemptGrab(otherGB);
            }
        }

        #endregion


        #region PlayerInputs

        /// <summary>
        /// Function called in Update to register player inputs
        /// </summary>
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

        /// <summary>
        /// Fonction which returns the right analog horizontal input
        /// </summary>
        /// <returns></returns>
        public float GetLeftAnalogHorizontal() {
            return _leftAnalogHorizontal;
        }

        /// <summary>
        /// Fonction which returns the left analog vertical input
        /// </summary>
        /// <returns></returns>
        public float GetLeftAnalogVertical() {
            return _leftAnalogVertical;
        }
        #endregion

    }
}
