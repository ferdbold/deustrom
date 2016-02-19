using UnityEngine;
using System.Collections;
using Simoncouche.Islands;

namespace Simoncouche.Controller {
    public class PlayerGrab : MonoBehaviour {

        //Attributes
        [Tooltip("Magnitude of the force applied to the thrown gravity body")][SerializeField]
        private float THROW_FORCE = 15f;

        /// <summary> Parent of grabbed gravity body. Used to reposition  body at the right place when releasing. </summary>
        private Transform _grabbedBodyParent = null;



        //Components
        /// <summary> Currently Grabbed GravityBody</summary>
        public GravityBody grabbedBody { get; private set;}

        /// <summary> Reference to the aim controller </summary>
        private AimController _aimController;

        /// <summary> Reference to the Player Controller</summary>
        private PlayerController _controller;

        void Awake() {
            _controller = GetComponent<PlayerController>();
            _aimController = GetComponent<AimController>();
            grabbedBody = null;

            if (_aimController == null) {
                Debug.LogError("Player/AimController cannot be found!");
            }
        }

        public void SetupInput(bool isPlayerOne) {
            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_pushButton : InputManager.Button.p2_pushButton,
                this.Throw
            );
        }

        void Update() {
            if (grabbedBody != null && Input.GetKeyDown(KeyCode.F)) {
                Throw();
            }
        }

        /// <summary> Attemps to grab gravity body if one is not already grabbed</summary>
        /// <param name="targetBody">target gravity body to grab</param>
        public void AttemptGrab(GravityBody targetBody) {
            if(grabbedBody == null) {
                IslandChunk targetChunk = targetBody.gameObject.GetComponent<IslandChunk>();

                if (targetChunk != null) { //Make sure we are trying to grab an IslandChunk

                    if (targetChunk.parentIsland == null) {
                        grabbedBody = targetBody;
                        //Set parent
                        _grabbedBodyParent = targetChunk.transform.parent;
                        targetChunk.transform.parent = transform;
                        //Ignore Collision
                        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), targetChunk.GetComponent<Collider2D>(), true);
                        //Deactivate gravity body
                        grabbedBody.DeactivateGravityBody();
                    } else {
                        Island parentIsland = targetChunk.parentIsland;
                        grabbedBody = targetBody;
                        //Set parent
                        _grabbedBodyParent = parentIsland.transform.parent;
                        parentIsland.transform.parent = transform;
                        //Ignore Collision
                        foreach (IslandChunk iChunk in parentIsland.GetComponentsInChildren<IslandChunk>()) {
                            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), iChunk.GetComponent<Collider2D>(), true);
                        }
                        //Deactivate gravity body
                        parentIsland.GetComponent<GravityBody>().DeactivateGravityBody();
                    }

                }
                
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
                bodyToAddForce.Velocity += forceDirection * THROW_FORCE;


            } else {
                Debug.LogWarning("Attempted to throw when grabbedBody is null.");
            }
        }

        /// <summary> Releases the gravity body </summary>
        public void Release() {
            
            IslandChunk targetChunk = grabbedBody.gameObject.GetComponent<IslandChunk>();

            //If chunk has no parent island
            if (targetChunk.parentIsland == null) {
                //Unparent chunk & activate GravBody
                targetChunk.transform.parent = _grabbedBodyParent;
                _grabbedBodyParent = null;
                grabbedBody.ActivateGravityBody();
                //UnIgnore Collision
                StartCoroutine(RemoveCollision(targetChunk.GetComponent<Collider2D>(), 1f));
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
                    StartCoroutine(RemoveCollision(iChunk.GetComponent<Collider2D>(), 1f));
                }
            }

            //Mark grabbed body as null
            grabbedBody = null;
        }

        /// <summary> Remove collision with given collider2D after a amount of time </summary>
        /// <param name="otherCol">collider to unIgnore</param>
        /// <param name="time">time before unignore</param>
        /// <returns></returns>
        IEnumerator RemoveCollision(Collider2D otherCol, float time) {
            yield return new WaitForSeconds(time);
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), otherCol, false);
        }

    }
}
