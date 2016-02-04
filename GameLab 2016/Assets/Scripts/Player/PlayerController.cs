﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {

	#region PublicVariables
	[Header("Player speed properties")]


	/// <summary>
	/// Variable to adjust the jetpack speed
	/// </summary>
    ///
    [Tooltip("Acceleration of the player in unit per second")]
	public float playerAcceleration;


	/// <summary>
	/// Maximum velocity of the player
	/// </summary>
    /// 
    [Tooltip("Maximum velocity of the player")]
	public float maximumVelocity;


	[Header("Player on orientation switch properties")]


	/// <summary>
	/// Angle between the last movement force used and the current movement force.  If higher thant this variable, we apply a boost.
	/// </summary>
	[Tooltip("Angle between the last movement force used and the current movement force.  If higher thant this variable, we apply a boost.")]
	public float boostAngleTreshold;


	/// <summary>
	/// This is value is a multiplier of the acceleration when we are in a boost situation
	/// </summary>
	[Tooltip("Acceleration multipler when the rotation boost happens")]
	public float boostMultipler;


	/// <summary>
	/// Angle between the last movement force used and the current movement force.  If higher thant this variable, we apply a boost.
	/// </summary>
	[Tooltip("Angle between the last movement force used and the current movement force.  If higher thant this variable, we apply a boost.")]
	public float boostTimeOnRotate;

	#endregion


	#region PrivateVariables
	

	/// <summary>
	/// Reference of player's rigidbody
	/// </summary>
	private Rigidbody2D _playerRigidBody;


	/// <summary>
	/// Reference of the player's sprite renderer in order to lerp on the player flip
	/// </summary>
	private SpriteRenderer _playerSpriteRenderer;


	/// <summary>
	/// Is the player moving horizontally?
	/// </summary>
	private bool _isMovingHorizontal;


	/// <summary>
	/// Is the player moving vertical?
	/// </summary>
	private bool _isMovingVertical;


	/// <summary>
	/// Is the player looking at his right
	/// </summary>
	private bool _isLookingRight=true;


	/// <summary>
	/// Is it flipping the sprite
	/// </summary>
	private bool _isFlippingSprite = false;


	/// <summary>
	/// Input of left analog at the horizontal
	/// </summary>
	private float _leftAnalogHorizontal;


	/// <summary>
	/// Input of left analog at the vertical
	/// </summary>
	private float _leftAnalogVertical;


	[Header("Private variables")]


	/// <summary>
	/// Last velocity change applied to our rigidbody
	/// </summary>
	[SerializeField]
	private Vector3 _lastAccelerationApplied;


	/// <summary>
	/// This value is set true if we change drastically our player's movement orientation
	/// </summary>
	[SerializeField]
	private bool _flipBoost;
	#endregion



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
					0.0f,
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


	private void TemporaryBoost() {
		_flipBoost = true;
		StartCoroutine(BoostTimer(boostTimeOnRotate));
	}

	private IEnumerator BoostTimer(float waitTime) {
		yield return new WaitForSeconds(waitTime);
		_flipBoost = false;
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
		
		Vector3 tempVelocity = _playerRigidBody.velocity;
		Vector3 tempAcceleration = new Vector3(0.0f, 0.0f, 0.0f);
		if (_isMovingHorizontal) {
			tempAcceleration.x += _leftAnalogHorizontal;
		}

		if (_isMovingVertical) {
			tempAcceleration.y += _leftAnalogVertical;
		}
		

		//Sprite Flip Condition
		if (_isMovingHorizontal) {
			Vector3 tempScale = _playerSpriteRenderer.transform.localScale;
			
			if (_isLookingRight && _leftAnalogHorizontal < 0.0f) {
				_playerSpriteRenderer.flipY = true;
				_isLookingRight = false;
			}else if (!_isLookingRight && _leftAnalogHorizontal > 0.0f) {
				_playerSpriteRenderer.flipY = false;
				_isLookingRight = true;
			}
		}

		//Velocity modification
		tempAcceleration *= playerAcceleration * Time.fixedDeltaTime;
		if ((Mathf.Abs(Vector3.Angle(tempAcceleration.normalized, _lastAccelerationApplied.normalized))) > 0.0f+ boostAngleTreshold   
			&& (Mathf.Abs(Vector3.Angle(tempAcceleration.normalized, _lastAccelerationApplied.normalized))) < 360.0f- boostAngleTreshold) 
		{
			TemporaryBoost();
		}
		if (_flipBoost) {
			tempAcceleration *= boostMultipler; 
		}
		tempVelocity = Vector3.ClampMagnitude(tempVelocity+ tempAcceleration, maximumVelocity);
        _playerRigidBody.velocity = tempVelocity;
		if (_isMovingHorizontal || _isMovingVertical) {
			_lastAccelerationApplied = tempAcceleration.normalized;
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
