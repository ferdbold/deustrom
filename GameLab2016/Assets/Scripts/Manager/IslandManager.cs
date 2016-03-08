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
        private GameObject[] AssembleParticlePrefab;

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

        #region HandleCollision

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
                    if (isA) AddChunkToExistingIsland(a_IslandLink, chunk);
                    else AddChunkToExistingIsland(a_IslandLink, chunk);

                    /*
                    a_IslandLink.AddChunkToIsland(chunk, GetMergingPoint((isA ? b : a).transform.position, 
                                                                         (isA ? a : b).transform.position), 
                                                                         (isA ? a : b).transform.rotation.eulerAngles);
                    PlayerGrab.UngrabBody(chunk.gravityBody);
                    */
                    
                }

                //Call the to island to be connected (The many ternary operator are for the possible value if one or the other island are assembled (hi hi)
                (isA ? b_IslandLink : a_IslandLink).ConnectIslandToIsland(
                    FindTargetLocalPosition(
                        isA ? b : a,
                        isA ? b_anchor : a_anchor,
                        isA ? b_IslandLink : a_IslandLink
                    ),
                    FindTargetRotForAnchor(
                        isA ? b_anchor : a_anchor,
                        isA ? a_anchor : b_anchor   
                    ),
                    isA ? a_IslandLink : b_IslandLink,
                    _chunkMergeTime
                );

                OnJoinChunk(b_anchor, b.color);
                StartCoroutine(TimerIslandRemove(_chunkMergeTime, isA ? b_IslandLink : a_IslandLink));
                (isA ? b_IslandLink : a_IslandLink).RecreateIslandChunkConnection();
            } 

            //If only a is contained in a Island
            else if (a_IslandLink != null) {
                AddChunkToExistingIsland(a_IslandLink,b);
               
                //a_IslandLink.AddChunkToIsland(b, GetMergingPoint(b.transform.position, a.transform.position), a.transform.rotation.eulerAngles);
                //PlayerGrab.UngrabBody(b.gravityBody);
               
                JoinTwoChunk(b, b_anchor, a, a_anchor, a_IslandLink);
                a_IslandLink.RecreateIslandChunkConnection();
           } 

           //If only b is contained in a Island
           else if (b_IslandLink != null) {
                AddChunkToExistingIsland(b_IslandLink, a);
                
                //b_IslandLink.AddChunkToIsland(a, GetMergingPoint(a.transform.position, b.transform.position), b.transform.rotation.eulerAngles);
                //PlayerGrab.UngrabBody(a.gravityBody);
                
                JoinTwoChunk(a, a_anchor, b, b_anchor, b_IslandLink);
                b_IslandLink.RecreateIslandChunkConnection();
            } 

            //If a & b are not contained in a Island
            else {
                Island createdIsland = CreateIsland(a, b);
                JoinTwoChunk(b, b_anchor, a, a_anchor, createdIsland);
                createdIsland.RecreateIslandChunkConnection();
            }

            TestIslandForConversion(a.parentIsland);
        }

        /// <summary>
        /// Adds a chunk to an island
        /// </summary>
        /// <param name="islandLink">Island to add chunk to</param>
        /// <param name="chunk">Chunk to add to island</param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void AddChunkToExistingIsland(Island islandLink, IslandChunk chunk) {
            islandLink.AddChunkToIsland(chunk);
            PlayerGrab.UngrabBody(chunk.gravityBody);
            PlayerGrab.RemoveCollisionIfGrabbed(islandLink, chunk);
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
        private Island CreateIsland(IslandChunk a, IslandChunk b) {
            GameObject island = Instantiate(_islandComponent, a.transform.position, a.transform.rotation) as GameObject;
            island.name = "Island";
            if (_islandSubFolder != null) {
                island.transform.SetParent(_islandSubFolder);
            }

            
            AddChunkToExistingIsland(island.GetComponent<Island>(), a);
            AddChunkToExistingIsland(island.GetComponent<Island>(), b);

            /*
            island.GetComponent<Island>().AddChunkToIsland(a, GetMergingPoint(b.transform.position, a.transform.position), a.transform.rotation.eulerAngles);
            PlayerGrab.UngrabBody(a.gravityBody);
            island.GetComponent<Island>().AddChunkToIsland(b, GetMergingPoint(b.transform.position, a.transform.position), a.transform.rotation.eulerAngles);
            PlayerGrab.UngrabBody(b.gravityBody);
            */

            _island.Add(island.GetComponent<Island>());

            return island.GetComponent<Island>();
        }

        /// <summary>
        /// Removes the Island from the list then destroy it
        /// </summary>
        /// <param name="Island">The Island that need to be removed</param>
        private void RemoveIsland(Island Island) {

            _island.Remove(Island);
            if (Island != null) {
                Destroy(Island.gameObject);
            }
        }

        #endregion

        #region Destroy Island/Chunks
        /// <summary>
        /// Handles the destruction of the chunk
        /// </summary>
        /// <param name="chunk">chunk to destroy</param>
        public void DestroyChunk(IslandChunk chunk) {
            PlayerGrab.UngrabBody(chunk.gravityBody);
            chunk.gravityBody.DestroyGravityBody();
        }

        /// <summary> Remove Island from the list and call a destroyChunk for each chunk of the island </summary>
        /// <param name="island">island to destroy</param>
        public void DestroyIsland(Island island) {
            foreach (IslandChunk chunk in island.chunks) DestroyChunk(chunk);
            RemoveIsland(island);
        }

        /// <summary>
        /// Check if an island was broken by the destruction/disassembling of one of his chunk
        /// </summary>
        /// <param name="island">The target island to check</param>
        public void CheckIslandBroken(Island island) {
            if (island == null || island.chunks.Count <= 0) {
                _island.Remove(island);
                Destroy(island.gameObject); //Must destroy the island if there is no more chunks attached to the island
                return;
            }

            island.RecreateIslandChunkConnection();
            List<IslandChunk> chunkIsland = new List<IslandChunk>();
            chunkIsland = CheckIslandBroken_Helper(island.chunks[0], chunkIsland);
            List<IslandChunk> chunkChecked = chunkIsland;
            //Debug.Log(chunkIsland.Count);
            //is broken
            while (chunkChecked.Count != island.chunks.Count) {
                //Find list of Chunk that should be island
                chunkIsland.Clear();
                foreach (IslandChunk chunk in island.chunks) {
                    if (!chunkChecked.Contains(chunk)) {
                        chunkIsland = CheckIslandBroken_Helper(chunk, chunkIsland);
                        break;
                    }
                }

                //Debug.Log(chunkIsland.Count);
                chunkChecked.AddRange(chunkIsland);

                //Remove Chunk
                if (chunkIsland.Count == 1) {
                    island.chunks.Remove(chunkIsland[0]);
                    chunkIsland[0].transform.SetParent(island.transform.parent);
                }

                //Create Island
                else if (chunkIsland.Count != 0) {
                    foreach (IslandChunk chunk in chunkIsland) {
                        island.chunks.Remove(chunk);
                    }
                    Island newIsland = CreateIsland(chunkIsland[0], chunkIsland[1]);
                    for (int i = 2; i < chunkIsland.Count; i++) {
                        AddChunkToExistingIsland(newIsland, chunkIsland[i]);
                    }
                    newIsland.RecreateIslandChunkConnection();
                }
            }
            island.RecreateIslandChunkConnection();
        }

        /// <summary>
        /// The helper function to recursively check the island
        /// </summary>
        /// <param name="current"></param>
        /// <param name="islandChecked"></param>
        /// <returns></returns>
        private List<IslandChunk> CheckIslandBroken_Helper(IslandChunk current, List<IslandChunk> islandChecked) {
            islandChecked.Add(current);

            foreach (IslandChunk connection in current.connectedChunk) {
                if (islandChecked.Count == current.parentIsland.chunks.Count) break;

                if (connection != null && !islandChecked.Contains(connection)) {
                    CheckIslandBroken_Helper(connection, islandChecked);
                }
            }

            return islandChecked;
        }


        #endregion

        #region Take Damage

        /// <summary>
        /// Handle the damage of a chunk and it's division
        /// </summary>
        /// <param name="chunk">The chunk affected by this</param>
        /// <param name="damage">The number of chunk affected</param>
        public void TakeDamageHandler(IslandChunk chunk, int damage) {
            //If no damage
            if (damage < 1) {
                return;
            }

            //If the chunk has no connection
            if (chunk.connectedChunk == null || chunk.connectedChunk.Count == 0) {
                return;
            }

            Island islandLink = chunk.parentIsland;

            //Check if the damage is too high for the island (the maximum is to divided the island in 2
            if (islandLink.chunks.Count <= damage) {
                damage = Mathf.CeilToInt(islandLink.chunks.Count / 2f);
            }

            //Recursivly remove island
            List<IslandChunk> islandRemoved = new List<IslandChunk>();
            islandRemoved.Add(chunk);
            if (damage > 1) {
                islandRemoved = DamageConnectedIsland(chunk, islandRemoved, damage);
            }

            //Divide Island
            if (islandRemoved.Count > 1)  { //Multiple Chunk
                Island island = CreateIsland(islandRemoved[0], islandRemoved[1]);
                if (islandRemoved.Count >= 3) {
                    for (int i = 2; i < islandRemoved.Count; i++) {
                        island.AddChunkToIsland(islandRemoved[i]);
                    }
                }
                island.CenterIslandRoot();
            }

            //Remove connection (only need to remove the connection from one side, one chunk removes the connection from both)
            foreach (IslandChunk c in islandLink.chunks) {
                if (!islandRemoved.Contains(c)) {
                    c.RemoveConnectedChunk(islandRemoved);
                } 
            }

            /*DEBUG Destruction TODO replace with separation*/
            foreach (IslandChunk c in islandRemoved) {
                Destroy(c.gameObject);
            }

            //Check if the island is broken in pieces
            CheckIslandBroken(islandLink);

            //Find the chunk to give velocity to
            /*
            List<IslandChunk> chunkAccelerated = new List<IslandChunk>();
            List<Island> islandAccelerated = new List<Island>();
            foreach (IslandChunk c in islandRemoved) {
                Collider2D[] others = Physics2D.OverlapCircleAll(c.transform.position, 2f);

                foreach (Collider2D other in others) {
                    IslandChunk chk = other.GetComponent<IslandChunk>();

                    if (chk != null && !islandRemoved.Contains(chk)) {
                        if (chk.parentIsland != null && !islandAccelerated.Contains(chk.parentIsland)) {
                            islandAccelerated.Add(chk.parentIsland);
                        } else if (!chunkAccelerated.Contains(chk)) {
                            chunkAccelerated.Add(chk);
                        }
                    }
                }
            }
            
            //Give velocity to chunks around the destroyed ones
            Vector2 removedCenter = islandRemoved.Count > 1 ? islandRemoved[0].parentIsland.transform.localPosition : islandRemoved[0].transform.position;

            foreach (IslandChunk c in chunkAccelerated) {
                c.gravityBody.Velocity = CalculateVelocityAfterHit((Vector2)c.transform.localPosition - removedCenter, damage, 1);
            }
            foreach (Island i in islandAccelerated) {
                i.gravityBody.Velocity = CalculateVelocityAfterHit((Vector2)i.transform.localPosition - removedCenter, damage, i.chunks.Count);
            }/**/
        }

        /// <summary>
        /// Calculate the velocity resulting from a chunk being destroyed
        /// </summary>
        /// <param name="dir">The direction vector</param>
        /// <param name="power">The power of the impact</param>
        /// <param name="weight">The weight of the thing being impacted</param>
        /// <returns>The resulting velocity</returns>
        private Vector2 CalculateVelocityAfterHit(Vector2 dir, int power, int weight) {
            return dir * (Mathf.Max(1, (float)power - (Mathf.Floor((float)weight / 4))));
        }

        /// <summary>
        /// The recursive helper to disconnected chunks
        /// </summary>
        /// <param name="current">the current island chunk to check</param>
        /// <param name="islandRemoved">the list of chunk currently being removed</param>
        /// <param name="damage">the number of chunk remaining to damage</param>
        /// <returns></returns>
        private List<IslandChunk> DamageConnectedIsland(IslandChunk current, List<IslandChunk> islandRemoved, int damage) {
            for (int i = current.connectedChunk.Count - 1; i >= 0; i++) { //Starts from end to get chunk recently removed
                if (islandRemoved.Count == damage) {
                    return islandRemoved;
                }

                if (!islandRemoved.Contains(current.connectedChunk[i])) {
                    islandRemoved.Add(current.connectedChunk[i]);
                }
            }

            //DamageConnected Island
            for (int i = current.connectedChunk.Count - 1; i >= 0; i++) { //Starts from end to get chunk recently removed
                if (islandRemoved.Count == damage) {
                    return islandRemoved;
                }

                islandRemoved = DamageConnectedIsland(current.connectedChunk[i], islandRemoved, damage);
            }

            return islandRemoved;
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Test if any chunk as part of the island can be converted
        /// </summary>
        /// <param name="island">The island to test</param>
        public void TestIslandForConversion(Island island) {
            foreach (IslandChunk chunk in island.chunks) {
                int sobekChunk = 0;
                int cthuluChunk = 0;
                foreach (IslandChunk connection in chunk.connectedChunk) {
                    if (connection != null) {
                        if (connection.color == IslandUtils.color.red) sobekChunk++;
                        else if (connection.color == IslandUtils.color.blue) cthuluChunk++;
                    }
                }

                IslandUtils.color newColor = ColorConversion(sobekChunk, cthuluChunk, chunk.color);
                if (newColor != chunk.color) {
                    chunk.ConvertChunkToAnotherColor(newColor);
                }
            }
        }

        /// <summary>
        /// Which color should the conversion return
        /// </summary>
        /// <param name="c_sobek">number of chunk from sobek around this one</param>
        /// <param name="c_cthulu">number of chunk from cthulu around this one</param>
        /// <param name="current">the current color of the island</param>
        /// <returns>The new color for the island</returns>
        private IslandUtils.color ColorConversion(int c_sobek, int c_cthulu, IslandUtils.color current) {
            if (current == IslandUtils.color.blue) c_cthulu++;
            else if (current == IslandUtils.color.red) c_sobek++;

            if (c_sobek < c_cthulu) return IslandUtils.color.blue;
            else if (c_sobek > c_cthulu) return IslandUtils.color.red;
            else return current;
        }

        #endregion

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
                FindTargetLocalPosition(b, b_anchor),
                FindTargetRotForAnchor(a_anchor, b_anchor),
                b,
                targetIsland,
                _chunkMergeTime
            );
            OnJoinChunk(b_anchor, b.color);
        }

        /// <summary>
        /// Event when a chunk is joined to another (same for island to island merge)
        /// </summary>
        /// <param name="anchor">The anchor to spawn the particle</param>
        private void OnJoinChunk(IslandAnchorPoints anchor, Islands.IslandUtils.color color) {
            //Get Particles Index from color
            int type = -1;
            switch (color) {
                case Islands.IslandUtils.color.red: type = 0; break;
                case Islands.IslandUtils.color.blue: type = 1; break;
                default: Debug.Log("Island color is not not blue or red ! "); break;
            }

            //Instantiate Particles FX
            GameObject ParticleGO = (GameObject)Instantiate(AssembleParticlePrefab[type], anchor.transform.position + new Vector3(0, 0, -1.25f), Quaternion.identity);
            ParticleGO.transform.parent = anchor.transform;
        }

        /// <summary>
        /// Find the correct euler angle to be at the right position for the anchor
        /// </summary>
        /// <param name="a">The point to be merge to other island chunk</param>
        /// <param name="b">Island that point a merges to</param>
        /// <returns>euler angle</returns>
        private Vector3 FindTargetRotForAnchor(IslandAnchorPoints a, IslandAnchorPoints b) {
            return new Vector3(0, 0, a.angle - b.angle - b.GetIslandChunk().transform.localRotation.eulerAngles.z + 180);
        }

        /// <summary>
        /// Find the target local position
        /// </summary>
        /// <param name="a">The anchor of the island to merge</param>
        /// <param name="b_chunk">Island to be merge to</param>
        /// <returns></returns>
        private Vector3 FindTargetLocalPosition(IslandChunk b_chunk, IslandAnchorPoints b_anchor) {
            float distance = Vector3.Distance(Vector3.zero, b_anchor.position);
            float angle = b_anchor.angle + b_chunk.transform.localRotation.eulerAngles.z;
            Vector3 anchorProjection = new Vector3(
                distance * Mathf.Cos(angle * Mathf.PI / 180f),
                distance * Mathf.Sin(angle * Mathf.PI / 180f),
                0
            ); //The projection of the anchor on the current angle of the chunk
            return (Vector2)b_chunk.transform.localPosition + 2f * (Vector2)anchorProjection;
        }

        /// <summary>
        /// Find the target local position
        /// </summary>
        /// <param name="a">The anchor of the island to merge</param>
        /// <param name="b_chunk">Island to be merge to</param>
        /// <returns></returns>
        private Vector3 FindTargetLocalPosition(IslandChunk b_chunk, IslandAnchorPoints b_anchor, Island b_island) {
            float distance = Vector3.Distance(Vector3.zero, b_anchor.position);
            float angle = b_anchor.angle + b_chunk.transform.localRotation.eulerAngles.z;
            Vector3 anchorProjection = new Vector3(
                distance * Mathf.Cos(angle * Mathf.PI / 180f),
                distance * Mathf.Sin(angle * Mathf.PI / 180f),
                0
            ); //The projection of the anchor on the current angle of the chunk
            return (Vector2)b_island.transform.localPosition + (Vector2)b_chunk.transform.localPosition + 2f * (Vector2)anchorProjection;
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
