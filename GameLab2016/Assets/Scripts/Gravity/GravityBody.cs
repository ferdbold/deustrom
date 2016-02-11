using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object that is affected by all gravity modifier
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GravityBody : GravityObject {

    //Properties
    [Tooltip("Default linear drag of this rigidbody")][SerializeField] private float DEFAULT_DRAG = 0.35f;
    [Tooltip("Velocity of this gravity body when loading scene.")][SerializeField] private Vector2 START_VELOCITY = Vector2.zero;
    private float _additionnalDrag = 0f; //Additionnal drag added by attractor

    //Getters
    public float Weight { get { return _rigidBody.mass; }  set { _rigidBody.mass = value; } }
    public Vector2 Velocity { get { return _rigidBody.velocity; } set { _rigidBody.velocity = value; } }
    public float LinearDrag { get { return _rigidBody.drag; } set { _rigidBody.drag = value; } }
    public float AngularDrag { get { return _rigidBody.angularDrag; } set { _rigidBody.angularDrag = value; } }
    
    public float AdditionnalDrag { get { return _additionnalDrag; } set { _additionnalDrag = Mathf.Clamp(value, 0f, 2f); } }
    public float DefaultDrag { get { return DEFAULT_DRAG;  } private set { DEFAULT_DRAG = value; } }

    //Components
    private Rigidbody2D _rigidBody;
    //Collision
    private int gravityModifierLayerMask;  //Layermask of gravity modifier
    private int playerLayerMask; //Layermask of player
    /// <summary> Time remaining without collisions activated </summary>
    private float timeNoCollision = 0f;
    /// <summary> Are collisions currently enabled </summary>
    public bool collisionEnabled { get; private set; }

    //Activation
    [Header("Debug : ")]
    [Tooltip("DO NOT TOUCH. USED FOR DEBUG.")] [SerializeField]  private bool _activated = true; //Serialized for debug purposes
    [Tooltip("DO NOT TOUCH. USED FOR DEBUG.")] public List<GravityModifier> currentGravObjects = new List<GravityModifier>();

    override protected void Awake(){
        base.Awake();

        gravityModifierLayerMask = 8; //Get gravity modifier layermask
        playerLayerMask = 9; //Get player layermask

        if (gameObject.layer != playerLayerMask) { //if player
            gameObject.layer = 10;
        }
        collisionEnabled = true;

        SetupRigidbody();
        SetupCollider();
    }

    override protected void Start () {
        base.Start();

    }

    void Update() {
        if (!_activated) return;

        CheckCollisionEnabled();
    }

    void FixedUpdate() {
        if (!_activated) return;

        Vector2 acceleration = Vector2.zero;
        foreach (GravityModifier gravObj in currentGravObjects) {
            Vector2 accelChange = gravObj.ApplyGravityForce(this);
            //Debug.Log("force : " + accelChange + "       by gravObj " + gravObj.name);
            acceleration += accelChange;          
        }
        _rigidBody.velocity += acceleration * Time.fixedDeltaTime;
    }

    /// <summary> Deactivates GravityBody (used when body creates or join an island)  </summary>
    public void DeactivateGravityBody() {
        _activated = false;
        _rigidBody.isKinematic = true;
        _collider.enabled = false;
    }

    /// <summary> Activates GravityBody </summary>
    public void ActivateGravityBody() {
        _activated = true;
        _rigidBody.isKinematic = false;
        _collider.enabled = true;
    }

    /// <summary>
    /// Destroys the gravity body (should override this to score points when destroying)
    /// </summary>
    public virtual void DestroyGravityBody(){
        Debug.Log("Destroyed RigidBody " + gameObject.name);
        Destroy(gameObject);
    }

    #region Components Initialization

    void SetupRigidbody() {
        //Get Component References
        _rigidBody = GetComponent<Rigidbody2D>();
        if (_rigidBody == null) _rigidBody = gameObject.AddComponent<Rigidbody2D>();
        //Set Collider values
        _rigidBody.gravityScale = 0;
        //Set Starting Velocity
        _rigidBody.velocity = START_VELOCITY;
        //Set starting Drag
        _rigidBody.drag = DEFAULT_DRAG;
    }

    /// <summary>
    /// Setup the _collider
    /// </summary>
    protected virtual void SetupCollider() {
        //Get Component References
        _collider = GetComponent<CircleCollider2D>();
        if (_collider == null) _collider = gameObject.AddComponent<CircleCollider2D>();
        //Set _collider values
        _collider.radius = radius;
        //Set trigger to false if we're not player. 
        if (gameObject.layer != playerLayerMask) _collider.isTrigger = false;
        else _collider.isTrigger = true;
    }
    #endregion

    #region MonoBehaviour Collision Methods
    /// <summary>
    /// Manages the objects that are within its radius by adding them to the currentGrabOjects list when they collide
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == gravityModifierLayerMask && other.gameObject != gameObject) {
            GravityModifier gravityObjScript = other.gameObject.GetComponent<GravityModifier>();
            if (gravityObjScript != null) {
                if(!currentGravObjects.Contains(gravityObjScript)) currentGravObjects.Add(gravityObjScript);
            }
        }
    }

    /// <summary>
    /// Manages the objects that are within its radius by removing them of the currentGrabOjects list when they leave
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.layer == gravityModifierLayerMask) {
            GravityModifier gravityObjScript = other.gameObject.GetComponent<GravityModifier>();
            if (gravityObjScript != null) {
                if(currentGravObjects.Contains(gravityObjScript)) currentGravObjects.Remove(gravityObjScript);
            }
        }
    }

    #endregion

    #region Advanced Collision Functions

    /// <summary>
    /// Resets the list of current gravity modifier. Used in various cases (ex: When teleporting through wormhole)
    /// </summary>
    public void ResetGravityObjects() {
        currentGravObjects = new List<GravityModifier>();
    }


    /// <summary>
    /// Checks if collision needs to be reactivated
    /// </summary>
    private void CheckCollisionEnabled() {
        if(!collisionEnabled) {
            timeNoCollision -= Time.deltaTime;
            if(timeNoCollision <= 0f) {
                collisionEnabled = true;
                timeNoCollision = 0f;
                _collider.enabled = true;
            }
        }
    }

    /// <summary>
    /// Disable the collision for a small amount of time (still affected by gravity)
    /// </summary>
    /// <param name="time">time in seconds </param>
    public void DisableCollision(float time) {
        if (timeNoCollision < time) timeNoCollision = time;
        collisionEnabled = false;
         _collider.enabled = false;
    }



    #endregion
}
