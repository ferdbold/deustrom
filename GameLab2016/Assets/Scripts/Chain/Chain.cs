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
        private bool _beginningHookIsSet = false;
        public bool _endingHookIsSet {  get;  private set; }         

        [Tooltip("Color flickered when the chain has done more than 50% of it's duration")]
        [SerializeField]
        private Color _chainFlickerColor;
        [Tooltip("Color when the chain has done 100% of it's duration")]
        [SerializeField]
        private Color _chainDamagedColor;

        [Tooltip("When the chain is flickering starts flickering at 50% of it's duration, it pass this amount of time between each cycle")]
        [SerializeField]
        private float flickerTime = 0.25f;

        [Tooltip("When the chain is at 75% of it's duration, we divide the flicker time by this factor to accelerate the rythm")]
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
			RecalculateChainSections();
            ChainMissAndHitUpdate();
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
                    _chainSections.Add(_beginningHook.SpawnChainSection());
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
			_beginningHook = Hook.Create(this, true);

            // Position where the player threw the hook
            throwerThrowPosition = this.thrower.transform.position;
		}

        /// <summary>Create and configure the ending hook</summary>
        public void CreateEndingHook() {
            _endingHook = Hook.Create(this, false); 

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
        /// We check check if our hooks are beyond a certain distance from the player, if so they are considered as a miss if they didnt hit a island
        /// </summary>
        private void ChainMissAndHitUpdate() {
            if (!_beginningHookIsSet) {
                if (_beginningHook.attachedToTarget) {
                    //Tells to the thrower he hit the hook
                    this.thrower.BeginningHookHit();

                    //Apply the maximum distance of the first hook (maxDistanceBetweenTwoHooks divided by 2)
                    this._beginningHook.chainJoint.distance = _maxDistanceBetweenTwoHooks/2;

                    //Set this bool to true in order to stop the calls of thrower.BeginningHookHit
                    _beginningHookIsSet = true;
                }

                //If the hook returns AND if it isn't attached (cause it can attach to an island when it's returning!) AND the distance between the hook and player is lower then a certain amount
                    //Then we destroy the chain
                else if (!_beginningHook.attachedToTarget &&
                    Vector2.Distance(_beginningHook.transform.position, throwerThrowPosition) > _maxDistanceBetweenTwoHooks / 2) {
                    Debug.Log("Destroy Beginning Hook");
                    DestroyBeginningHook();
                }
            }

            if (_endingHook != null && !_endingHookIsSet) {
                if (_endingHook.attachedToTarget) {
                    ///Tells to the thrower he hit the hook
                    this.thrower.EndingHookHit();

                    // Reroute the beginning hook from the player to the ending hook
                    _beginningHook.chainJoint.connectedBody = _endingHook.rigidbody;
                    //this._beginningHook.chainJoint.distance = _maxDistanceBetweenTwoHooks;

                    //Set this bool to true in order to stop the calls of thrower.EndingHookHit
                    _endingHookIsSet = true;

                    StartCoroutine(DestroyTimer());
                }

                //Check if the ending hook is further than 
                else if (!_endingHook.attachedToTarget &&
                    Vector2.Distance(_endingHook.transform.position, throwerThrowPosition) > _maxDistanceBetweenTwoHooks / 2) {
                    Debug.Log("Destroy Ending Hook");
                    DestroyEndingHook();
                }
            }
        }

        /// <summary>
        /// Destroy the chain when we miss the first throw
        /// </summary>
        private void DestroyBeginningHook() {
            foreach(ChainSection section in _chainSections) {
                Destroy(section.gameObject);
            }

            Destroy(_beginningHook.gameObject);

            //Tells the owner he missed the beginning hook
            thrower.BeginningHookMissed();

            Destroy(this.gameObject);
        }

        /// <summary>
        /// Destroy the second part of the chain on second hook throw miss
        /// </summary>
        private void DestroyEndingHook() {
            _chainSections[_chainSections.Count - 1].AttachVisualJointTo(this.thrower.rigidbody);
            
            Destroy(_endingHook.gameObject);

            //Tells the thrower he missed the ending hook throw
            thrower.EndingHookMissed();

            //Must reset the connected rigidbody!
            _beginningHook.chainJoint.connectedBody = this.thrower.rigidbody;
        }

        /// <summary>
        /// When an attached chain has passed a certain amount of time, we destroy it using this function
        /// </summary>
        private void DestroyChain() {
            foreach (ChainSection section in _chainSections) {
                Destroy(section.gameObject);
            }

            Destroy(_beginningHook.gameObject);

            Destroy(_endingHook.gameObject);

            Destroy(this.gameObject);
        }

        /// <summary>
        /// Update the color of the chain sections and destroy the chain after a certain amount of time
        /// </summary>
        /// <returns></returns>
        IEnumerator DestroyTimer() {
            float elapsedTime = 0.0f;
            float elapsedTimeFlickering = 0.0f;
            bool flickerRythmAccelerated = false;
            bool soundIsPlaying = false;
            while (elapsedTime < _timeUntilChainExpires) {
                if (elapsedTime > _timeUntilChainExpires - _destroySoundSource.clip.length) {
                    if (!soundIsPlaying) {
                        soundIsPlaying = true;
                        _destroySoundSource.PlayOneShot(GameManager.audioManager.chainSound.chainDestruction);
                        
                    }
                }

                if (elapsedTime > _timeUntilChainExpires * 3 / 4 && !flickerRythmAccelerated) {
                    flickerRythmAccelerated = true;
                    flickerTime /= flickerTimeDivider;
                }

                if (elapsedTime > _timeUntilChainExpires / 2  && elapsedTimeFlickering < flickerTime/2) {
                    foreach (ChainSection section in _chainSections) {
                        MeshRenderer mr = section.GetComponentInChildren<MeshRenderer>();
                        mr.material.color = _chainFlickerColor;
                    }
                    elapsedTimeFlickering += Time.deltaTime;
                }

                if (elapsedTimeFlickering > flickerTime/2 || elapsedTime < _timeUntilChainExpires / 2) {
                    elapsedTimeFlickering += Time.deltaTime;
                    if (elapsedTimeFlickering > flickerTime) elapsedTimeFlickering = 0.0f;
                    UpdateChainSectionsColor(elapsedTime);
                }
                elapsedTime += Time.deltaTime;
                
                yield return null;
            }
            DestroyChain();
        }

        /// <summary>
        /// Simple function in order to lerp the color of the chain to red the longer it stays attached
        /// </summary>
        /// <param name="time"></param>
        private void UpdateChainSectionsColor(float time) {
            foreach (ChainSection section in _chainSections) {
                MeshRenderer mr = section.GetComponentInChildren<MeshRenderer>();
                mr.material.color = Color.Lerp(Color.white, _chainDamagedColor, time / _timeUntilChainExpires);
            }
        }

        public void RetractChain(float retractDistance) {
            if (_endingHookIsSet && _beginningHook.chainJoint!=null) {
                float tempDistance = _beginningHook.chainJoint.distance;
                tempDistance = Mathf.Clamp(tempDistance - retractDistance, 0f,_beginningHook.chainJoint.distance);
                _beginningHook.chainJoint.distance = tempDistance;
            }
        }

    }
}
