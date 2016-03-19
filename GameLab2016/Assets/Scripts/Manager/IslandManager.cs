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

        /// <summary> A list of every IslandChunk currently in play</summary>
        private List<IslandChunk> _islandChunks = new List<IslandChunk>();

        [SerializeField] [Tooltip("Island Object Prefab Reference")]
        private GameObject _islandComponent = null;

        [Header("Conversion")]

        [SerializeField] [Tooltip("The time it takes for two island to make a conversion")]
        private float _conversionTime = 1f;

        [Header("Visuals")]

        [SerializeField] [Tooltip("Particle spawned when island assemble")]
        private GameObject[] AssembleParticlePrefab;

        [SerializeField] [Tooltip("Particle spawned when island gets destroyed by collision with high speed impacting object")]
        private GameObject DestroyParticle;

        
        [SerializeField] [Tooltip("The time it takes for 2 chunks to do their merging anim")]
        private float _chunkMergeTime = 1f;

        /// <summary> the island subfolder in scene </summary>
        private Transform _islandSubFolder;

        public void Setup() {
            try {
                _islandSubFolder = GameObject.FindWithTag("IslandSubFolder").transform;
            }
            catch (System.NullReferenceException e) {
                Debug.LogWarning("No Island sub folder in scene, you might have forgotten to add the tag to the subfolder. Error Thrown: " + e.Message);
            }
        }

        void Start() {
            //Add starting chunks to the chunk list
            IslandChunk[] chunks = GameObject.FindObjectsOfType<IslandChunk>();
            foreach (IslandChunk chunk in chunks) {
                CreatedIslandChunk(chunk);
            }
        }

        /// <summary> Called when an island is created. Add it's reference to the island chunk list  </summary>
        /// <param name="ic">created island chunk</param>
        public void CreatedIslandChunk(IslandChunk chunk) {
            _islandChunks.Add(chunk);
        }

        #region Get/Set
        public List<Island> GetIslands() { return _island; }
        public List<IslandChunk> GetIslandChunks() { return _islandChunks; }
        public Transform GetIslandSubFolder() { return _islandSubFolder; }
        public float GetMergeTime() { return _chunkMergeTime; }
        public float GetConversionTime() { return _conversionTime; }
        #endregion

        #region HandleCollision

        /// <summary>
        /// Creates a Island from 2 chunk, Will not work for multiple piece of the same letter in one scene
        /// </summary>
        /// <param name="a">the first chunk</param>
        /// /// <param name="a_anchor">the anchor associated to a</param>
        /// <param name="b">the second chunk</param>
        /// <param name="b_anchor">the anchor associated to b</param>
        public void HandleChunkCollision(IslandChunk a, IslandAnchorPoints a_anchor, IslandChunk b, IslandAnchorPoints b_anchor) {
            Island a_IslandLink = a.parentIsland;
            Island b_IslandLink = b.parentIsland;
            
            //If both are contained in Island
            if (a_IslandLink != null && b_IslandLink != null && a_IslandLink != b_IslandLink) {
                if (IslandUtils.CheckIfOnSameIsland(a_IslandLink, b_IslandLink)) return;

                //Is A Island bigger than B Island
                bool mergeA = a_IslandLink.chunks.Count <= b_IslandLink.chunks.Count;
                Island islandToMerge = mergeA ? a_IslandLink : b_IslandLink;
                Island targetIsland = mergeA ? b_IslandLink : a_IslandLink;
                IslandChunk chunkToMerge = mergeA ? a : b;
                IslandChunk targetChunk = mergeA ? b : a;
                IslandAnchorPoints anchorToMerge = mergeA ? a_anchor : b_anchor;
                IslandAnchorPoints targetAnchor = mergeA ? b_anchor : a_anchor;

                AddChunkToExistingIsland(targetIsland, chunkToMerge);
                islandToMerge.chunks.Remove(chunkToMerge);
                Vector3 targetPos = FindTargetLocalPosition(targetChunk, targetAnchor);
                Vector3 targetRot = FindTargetRotForAnchor(anchorToMerge, targetAnchor);
                chunkToMerge.ConnectChunk(targetPos, targetRot, null, null, _chunkMergeTime); //change b
                targetIsland.islandColliders.AddCollision(chunkToMerge, targetPos);
                OnJoinChunk(targetAnchor, targetChunk.color);

                //Merge every chunk
                foreach (IslandChunk chunk in islandToMerge.chunks) {
                    AddChunkToExistingIsland(targetIsland, chunk);
                    targetPos = FindTargetLocalPositionBasedOnPosRot(
                            chunkToMerge.transform.localPosition,
                            chunkToMerge.transform.localRotation.eulerAngles,
                            targetPos,
                            targetRot,
                            chunk
                     );
                    chunkToMerge = chunk;
                    chunk.ConnectChunk(
                        targetPos,
                        targetRot, //TODO relative rot
                        null, //depracated
                        null, //depracated
                        _chunkMergeTime
                    );
                    targetIsland.islandColliders.AddCollision(chunk, targetPos);
                }

                //Finish merge
                targetIsland.RecreateIslandChunkConnection();
                RemoveIsland(islandToMerge);
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
                createdIsland.islandColliders.AddCollision(a, Vector3.zero);
                createdIsland.RecreateIslandChunkConnection();
            }

            //TestIslandForConversion(a.parentIsland); //Check each insland individually for conversion
            SimpleIslandConversion(a.parentIsland); //All chunks in island change to the same color which is the most present
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
            PlayerGrab.UngrabBody(chunk.gravityBody,true);
            PlayerGrab.RemoveCollisionIfGrabbed(islandLink, chunk);
        }

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
            _islandChunks.Remove(chunk);
            PlayerGrab.UngrabBody(chunk.gravityBody);
            chunk.gravityBody.DestroyGravityBody();
            foreach (GameObject chain in GameObject.FindGameObjectsWithTag("Chain")) {
                chain.SendMessage("AttachedHookToIslandsUpdate");
            }
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
                DestroyIsland(island);
                return;
            }
            island.RecreateIslandChunkConnection();
            List<IslandChunk> originChunkList = island.chunks;
            List<IslandChunk> chunkIsland = new List<IslandChunk>();
            chunkIsland = CheckIslandBroken_Helper(island.chunks[0], chunkIsland);
            List<IslandChunk> chunkChecked = chunkIsland;
            
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
                    island.RemoveChunkToIsland(chunkIsland[0]);
                    chunkIsland[0].transform.SetParent(island.transform.parent, true);
                }

                //Create Island
                else if (chunkIsland.Count != 0) {
                    foreach (IslandChunk chunk in chunkIsland) {
                        island.RemoveChunkToIsland(chunk);
                    }
                    Island newIsland = CreateIsland(chunkIsland[0], chunkIsland[1]);
                    for (int i = 2; i < chunkIsland.Count; i++) {
                        AddChunkToExistingIsland(newIsland, chunkIsland[i]);
                    }
                    foreach (IslandChunk chunk in newIsland.chunks) {
                        newIsland.islandColliders.AddCollision(chunk, chunk.transform.localPosition);
                    }
                    newIsland.RecreateIslandChunkConnection();
                }
            }
            island.RecreateIslandChunkConnection();

            //Make the player lose the connection to island being dismantle
            if (island.chunks.Count != originChunkList.Count) {
                foreach (IslandChunk check in chunkChecked) {
                    PlayerGrab.ReactivateCollisionForBothPlayer(check.GetComponent<CircleCollider2D>());
                }
            }

            //Update Conversion Status of the island
            island.UpdateConversionStatus();
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
                if (current != null && islandChecked.Count == current.parentIsland.chunks.Count) break;

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

            //Spawn Particle and play sound
            Instantiate(DestroyParticle, chunk.transform.position + new Vector3(0, 0, -1.25f), Quaternion.identity);

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
                //island.CenterIslandRoot();
            }

            //Remove connection (only need to remove the connection from one side, one chunk removes the connection from both)
            foreach (IslandChunk c in islandLink.chunks) {
                if (!islandRemoved.Contains(c)) {
                    c.RemoveConnectedChunk(islandRemoved);
                } 
            }

            /*DEBUG Destruction TODO replace with separation*/
            foreach (IslandChunk c in islandRemoved) {
                c._parentIsland.RemoveChunkToIsland(c);
                DestroyChunk(c);
            }

            //Check if the island is broken in pieces
            CheckIslandBroken(islandLink);

            /*//EXPERIMENTAL
            //Find the chunk to give velocity to
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
        /// Test if any chunk as part of the island can be converted
        /// </summary>
        /// <param name="island">The island to test</param>
        public void SimpleIslandConversion(Island island) {
            island.UpdateConversionStatus();
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
            Vector3 targetPos = FindTargetLocalPosition(b, b_anchor);
            a.ConnectChunk(
                targetPos,
                FindTargetRotForAnchor(a_anchor, b_anchor),
                null, //Depracated
                null, //Depracated
                _chunkMergeTime
            );
            targetIsland.islandColliders.AddCollision(a, targetPos);
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
            //Debug.Log(a.angle + " " + a.GetIslandChunk().transform.rotation.eulerAngles.z + " " + b.GetIslandChunk().transform.rotation.eulerAngles.z + " " + b.angle);
            //Debug.Log((b.angle + b.GetIslandChunk().transform.rotation.eulerAngles.z + 180 - a.angle) % 360);
            return new Vector3(0, 0, (b.angle + b.GetIslandChunk().transform.rotation.eulerAngles.z + 180 - a.angle) % 360);
        }

        /// <summary>
        /// Find the target local position
        /// </summary>
        /// <param name="a">The anchor of the island to merge</param>
        /// <param name="b_chunk">Island to be merge to</param>
        /// <returns></returns>
        private Vector3 FindTargetLocalPosition(IslandChunk b_chunk, IslandAnchorPoints b_anchor) {
            float distance = Vector3.Distance(b_chunk.transform.position, b_anchor.transform.position);
            float angle = b_anchor.angle + b_chunk.transform.localRotation.eulerAngles.z;
            Vector3 anchorProjection = new Vector3(
                distance * Mathf.Cos(angle * Mathf.PI / 180f),
                distance * Mathf.Sin(angle * Mathf.PI / 180f),
                0
            ); //The projection of the anchor on the current angle of the chunk
            return b_chunk.transform.localPosition + 2f * anchorProjection;
        }

        /// <summary>
        /// Used when an island hits another, to get target pos relative to hit chunk
        /// </summary>
        /// <param name="originalPos">the original pos of the first chunk</param>
        /// <param name="originalRot">the original rot of the first chunk</param>
        /// <param name="targetPos">the target pos of the first chunk</param>
        /// <param name="targetRot">the target rot of the first chunk</param
        /// <param name="currentChunk">The chunk that has is position being calculated</param>
        /// <returns>the target pos relative to first chunk</returns>
        private Vector3 FindTargetLocalPositionBasedOnPosRot(Vector3 originalPos, Vector3 originalRot, Vector3 targetPos, Vector3 targetRot, IslandChunk currentChunk) {
            float changeInRot = targetRot.z + originalRot.z + 180;
            float distance = Vector3.Distance(originalPos, currentChunk.transform.localPosition);
            
            Vector3 chunkRotProjection = new Vector3(
                targetPos.x + distance * Mathf.Cos(changeInRot * Mathf.PI / 180f),
                targetPos.y + distance * Mathf.Sin(changeInRot * Mathf.PI / 180f),
                0
            );

            //TODO finish rotation
            return targetPos + currentChunk.transform.localPosition - originalPos;
            //return chunkRotProjection;
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
