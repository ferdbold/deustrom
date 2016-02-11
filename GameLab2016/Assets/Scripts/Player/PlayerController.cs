﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {

	#region PublicVariables
	[Header("Player Speed Properties")]


	/// <summary> Variable to adjust the jetpack speed </summary>
    [Tooltip("Acceleration of the player in unit per second")]
	public float playerAcceleration;

	/// <summary> Maximum velocity of the player </summary>
    [Tooltip("Maximum velocity of the player")]
	public float maximumVelocity;
	#endregion


	#region PrivateVariables
	/// <summary> Reference of player's rigidbody  </summary>
	private Rigidbody2D _playerRigidBody;

	/// <summary> Reference of the player's sprite renderer in order to lerp on the player flip  </summary>
	private SpriteRenderer _playerSpriteRenderer;

	/// <summary>  Is the player moving horizontally? </summary>
	private bool _isMovingHorizontal;

	/// <summary>  Is the player moving vertical? </summary>
	private bool _isMovingVertical;

	/// <summary>  Is the player looking at his right  </summary>
	private bool _isLookingRight=true;

	/// <summary> Is it flipping the sprite  </summary>
	private bool _isFlippingSprite = false;

	/// <summary> Input of left analog at the horizontal  </summary>
	private float _leftAnalogHorizontal;

	/// <summary> Input of left analog at the vertical </summary>
	private float _leftAnalogVertical;


	private Vector2 _currentAddedVelocity=Vector2.zero;
    #endregion

    public bool DEBUG_USING_CONTROLS_1 = false;

	/// <summary>
	/// Getting multiple needed components (Rigidbody, ...)
	/// </summary>
	void Awake() {
		_playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		_playerRigidBody = GetComponent<Rigidbody2D>();
	}


	/// <summary>
	/// Initialization of variables
	/// </summary>
	void Start() {
		_playerRigidBody.interpolation = RigidbodyInterpolation2D.Interpolate; //Setting the interpolation of _playerRigidBody on to have more fluidity
		GameManager.inputManager.AddEvent(InputManager.Axis.leftAnalog, PlayerInputs); //Setup input
	}
	/// <summary>
	/// FixedUpdate pour le character avec rigidbody (sujet à changements)
	/// </summary>
	void FixedUpdate() {
		CharacterMovement();
	}


	/// <summary>
	/// Function to flip the player's sprite if going toward the opposite direction
	/// </summary>
	private void UpdateSpriteFlip() {
		if (_isMovingHorizontal) {
			if (_leftAnalogHorizontal < 0) {
				StartCoroutine(SmoothSpriteFlip(new Vector3(180.0f,
					0.0f), //Because of Orientation modification in Orientation modification
					1.5f));
			}else {
                StartCoroutine(SmoothSpriteFlip(new Vector3(0.0f
					, 0.0f
					, 0.0f)
					, 1.5f));
			}
		}
	}


	/// <summary>
	/// Coroutine doing a lerp on the rotation of a sprite in order to flip his it !!! Not actually used (buggy)  !!!
	/// </summary>
	/// <param name="endRot"></param>
	/// <param name="time"></param>
	/// <returns></returns>
	private IEnumerator SmoothSpriteFlip(Vector3 endRot, float time) {
		float elapsedTime =  0.0f;
        Vector3 initialTransform = _playerSpriteRenderer.transform.rotation.eulerAngles;
		while (elapsedTime < time) {
            Vector3 tempVector = Vector3.Lerp(initialTransform, endRot, elapsedTime / time);
			_playerSpriteRenderer.transform.rotation = Quaternion.Euler(tempVector);
			elapsedTime += Time.deltaTime;
			yield return new WaitForFixedUpdate();	//Wait For Fixed update because the function is the coroutine is called in charaterMovement which is in fixed update
		}
		_isFlippingSprite = false;
	}




	/// <summary>
	/// Function called in Update to register player inputs
	/// </summary>
	private void PlayerInputs(params float[] input) {
		_leftAnalogHorizontal = input[0];
		_leftAnalogVertical = input[1];

		if (Mathf.Abs(_leftAnalogHorizontal) > 0.0f) {
			_isMovingHorizontal = true;
		}else{
			_isMovingHorizontal = false;
		}

		if (Mathf.Abs(_leftAnalogVertical) > 0.0f) _isMovingVertical = true;
		else _isMovingVertical = false;
	}



	/// <summary>
	/// Function that is called right after PlayerInputs inside Update in order to apply movement to our character
	/// </summary>
	private void CharacterMovement() {
		Vector2 tempVelocity = _playerRigidBody.velocity;
		Vector2 tempAcceleration = new Vector2(0.0f, 0.0f);


		

		//Sprite Flip Condition
		if (_isMovingHorizontal) {
			Vector2 tempScale = _playerSpriteRenderer.transform.localScale;
			
			if (_isLookingRight && _leftAnalogHorizontal < 0.0f) {
				_playerSpriteRenderer.flipY = true;
				_isLookingRight = false;
			}else if (!_isLookingRight && _leftAnalogHorizontal > 0.0f) {
				_playerSpriteRenderer.flipY = false;
				_isLookingRight = true;
			}
		}

        if (DEBUG_USING_CONTROLS_1) {
            //Velocity modification 1
            //Get Horizontal and vertical analog values
            if (_isMovingHorizontal) {
                tempAcceleration.x += _leftAnalogHorizontal;
            }
            if (_isMovingVertical) {
                tempAcceleration.y += _leftAnalogVertical;
            }
            //Modify velocity
            if (_isMovingHorizontal || _isMovingVertical) {
                tempAcceleration *= playerAcceleration * Time.fixedDeltaTime;
                tempVelocity = Vector2.ClampMagnitude(tempVelocity + tempAcceleration, maximumVelocity);
                _currentAddedVelocity = tempVelocity - _playerRigidBody.velocity;
                _playerRigidBody.velocity = tempVelocity;
            }
        } else {

            //Velocity modification 2
            if (_isMovingHorizontal || _isMovingVertical) {
                Vector2 movementDirection = new Vector2(_leftAnalogHorizontal, _leftAnalogVertical); //direction of the analogs
                Vector2 addedAcceleration = movementDirection * playerAcceleration * Time.fixedDeltaTime; //Acceleration to add 
                Vector2 projection = (Vector2)(Vector3.Project(_playerRigidBody.velocity + addedAcceleration, movementDirection*maximumVelocity)); //projection of player's velocity on max movement

                //If we're trying to move in the direction of the player's velocity, reduce the movement by a factor of the current speed divided by max speed
                if (movementDirection.normalized == projection.normalized) {
                    float speedMult = Mathf.Max(0f,(1f - (projection.magnitude / maximumVelocity)));
                    //Debug.Log("mov :" + movementDirection * maximumVelocity + "      vel : " + _playerRigidBody.velocity + "    proj : " + projection + "     factor : " + speedMult);
                    movementDirection = movementDirection.normalized * speedMult;
                }
                //Apply transformations
                _playerRigidBody.velocity += addedAcceleration;
            }
        }
        

        //Orientation modification
        if (_isMovingHorizontal || _isMovingVertical){
            float angle = Mathf.Atan((_leftAnalogVertical / (_leftAnalogHorizontal != 0.0f ? _leftAnalogHorizontal : 0.000001f))) * Mathf.Rad2Deg; //Ternary condition due to a possibility of divide by 0
            Vector3 tempRotation = transform.rotation.eulerAngles;
            tempRotation.z = angle;
            if (_leftAnalogHorizontal < 0.0f){
                tempRotation.z -= 180.0f;
            }
            transform.eulerAngles = tempRotation; //We apply the rotation
        }




	}


	public Vector2 GetCurrentAddedVelocity() {
		return _currentAddedVelocity;
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
}
