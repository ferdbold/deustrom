﻿using UnityEngine;
using Simoncouche.Islands;

namespace Simoncouche.Chain {

	/// <summary>
	/// A Hook is an ending element of a Chain that can snap itself either to a character or an IslandAnchorPoint.
	/// </summary>
	[RequireComponent(typeof(FixedJoint2D))]
	public class Hook : MonoBehaviour {

		/// <summary>
		/// Self-reference to the hook prefab for factory purposes
		/// </summary>
		private static GameObject _hookPrefab;

        [Tooltip("This is the maximum distance between the hook and the player")]
        [SerializeField]
        private float _maxDistanceBetweenPlayerAndHook=10f;

		/// <summary>
		/// The chain this hook is part of
		/// </summary>
		private Chain _chain;

		/// <summary>
		/// The ChainSection linked to this hook
		/// </summary>
		private ChainSection _nextChain;

		// COMPONENTS

		private FixedJoint2D _joint;
		public FixedJoint2D joint { get { return _joint; } }

		private Rigidbody2D _rigidbody;

		/// <summary>
		/// Spawn a new hook inside a chain
		/// </summary>
		/// <param name="chain">The parent chain</param>
		public static Hook Create(Chain chain) {
			if (_hookPrefab == null) {
				_hookPrefab = Resources.Load("Chain/Hook") as GameObject;
			}

			Hook hook = ((GameObject)Instantiate(
				_hookPrefab, 
				chain.thrower.transform.position, 
				Quaternion.Euler(0, 0, chain.thrower.aimController.aimOrientation)
			)).GetComponent<Hook>();

			hook.transform.parent = chain.transform;
			hook.SetChain(chain);

			return hook;
		}

		public void Awake() {
			_joint = GetComponent<FixedJoint2D>();
			_rigidbody = GetComponent<Rigidbody2D>();
		}

		public void Start() {
			_nextChain = ChainSection.Create(transform.position, _chain, _rigidbody);
			_rigidbody.AddForce(transform.rotation * new Vector2(_chain.initialForce, 0));
		}

		public void OnTriggerEnter2D(Collider2D coll) {
			if (_joint.connectedBody == null) {
				if (coll.gameObject.GetComponent<IslandAnchorPoints>() != null) {
					_joint.enabled = true;
					_joint.connectedBody = coll.GetComponent<Rigidbody2D>();
				}
			}
		}
        //TODO: Destroy the chain when the distance is higher than a certain value
        void Update() {
            if(Vector2.Distance(this._rigidbody.position, _chain.thrower.transform.position) > _maxDistanceBetweenPlayerAndHook) {
                _rigidbody.velocity = Vector2.zero;
            }
            //if (_joint.connectedBody == null) ClampDistanceWithPlayerPos(_chain.thrower.transform, _maximumDistanceBetweenPlayer);
        }

		public void SetChain(Chain value) {
			_chain = value;
		}

        /// <summary>
        /// Restrain the closest link of a chain with a maxDistance from the thrower's position
        /// </summary>
        /// <param name="throwerPosition"></param>
        /// <param name="maxDistance"></param>
        private void ClampDistanceWithPlayerPos(Transform throwerPosition, float maxDistance) {
            Debug.Log("ON CLAMP LE HOOK");
            float currentDistance = Vector3.Distance(_rigidbody.position, throwerPosition.position);
            if (currentDistance > maxDistance) {
                Vector2 vect =  throwerPosition.position - (Vector3) _rigidbody.position;
                vect = vect.normalized;
                vect *= (currentDistance - maxDistance);
                _rigidbody.position += vect;
            }
        }


	}
}
