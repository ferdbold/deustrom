using UnityEngine;
using System.Collections.Generic;

namespace Simoncouche.Islands {
	/// <summary>
	/// Every Island data
	/// </summary>
	public class IslandManager : MonoBehaviour {

		/// <summary>
		/// A list of every Island currently in play
		/// </summary>
		private List<Island> _island = new List<Island>();

		/// <summary>
		/// The Island prefab reference
		/// </summary>
		[SerializeField]  
		[Tooltip("Island Object Prefab Reference")]
		private GameObject _islandComponent;

		/// <summary>
		/// Creates a Island from 2 chunk, Will not work for multiple piece of the same letter in one scene
		/// </summary>
		/// <param name="a">the first chunk</param>
		/// <param name="b">the second chunk</param>
		public void HandleChunkCollision(IslandChunk a, IslandChunk b) {
			Island a_IslandLink = ChunkContainedInIsland(a);
			Island b_IslandLink = ChunkContainedInIsland(b);
			
			//If both are contained in Island
			if (a_IslandLink != null && b_IslandLink != null && a_IslandLink != b_IslandLink) {
				List<IslandChunk> chunks = b_IslandLink.chunks;
				foreach(IslandChunk chunk in chunks) {
					a_IslandLink.AddChunkToIsland(chunk, GetMergingPoint(b.transform.position, a.transform.position), a.transform.rotation.eulerAngles);
				}
				RemoveIsland(b_IslandLink);
			} 

			//If a is contained in a Island
			else if (a_IslandLink != null) {
				a_IslandLink.AddChunkToIsland(b, GetMergingPoint(b.transform.position, a.transform.position), a.transform.rotation.eulerAngles);
			} 
			
			//If b is contained in a Island
			else if (b_IslandLink != null) {
				b_IslandLink.AddChunkToIsland(a, GetMergingPoint(a.transform.position, b.transform.position), b.transform.rotation.eulerAngles);
			} 
			
			//If a & b are not contained in a Island
			else {
				CreateIsland(a, b);
			}
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
		private void CreateIsland(IslandChunk a, IslandChunk b) {
			GameObject Island = Instantiate(_islandComponent, a.transform.position, a.transform.rotation) as GameObject;
			Island.name = "Island";

			Island.GetComponent<Island>().AddChunkToIsland(b, GetMergingPoint(b.transform.position, a.transform.position), a.transform.rotation.eulerAngles);

			_island.Add(Island.GetComponent<Island>());
		}

		/// <summary>
		/// Removes the Island from the list then destroy it
		/// </summary>
		/// <param name="Island">The Island that need to be removed</param>
		private void RemoveIsland(Island Island) {
			_island.Remove(Island);
			Destroy(Island.gameObject);
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
	}
}
