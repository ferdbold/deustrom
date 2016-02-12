﻿using UnityEngine;
using System.Collections;

namespace Simoncouche.Islands {
	/// <summary>
	/// Useful function and enums for Islands
	/// </summary>
	public class IslandUtils {
		/// <summary>
		/// Possible color of Islands
		/// </summary>
		public enum color {
			red,
			blue,
			green
		}

		/*/// <summary>
		/// Island Chunk possible Letters
		/// </summary>
		public enum ChunkLetter { A, B, C, D, E, F, G, H, I, J }*/

		/// <summary>
		/// Creates a string from the Island information of a Island
		/// </summary>
		/// <param name="c"> The color of the Island </param>
		/// <param name="number"> It's numerical value </param>
		/// <returns> The info in string form </returns>
		public static string CreateStringWithIslandInfo(color c, int number) {
			return c.ToString("g") + " " + number.ToString();
		}

		/// <summary>
		/// Check if the 2 island are the same object
		/// </summary>
		/// <param name="a">An Island</param>
		/// <param name="b">An Island</param>
		/// <returns>TRUE if the island are the same, FALSE if their is no island or </returns>
		public static bool CheckIfOnSameIsland(Island a, Island b) {
			return a != null && b != null && a.gameObject != b.gameObject;
		}
	}
}
