using UnityEngine;
using System.Collections.Generic;

namespace Simoncouche.Islands {
	/// <summary>
	/// The global Island information, parent to Island Chunk
	/// </summary>
	public class Island : IslandChunk {



		/// <summary> The many part of the Island </summary>
		public List<IslandChunk> chunks { get; private set; }

        //Island's Components
        private CircleCollider2D _collider;
        private TrailRenderer _trailRenderer;


		void Awake() {
			chunks = new List<IslandChunk>();
            _collider = GetComponent<CircleCollider2D>();
			base.Awake();
            _trailRenderer = GetComponent<TrailRenderer>();
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
		/// Changes the velocity of the entire Island based on new fragment
		/// </summary>
		/// <param name="chunk"></param>
        private void ChangeGravityBodyWhenMerging(IslandChunk chunk) {
            gravityBody.LinearDrag = chunk.gravityBody.LinearDrag;
			//Merge weight
			gravityBody.Velocity = (gravityBody.Velocity * weight + chunk.gravityBody.Velocity * chunk.weight) / (weight + chunk.weight);
			weight = weight + chunk.weight;

            _collider.radius += 0.25f; //TODO : Get Collider Position and Radius based on island chunks. This is only placeholder !
			gravityBody.Weight += chunk.gravityBody.Weight;
            _trailRenderer.startWidth += 0.25f;
            _trailRenderer.time += 0.25f;

            //deactivate the gravitybody of the chunk
            chunk.gravityBody.DeactivateGravityBody();
		}
	}
}
