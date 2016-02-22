using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

        /// <summary> List of references to all playerGrabs. Used to avoid Searching the map everytime we grab.</summary>
        private static List<PlayerGrab> _allPlayerGrabs = new List<PlayerGrab>();

        /// <summary> List of currently running coroutines. Used to keep track of running coroutine that re-enable collision in case we need to stop them. </summary>
        private Dictionary<string, Coroutine> collisionCoroutines = new Dictionary<string, Coroutine>();


        void Awake() {
            _aimController = GetComponent<AimController>();
            grabbedBody = null;

            //Add to static playergrab list
            _allPlayerGrabs.Add(this);

            if (_aimController == null) {
                Debug.LogError("Player/AimController cannot be found!");
            }
        }

        void OnDestroy() {
            //Remove ref to in static playergrab
            _allPlayerGrabs.Remove(this);
        }

        public void SetupInput(bool isPlayerOne) {
            GameManager.inputManager.AddEvent(
                isPlayerOne ? InputManager.Button.p1_pushButton : InputManager.Button.p2_pushButton,
                this.Throw
            );
        }

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
            if(grabbedBody == null) {
                IslandChunk targetChunk = targetBody.gameObject.GetComponent<IslandChunk>();

                if (targetChunk != null) { //Make sure we are trying to grab an IslandChunk
                    //Before grabbing, make the other players release this chunk
                    MakeOtherPlayerRelease(targetChunk);

                    if (targetChunk.parentIsland == null) {
                        grabbedBody = targetBody;
                        //Set parent
                        _grabbedBodyParent = targetChunk.transform.parent;
                        targetChunk.transform.parent = transform;
                        //Ignore Collision
                        DeactivateCollision(targetChunk.GetComponent<Collider2D>());
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
                            DeactivateCollision(iChunk.GetComponent<Collider2D>());
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

                bodyToAddForce.Velocity = forceDirection * THROW_FORCE / Mathf.Max(1,bodyToAddForce.Weight / 10f);


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
                ReactivateCollision(targetChunk.GetComponent<Collider2D>(), 1f);
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
                    ReactivateCollision(iChunk.GetComponent<Collider2D>(), 1f);
                    
                }
            }

            //Mark grabbed body as null
            grabbedBody = null;
        }

        private void ReactivateCollision(Collider2D otherCol, float time) {
            Coroutine disableCoroutine = StartCoroutine(ResumeCollision(otherCol, time));

            //Add coroutine to a dictionnary until is done in case it needs to be interrupted
            string colID = otherCol.GetInstanceID().ToString();
            collisionCoroutines.Add(colID, disableCoroutine);
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

        /// <summary> Check if any player is currently grabbinb bodyToMerge. If so, make that player release the object </summary>
        /// <param name="bodyToMerge">Gravity body of the body to merge</param>
        public static void UngrabBody(GravityBody bodyToMerge) {
            foreach (PlayerGrab pg in _allPlayerGrabs) {
                //Debug.Log(pg.grabbedBody);
                if (pg.grabbedBody == bodyToMerge ) {
                    //Debug.Log(pg.name + " ungrabbed " + bodyToMerge.name + " because it was merged.");
                    pg.Release();
                }
            }
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

    }
}
