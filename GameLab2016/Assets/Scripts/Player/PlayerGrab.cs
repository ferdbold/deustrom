using UnityEngine;
using System.Collections;
using Simoncouche.Islands;

namespace Simoncouche.Controller {
    public class PlayerGrab : MonoBehaviour {

        //Attributes
        [Tooltip("Magnitude of the force applied to the thrown gravity body")][SerializeField]
        private float THROW_FORCE = 10f;

       /// <summary> Parent of grabbed gravity body. Used to reposition  body at the right place when releasing. </summary>
        private Transform _grabbedBodyParent = null;


        
        //Components
        /// <summary> Currently Grabbed GravityBody</summary>
        private GravityBody _grabbedBody = null;

        /// <summary> Reference to the aim controller </summary>
        private AimController _aimController;

        /// <summary> Reference to the Player Controller</summary>
        private PlayerController _controller;

        void Awake() {
            _controller = GetComponent<PlayerController>();
            _aimController = GetComponent<AimController>();


            if (_aimController == null) {
                Debug.LogError("Player/AimController cannot be found!");
            }
        }

        void Update() {
            if (_grabbedBody != null && Input.GetKeyDown(KeyCode.F)) {
                Throw();
            }
        }

        /// <summary> Attemps to grab gravity body if one is not already grabbed</summary>
        /// <param name="targetBody">target gravity body to grab</param>
        public void AttemptGrab(GravityBody targetBody) {
            if(_grabbedBody == null) {
                IslandChunk targetChunk = targetBody.gameObject.GetComponent<IslandChunk>();
                //Register body as grabbed
                

                //Modify grabbed body / chunk
                
                if(targetChunk.parentIsland == null) {
                    _grabbedBody = targetBody;
                    _grabbedBodyParent = targetChunk.transform.parent;
                    targetChunk.transform.parent = transform;
                    targetBody.DeactivateGravityBody();
                    Physics2D.IgnoreCollision(GetComponent<Collider2D>(), targetChunk.GetComponent<Collider2D>(), true);
                } else {
                    //TODO : 
                }
                



            }
        }
        
        /// <summary> Throw gravity body in direction of player's aim controller</summary>
        private void Throw() {
            if(_grabbedBody != null) {
                //Get Body to add force to
                GravityBody bodyToAddForce = _grabbedBody;
                IslandChunk targetChunk = _grabbedBody.gameObject.GetComponent<IslandChunk>();
                if (targetChunk.parentIsland != null) {
                    bodyToAddForce = targetChunk.parentIsland.gravityBody;
                }
                //Release
                Release();
                //Add Force
                Vector2 forceDirection = _aimController.GetDirectionToAimObject(transform).normalized;
                bodyToAddForce.Velocity += forceDirection * THROW_FORCE;


            } else {
            Debug.Log("Attempted to throw when _grabbedBody is null.");
            }
        }

        /// <summary> Releases the gravity body </summary>
        private void Release() {
            
            IslandChunk targetChunk = _grabbedBody.gameObject.GetComponent<IslandChunk>();

            if (targetChunk.parentIsland == null) {
                targetChunk.transform.parent = _grabbedBodyParent;
                _grabbedBodyParent = null;
                _grabbedBody.ActivateGravityBody();  
            } else {
                //TODO:
                targetChunk.transform.parent = _grabbedBodyParent;
                _grabbedBodyParent = null;
            }

            //Put collisiosn back on and unmark as grabbed
            StartCoroutine(RemoveCollision(targetChunk.GetComponent<Collider2D>(), 1f));
            _grabbedBody = null;
        }

        IEnumerator RemoveCollision(Collider2D otherCol, float time) {
            yield return new WaitForSeconds(time);
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), otherCol, false);
        }

    }
}
