using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Simoncouche.Controller;

namespace Simoncouche.Islands {
	/// <summary>
	/// Every Island data
	/// </summary>
	public class IslandManager : MonoBehaviour {

		/// <summary>
		/// A list of every Island currently in play
		/// </summary>
		private List<Island> _island = new List<Island>();

		[SerializeField] [Tooltip("Island Object Prefab Reference")]
		private GameObject _islandComponent = null;

        [Header("Visuals")]

        [SerializeField] [Tooltip("Particle spawned when island assemble")]
        private GameObject AssembleParticlePrefab;

        [SerializeField]
        [Tooltip("The time it takes for 2 chunks to do their merging anim")]
        private float _chunkMergeTime = 1f;

        /// <summary> the island subfolder in scene </summary>
        private Transform _islandSubFolder;

        private PlayerGrab _playerGrab;

        void Awake() {
            GameObject playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null) _playerGrab = playerGO.GetComponent<PlayerGrab>();
            if(_playerGrab == null) Debug.LogError("_PlayerGrab cannot be found!");

            try {
                _islandSubFolder = GameObject.FindWithTag("IslandSubFolder").transform;
            }
            catch (System.NullReferenceException e) {
                Debug.LogWarning("No Island sub folder in scene, you might have forgotten to add the tag to the subfolder. Error Thrown: " + e.Message);
            }
        }

		/// <summary>
		/// Creates a Island from 2 chunk, Will not work for multiple piece of the same letter in one scene
		/// </summary>
		/// <param name="a">the first chunk</param>
		/// /// <param name="a_anchor">the anchor associated to a</param>
		/// <param name="b">the second chunk</param>
		/// <param name="b_anchor">the anchor associated to b</param>
		public void HandleChunkCollision(IslandChunk a, IslandAnchorPoints a_anchor, IslandChunk b, IslandAnchorPoints b_anchor) {
			Island a_IslandLink = ChunkContainedInIsland(a);
			Island b_IslandLink = ChunkContainedInIsland(b);
			
			//If both are contained in Island
			if (a_IslandLink != null && b_IslandLink != null && a_IslandLink != b_IslandLink) {
				if (IslandUtils.CheckIfOnSameIsland(a_IslandLink, b_IslandLink)) return;

                //Is A Island bigger than B Island
				bool isA = a_IslandLink.chunks.Count <= b_IslandLink.chunks.Count;

				List<IslandChunk> chunks = isA ? b_IslandLink.chunks : a_IslandLink.chunks;
				foreach (IslandChunk chunk in chunks) {
					a_IslandLink.AddChunkToIsland(chunk, GetMergingPoint((isA ? b : a).transform.position, 
																		 (isA ? a : b).transform.position), 
																		 (isA ? a : b).transform.rotation.eulerAngles);
				}

                //Call the to island to be connected (The many ternary operator are for the possible value if one or the other island are assembled (hi hi)
                (isA ? b_IslandLink : a_IslandLink).ConnectIslandToIsland(
                    FindTargetLocalPosition(
                        isA ? b_anchor : a_anchor,
                        isA ? b_IslandLink : a_IslandLink,
                        isA ? a_anchor : b_anchor
                    ),
                    FindTargetRotForAnchor(
                        isA ? b_anchor : a_anchor,
                        isA ? a_anchor : b_anchor   
                    ),
                    isA ? a_IslandLink : b_IslandLink,
                    _chunkMergeTime
                );

                OnJoinChunk(b_anchor);
                StartCoroutine(TimerIslandRemove(_chunkMergeTime, isA ? b_IslandLink : a_IslandLink));
                CheckPlayerGrab(chunk.gravityBody);
			} 

			//If a is contained in a Island
			else if (a_IslandLink != null) {
				a_IslandLink.AddChunkToIsland(b, GetMergingPoint(b.transform.position, a.transform.position), a.transform.rotation.eulerAngles);
                CheckPlayerGrab(b.gravityBody);
                JoinTwoChunk(b, b_anchor, a, a_anchor, a_IslandLink);
			} 
			
			//If b is contained in a Island
			else if (b_IslandLink != null) {
				b_IslandLink.AddChunkToIsland(a, GetMergingPoint(a.transform.position, b.transform.position), b.transform.rotation.eulerAngles);
                CheckPlayerGrab(a.gravityBody);
                JoinTwoChunk(a, a_anchor, b, b_anchor, b_IslandLink);
			} 
			
			//If a & b are not contained in a Island
			else {
				CreateIsland(a, b);
				JoinTwoChunk(b, b_anchor, a, a_anchor, ChunkContainedInIsland(a));
			}
		}

        /// <summary>
        /// Timer before the island is remove and merged
        /// </summary>
        /// <param name="time">timer time</param>
        /// <param name="island">island to be removed</param>
        /// <returns></returns>
        private IEnumerator TimerIslandRemove(float time, Island island) { yield return new WaitForSeconds(time); RemoveIsland(island); }

		/// <summary>
		/// Check if the chunk is contained in a Island
		/// </summary>
		/// <param name="chunk">Chunk to check</param>
		/// <returns>THe reference to the Island that contains this chunk or null if no Island contains it </returns>
		private Island ChunkContainedInIsland(IslandChunk chunk) {
			foreach (Island Island in _island) {
				if (Island.IslandContainsChunk(chunk)) return Island;
			}
			return null;
		}

