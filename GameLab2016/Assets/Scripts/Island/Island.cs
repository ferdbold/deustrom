﻿using UnityEngine;
using System.Collections.Generic;

namespace Simoncouche.Islands {
	/// <summary>
	/// The global Island information, parent to Island Chunk
	/// </summary>
	public class Island : MonoBehaviour {
		
		private int _weight = 1;
		public int weight {
			get { return _weight; }
			protected set { _weight = value; }
		}

		/// <summary> The many part of the Island </summary>
		public List<IslandChunk> chunks { get; private set; }

        //Island's Components
        private CircleCollider2D _collider;
		private GravityBody _gravityBody;

		private void Awake() {
			chunks = new List<IslandChunk>();
			_collider = GetComponent<CircleCollider2D>();
			_gravityBody = GetComponent<GravityBody>();
		}

		/// <summary>
		/// Returns if this Island has the target chunk
		/// </summary>
		/// <param name="chunk">Target chunk</param>
		/// <returns>True if it contains the chunk</returns>
		public bool IslandContainsChunk(IslandChunk chunk) {
			return chunks.Contains(chunk);
		}

		/// <summary>
		/// Add a chunk to this Island, used when a chunk collides with a Island
		/// </summary>
		/// <param name="chunk">Reference to the collinding chunk</param>
		/// <param name="pos">The position of the chunk</param>
		/// <param name="rot">The rotation of the chunk</param>
		public void AddChunkToIsland(IslandChunk chunk, Vector3 pos, Vector3 rot) {
			if (!chunks.Contains(chunk)) {
				chunk.transform.SetParent(transform);
				chunks.Add(chunk);
				/*chunk.transform.localPosition = pos;
				chunk.transform.localRotation = Quaternion.Euler(rot);*/
				ChangeGravityBodyWhenMerging(chunk);
			}
        }

        /// <summary>
        /// Remove a chunk of this island.
        /// </summary>
        /// <param name="chunk">Reference of the chunk to remove</param>
        private void RemoveChunkToIsland(IslandChunk chunk) {
            //TODO : Implement this function

            //Recenter island middle
            CenterIslandRoot();
        }

        /// <summary>
        /// Changes the velocity of the entire Island based on new fragment
        /// </summary>
        /// <param name="chunk"></param>
        private void ChangeGravityBodyWhenMerging(IslandChunk chunk) {
            _gravityBody.LinearDrag = chunk.gravityBody.LinearDrag;
			//Merge weight
			_gravityBody.Velocity = (_gravityBody.Velocity * weight + chunk.gravityBody.Velocity * chunk.weight) / (weight + chunk.weight);
			weight = weight + chunk.weight;

            _collider.radius += 0.25f; //TODO : Get Collider Position and Radius based on island chunks. This is only placeholder !
			_gravityBody.Weight += chunk.gravityBody.Weight;

            //deactivate the gravitybody of the chunk
            chunk.gravityBody.DeactivateGravityBody();
		}

        /// <summary>
        /// Centers the root of the island based on its existing chunk
        /// </summary>
        public void CenterIslandRoot() {
            
            //Calculate median position of the island's chunks
            Vector3 medianPosition = Vector3.zero;
            for(int i = 0; i < chunks.Count; i++ ) {
                medianPosition += chunks[i].transform.localPosition;
            }
            medianPosition /= chunks.Count;
            Debug.Log("median position " + medianPosition);

            //Modifiy Island chunks and island positions
            for (int i = 0; i < chunks.Count; i++) {
                chunks[i].transform.localPosition -= medianPosition;
            }
            transform.position = medianPosition;

        }

    }
}
