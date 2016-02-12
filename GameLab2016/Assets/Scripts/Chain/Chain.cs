using UnityEngine;
using System.Collections;

namespace Simoncouche.Chain {

	/// <summary>
	/// A Chain is a group of ChainSection game objects linked together, with one Hook object at either end.
	/// </summary>
	public class Chain : MonoBehaviour {

		/// <summary>
		/// Self-reference to the chain prefab for factory purposes
		/// </summary>
		private static GameObject _chainPrefab;

		[Tooltip("Reference to the Hook prefab")]
		[SerializeField]
		private Hook _hookPrefab;

		private Hook _beginningHook;
		private Hook _endingHook;

		public HookThrower thrower { get; set; }
		public float initialForce { get; set; }

		/// <summary>
		/// Spawn a new chain in the scene
		/// </summary>
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

		public void Start() {
			_beginningHook = Hook.Create(this);
		}
	}
}
