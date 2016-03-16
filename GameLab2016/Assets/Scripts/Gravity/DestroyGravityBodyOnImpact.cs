using UnityEngine;
using System.Collections.Generic;
using Simoncouche.Controller;


namespace Simoncouche.Islands {
    [RequireComponent(typeof(AudioSource))]
    public class DestroyGravityBodyOnImpact : MonoBehaviour {

        [Tooltip("Time the pl")] [SerializeField]
        private float timeForDeath = 1.5f;

        [Tooltip("NOT FOR EDIT. Layers of objects to teleport into wormhole")] [SerializeField]
        private LayerMask GravityLayerMask;


        //DO SOMETHING WITH THIS
        public class playerTimeData {
            public PlayerController playerRef;
            public float currentTime;

            public playerTimeData(PlayerController pc) {
                playerRef = pc;
                currentTime = 0f;
            }
        }

        private AudioSource _audioSource;
        private List<playerTimeData> _curPlayerTimes;

        void Awake() {
            _audioSource = GetComponent<AudioSource>();
            _curPlayerTimes = new List<playerTimeData>();
        }

        void Update() {
            for (int i = 0; i < _curPlayerTimes.Count; i++) {
                _curPlayerTimes[i].currentTime += Time.deltaTime;
                if (_curPlayerTimes[i].currentTime >= timeForDeath) {
                    _curPlayerTimes[i].playerRef.OnMaelstromEnter(transform.position);
                    _curPlayerTimes.RemoveAt(i);
                    i--;
                }

            }
        }

        /// <summary> Destroy graviy body if triggered</summary>
        /// <param name="other"></param>
        void OnTriggerEnter2D(Collider2D other) {
            if (((1 << other.gameObject.layer) & GravityLayerMask) != 0 && other.gameObject != gameObject) {
                GravityBody gravityBodyScript = other.gameObject.GetComponentInChildren<GravityBody>();
                IslandChunk islandChunk = other.gameObject.GetComponentInChildren<IslandChunk>();
                PlayerController playerController = other.gameObject.GetComponentInChildren<PlayerController>();

                //Check if islandChunk exist. If so, call Maelstrom Collision Method
                if (islandChunk != null) {
                    islandChunk.OnMaelstromEnter();
                    _audioSource.PlayOneShot(GameManager.audioManager.environmentSpecificSound.maelstromDestructionSound);
                }
                //Check if playercontroller exists. If so, call Maelstrom Collision Method
                else if (playerController != null) {
                    _curPlayerTimes.Add(new playerTimeData(playerController));
                }
                //Else if gravity body exists, call destroy method
                else if (gravityBodyScript != null) {
                    if (gravityBodyScript.collisionEnabled == true) {
                        gravityBodyScript.DestroyGravityBody();
                        _audioSource.PlayOneShot(GameManager.audioManager.environmentSpecificSound.maelstromDestructionSound);
                    }
                }
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (((1 << other.gameObject.layer) & GravityLayerMask) != 0 && other.gameObject != gameObject) {
                GravityBody gravityBodyScript = other.gameObject.GetComponentInChildren<GravityBody>();
                IslandChunk islandChunk = other.gameObject.GetComponentInChildren<IslandChunk>();
                PlayerController playerController = other.gameObject.GetComponentInChildren<PlayerController>();

                //Check if playercontroller exists. If so, call Maelstrom Collision Method
                if (playerController != null) {
                    for (int i = 0; i < _curPlayerTimes.Count; i++) {
                        if (_curPlayerTimes[i].playerRef == playerController) {
                            _curPlayerTimes.RemoveAt(i);
                            return;
                        }
                    }

                }

            }
        }
    }
}
