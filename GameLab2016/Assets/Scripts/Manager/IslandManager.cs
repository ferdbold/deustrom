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

        /// <summary> A list of every IslandChunk not currently in play but currently ready to be released from feeder (shaking) </summary>
        private List<IslandChunk> _pendingIslandChunks = new List<IslandChunk>();

        [SerializeField] [Tooltip("Island Object Prefab Reference")]
        private GameObject _islandComponent = null;

        [Header("Conversion")]

        [SerializeField] [Tooltip("The time it takes for two island to make a conversion")]
        private float _conversionTime = 1f;

        [Header("Visuals")]

        [SerializeField] [Tooltip("Particle spawned when island assemble")]
        private GameObject[] AssembleParticlePrefab;

        [SerializeField] [Tooltip("Particle spawned when Sobek island gets destroyed by collision with high speed impacting object")]
        private GameObject DestroyParticle_SO;
        [SerializeField] [Tooltip("Particle spawned when Cthlhu island gets destroyed by collision with high speed impacting object")]
        private GameObject DestroyParticle_CT;


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
        public List<IslandChunk> GetPendingIslandChunks() { return _pendingIslandChunks;  }
        public void AddPendingIslandChunk(IslandChunk ic) { _pendingIslandChunks.Add(ic); }
        public void RemovePendingIslandChunk(IslandChunk ic) { _pendingIslandChunks.Remove(ic); }
        public int GetAmountPendingIslandChunk() { return _pendingIslandChunks.Count; }
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

            //If one of the island is not mergeable
            if (!a.isMergeable || !b.isMergeable) {
                return;
            }
            
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
                if (a.color == IslandUtils.color.volcano || b.color == IslandUtils.color.volcano) {
                    HandleVolcanoCollision(a, b); //Handle Volcano collision
                } else {
                    AddChunkToExistingIsland(a_IslandLink, b);

                    JoinTwoChunk(b, b_anchor, a, a_anchor, a_IslandLink);
                    a_IslandLink.RecreateIslandChunkConnection();
                }
           } 

           //If only b is contained in a Island
           else if (b_IslandLink != null) {
               if (a.color == IslandUtils.color.volcano || b.color == IslandUtils.color.volcano) {
                    HandleVolcanoCollision(a, b); //Handle Volcano collision
                } else {
                   AddChunkToExistingIsland(b_IslandLink, a);

                   JoinTwoChunk(a, a_anchor, b, b_anchor, b_IslandLink);
                   b_IslandLink.RecreateIslandChunkConnection();
               }
            } 

            //If a & b are not contained in a Island
            else {
                if (a.color == IslandUtils.color.volcano || b.color == IslandUtils.color.volcano) {
                    HandleVolcanoCollision(a, b);  //Handle Volcano collision
                } else {
                    Island createdIsland = CreateIsland(a, b);
                    JoinTwoChunk(b, b_anchor, a, a_anchor, createdIsland);
                    createdIsland.islandColliders.AddCollision(a, Vector3.zero);
                    createdIsland.RecreateIslandChunkConnection();
                }
            }

            //All chunks in island change to the same color which is the most present
            if (!(a.color == IslandUtils.color.volcano || b.color == IslandUtils.color.volcano)) SimpleIslandConversion(a.parentIsland);
        }

        /// <summary> Handle Collisions if one of the chunk was a volcano</summary>
        /// <param name="a"> first chunk </param>
        /// <param name="b"> second chunk </param>
        private void HandleVolcanoCollision(IslandChunk a, IslandChunk b) {
            Island islandToBreak = null;
            IslandChunk chunkToPush= null;
            IslandChunk volcano = null;
            if (a.color == IslandUtils.color.volcano && b.color == IslandUtils.color.volcano) { 
                islandToBreak = null;
                chunkToPush = null;
            }
            else if (a.color == IslandUtils.color.volcano)
            {
                volcano = a;
                if (b.parentIsland == null) chunkToPush = b;
                else islandToBreak = b.parentIsland;
            }
            else {
                volcano = b;
                if (a.parentIsland == null) chunkToPush = a;
                else islandToBreak = a.parentIsland;
            }
            GameObject ParticleGO = (GameObject)Instantiate(AssembleParticlePrefab[2], volcano.transform.position + new Vector3(0, 0, -1.25f), Quaternion.identity);
            ParticleGO.transform.parent = volcano.transform;
       
            //TODO ANTOINE : DESTROY ISLAND that is not the volcano
            if(islandToBreak != null){
                //Break Island Here
            }
            if(chunkToPush != null) {
                PushChunk(chunkToPush, volcano);
            }

            Simoncouche.Chain.Hook[] hooks = GameObject.FindObjectsOfType<Simoncouche.Chain.Hook>();
            IslandChunk[] twoCollidedChunks = new IslandChunk[2] { a, b };
            foreach(Simoncouche.Chain.Hook hook in hooks) {
                hook.SendMessage("CheckConnectedVolcanoWithOtherIsland", twoCollidedChunks);
            }
        }

        /// <summary> Push the chunk away from the volcano and convert it to neutral </summary>
        /// <param name="chunkToPush">Chunk to push </param>
        /// <param name="volcano">Chunk to push </param>
        private void PushChunk(IslandChunk chunkToPush, IslandChunk volcano) {
            //Add Force
            Vector2 forceDirection = ((Vector2)(chunkToPush.transform.position - volcano.transform.position)).normalized;
            chunkToPush.gravityBody.Velocity += forceDirection * 5f;
            //Convert to neutral
            if(chunkToPush.color != IslandUtils.color.neutral) chunkToPush.ConvertChunkToAnotherColor(IslandUtils.color.neutral);
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
        private void RemoveIsland(Island island) {

            _island.Remove(island);
            if (island != null) {
                Destroy(island.gameObject);
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
            foreach (GameObject chain in GameObject.FindGameObjectsWithTag("Chain")) { //MUST SEND A MESSAGE TO CHAINS IN ORDER TO CHECK IF A HOOK IS ATTACHED TO A DESTROYED ISLAND
                chain.SendMessage("AttachedHookToIslandsUpdate");
            }

        }

        /// <summary> Remove Island from the list and call a destroyChunk for each chunk of the island </summary>
        /// <param name="island">island to destroy</param>
        public void DestroyIsland(Island island) {
            foreach (IslandChunk chunk in island.chunks) DestroyChunk(chunk);
            island.gravityBody.isDestroyed = true; //MUST SET THAT TO TRUE !!
            RemoveIsland(island);
            foreach (GameObject chain in GameObject.FindGameObjectsWithTag("Chain")) { //MUST SEND A MESSAGE TO CHAINS IN ORDER TO CHECK IF A HOOK IS ATTACHED TO A DESTROYED ISLAND
                chain.SendMessage("AttachedHookToIslandsUpdate");
            }
        }

        /// <summary>
        /// Check if an island was broken by the destruction/disassembling of one of his chunk
        /// </summary>
        /// <param name="island">The target island to check</param>
        public List<GravityBody> CheckIslandBroken(Island island) {
            if (island == null || island.chunks.Count <= 0) {
                DestroyIsland(island);
                return null;
            }

            //Make the player ungrab and then regrab only in 0.1seconds. 
            foreach (IslandChunk check in island.chunks) {
                PlayerGrab.UngrabBody(check.gravityBody, true, 0.1f);
            }

            List<GravityBody> everyPiece = new List<GravityBody>();
            everyPiece.Add(island.gravityBody);

            island.RecreateIslandChunkConnection();
            List<IslandChunk> originChunkList = island.chunks;
            List<IslandChunk> chunkIsland = new List<IslandChunk>();
            chunkIsland = CheckIslandBroken_Helper(island.chunks[0], chunkIsland);
            List<IslandChunk> chunkChecked = chunkIsland;
            
            //Reestablish collision to chunk not in main cluster 
            if (chunkChecked.Count != island.chunks.Count) {
                foreach (IslandChunk chunk in island.chunks) {
                    foreach (IslandChunk target in island.chunks) {
                        if (!(chunkChecked.Contains(target) && chunkChecked.Contains(chunk))) {
                            chunk.ChangeCollisionBetweenChunk(target, true);
                        }
                    }
                }
            }

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
                    everyPiece.Add(chunkIsland[0].gravityBody);
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
                    everyPiece.Add(island.gravityBody);
                }
            }
            island.RecreateIslandChunkConnection();


            //Update Conversion Status of the island
            island.UpdateConversionStatus();

            return everyPiece;
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
                if (current.parentIsland != null && islandChecked.Count == current.parentIsland.chunks.Count) break;

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
		public void TakeDamageHandler(IslandChunk chunk, int damage, Vector3 velocityGiven) {
			//If no damage
			if (damage < 1) {
				return;
			}
			//If the chunk has no connection
			if (chunk.parentIsland == null || chunk.connectedChunk == null || chunk.connectedChunk.Count == 0) {
				return;
			}
			//Make every part not mergeable for a time
			foreach (IslandChunk c in chunk.parentIsland.chunks) {
				//c.ResetMergeability(5f);
			}

			//Spawn Particle and play sound
			if (chunk.color == IslandUtils.color.red) Instantiate(DestroyParticle_SO, chunk.transform.position + new Vector3(0, 0, -1.25f), Quaternion.identity);
			if (chunk.color == IslandUtils.color.blue) Instantiate(DestroyParticle_CT, chunk.transform.position + new Vector3(0, 0, -1.25f), Quaternion.identity);

			Island islandLink = chunk.parentIsland;
			//Find median point for all chunks
			Vector3 medianIslandPos = FindMedianPos(islandLink.chunks);

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

			//Remove chunk from island
			foreach (IslandChunk c in islandRemoved) {
				islandLink.RemoveChunkToIsland(c);
			}

			//Remove connection (only need to remove the connection from one side, one chunk removes the connection from both)
			foreach (IslandChunk c in islandLink.chunks) {
				if (!islandRemoved.Contains(c)) {
					c.RemoveConnectedChunk(islandRemoved);
				}
			}

			//Divide Island and gives velocity to this piece
			if (islandRemoved.Count > 1) { //Multiple Chunk
				Island island = CreateIsland(islandRemoved[0], islandRemoved[1]);
				if (islandRemoved.Count >= 3) {
					for (int i = 2; i < islandRemoved.Count; i++) {
						island.AddChunkToIsland(islandRemoved[i]);
					}
				}
				island.gravityBody.Velocity = 100 * (-velocityGiven.normalized + DamageResultingVelocity(medianIslandPos, FindMedianPos(island.chunks))).normalized;

			} else {
				islandRemoved[0].gravityBody.Velocity = 100 * (-velocityGiven.normalized + DamageResultingVelocity(medianIslandPos, islandRemoved[0].transform.position)).normalized;
			}

			//Divide island and Set velocity for every pieces
			List<GravityBody> pieces = CheckIslandBroken(islandLink);
			foreach (GravityBody piece in pieces) {
				Island islandRef = piece.GetComponent<Island>();
				if (islandRef != null) {
					piece.Velocity = 20 * (velocityGiven.normalized + DamageResultingVelocity(medianIslandPos, FindMedianPos(islandRef.chunks))).normalized;
				} else {
					piece.Velocity = 20 * (velocityGiven.normalized + DamageResultingVelocity(medianIslandPos, piece.transform.position)).normalized;
				}
			}
		}

		/// <summary>
		/// Finds resulting velocity after a take Damage Hit
		/// </summary>
		/// <param name="center">The center point (or the origin of the damage)</param>
		/// <param name="target">The Target to give velocity to</param>
		/// <returns></returns>
		private Vector3 DamageResultingVelocity(Vector3 center, Vector3 target) {
			Debug.DrawLine(new Vector3(center.x, center.y, -10), new Vector3(target.x, target.y, -10), Color.red, 2f);
			Vector3 direction = target - center;
			direction.Normalize();
			Vector3 velocity = direction;
			return velocity;
		}

		/// <summary>
		/// Find the median position between multiple points
		/// </summary>
		/// <param name="positions"></param>
		/// <returns></returns>
		private Vector3 FindMedianPos(List<IslandChunk> positions) {
			Bounds box = new Bounds(positions[0].transform.position, Vector3.zero);
			foreach (IslandChunk pos in positions) {
				box.Encapsulate(pos.transform.position);
			}
			return box.center;
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
                case Islands.IslandUtils.color.neutral: type = 2; break;
                default: Debug.LogWarning("Island color is not not blue, red or neutral! "); break;
            }
            //Instantiate Particles FX
            if (type != -1) {
                GameObject ParticleGO = (GameObject)Instantiate(AssembleParticlePrefab[type], anchor.transform.position + new Vector3(0, 0, -1.25f), Quaternion.identity);
                ParticleGO.transform.parent = anchor.transform;
            }
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

        public int GetPlayerIslandCount(LevelManager.Player player) {
            int count = 0;

            foreach (IslandChunk chunk in _islandChunks) {
                if ((player == LevelManager.Player.sobek && chunk.color == IslandUtils.color.red) ||
                    (player == LevelManager.Player.cthulu && chunk.color == IslandUtils.color.blue)) {
                    count++;
                }
            }

            return count;
        }
    }
}