		/// <summary>
		/// Creates a Island at position a and adds a, b has it's child. Adds the new Island to the _Island list
		/// </summary>
		/// <param name="a">First chunk</param>
		/// <param name="b">Second chunk</param>
		private void CreateIsland(IslandChunk a, IslandChunk b) {
			GameObject island = Instantiate(_islandComponent, a.transform.position, a.transform.rotation) as GameObject;
			island.name = "Island";
            if (_islandSubFolder != null) {
                island.transform.SetParent(_islandSubFolder);
            }

			island.GetComponent<Island>().AddChunkToIsland(a, GetMergingPoint(b.transform.position, a.transform.position), a.transform.rotation.eulerAngles);
			island.GetComponent<Island>().AddChunkToIsland(b, GetMergingPoint(b.transform.position, a.transform.position), a.transform.rotation.eulerAngles);

			_island.Add(island.GetComponent<Island>());
		}

		/// <summary>
		/// Removes the Island from the list then destroy it
		/// </summary>
		/// <param name="Island">The Island that need to be removed</param>
		private void RemoveIsland(Island Island) {
			_island.Remove(Island);
			Destroy(Island.gameObject);
		}

        /// <summary> Check if player is currently grabbinb bodyToMerge. If so, make the player release the object </summary>
        /// <param name="bodyToMerge">Gravity body of the body to merge</param>
        private void CheckPlayerGrab(GravityBody bodyToMerge) {
            if(bodyToMerge == _playerGrab.grabbedBody) {
                _playerGrab.Release();
            }

        }

		#region Utils

		/// <summary>
		/// Join chunk to another
		/// </summary>
		/// <param name="a">The chunk to be joined</param>
		/// /// <param name="a_anchor">anchor assossiated to a</param>
		/// <param name="b">The chunk joined to</param>
		/// <param name="b_anchor">anchor assossiated to b</param>
		private void JoinTwoChunk(IslandChunk a, IslandAnchorPoints a_anchor, IslandChunk b, IslandAnchorPoints b_anchor, Island targetIsland) {
            //Debug.Log(a.transform.localPosition + " " + b.transform.localPosition);
			a.ConnectChunk(
                FindTargetLocalPosition(a_anchor, b, b_anchor),
				FindTargetRotForAnchor(a_anchor, b_anchor),
				b,
                targetIsland,
                _chunkMergeTime
            );
            OnJoinChunk(b_anchor);
        }

        /// <summary>
        /// Event when a chunk is joined to another (same for island to island merge)
        /// </summary>
        /// <param name="anchor">The anchor to spawn the particle</param>
        private void OnJoinChunk(IslandAnchorPoints anchor) {
            //Instantiate Particles FX
            GameObject ParticleGO = (GameObject)Instantiate(AssembleParticlePrefab, anchor.transform.position + new Vector3(0, 0, -1.25f), Quaternion.identity);
            ParticleGO.transform.parent = anchor.transform;
        }

		/// <summary>
		/// Find the correct euler angle to be at the right position for the anchor
		/// </summary>
		/// <param name="a">The point to be merge to other island chunk</param>
		/// <param name="b">Island that point a merges to</param>
		/// <returns>euler angle</returns>
		private Vector3 FindTargetRotForAnchor(IslandAnchorPoints a, IslandAnchorPoints b) {
			return new Vector3(0, 0, a.angle - b.angle + 180);
		}

        /// <summary>
        /// Find the target local position
        /// </summary>
        /// <param name="a">The anchor of the island to merge</param>
        /// <param name="b_chunk">Island to be merge to</param>
        /// <returns></returns>
        private Vector3 FindTargetLocalPosition(IslandAnchorPoints a, IslandChunk b_chunk, IslandAnchorPoints b) {
			return b_chunk.transform.localPosition - a.position + b.position;
		}

        /// <summary>
        /// Find the target local position
        /// </summary>
        /// <param name="a">The anchor of the island to merge</param>
        /// <param name="b_chunk">Island to be merge to</param>
        /// <returns></returns>
        private Vector3 FindTargetLocalPosition(IslandAnchorPoints a, Island a_island, IslandAnchorPoints b) {
            return a_island.transform.position - a.position + b.position;
        }

		/// <summary>
		/// Get the point of merge between 2 island chunk
		/// </summary>
		/// <param name="a">The point to be merge to other island chunk</param>
		/// <param name="b">Island that point a merges to</param>
		/// <returns>The merge position</returns>
		private Vector3 GetMergingPoint(Vector3 a, Vector3 b) {
			/*RaycastHit hit;
			Debug.DrawRay(a, b);
			Physics.Raycast(a, b, out hit, Vector3.Distance(a, b));
			return hit.point;*/
			return FindMiddlePoint(a, b);
		}

		/// <summary>
		/// Finds the middle point between two Vector3
		/// </summary>
		/// <param name="a">First Vector</param>
		/// <param name="b">Second Vector</param>
		/// <returns>Middle point between a and b</returns>
		private Vector3 FindMiddlePoint(Vector3 a, Vector3 b) {
			return (a + (b - a) / 2);
		}

		#endregion
	}
}
