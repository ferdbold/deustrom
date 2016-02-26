using UnityEngine;
using Simoncouche.Islands;

namespace Simoncouche.Chain {

	/// A Hook is an ending element of a Chain that can snap itself either to a character or an IslandAnchorPoint.
	[RequireComponent(typeof(DistanceJoint2D))]
	[RequireComponent(typeof(FixedJoint2D))]
	[RequireComponent(typeof(HingeJoint2D))]
	public class Hook : MonoBehaviour {

		[Tooltip("The mass of this hook once attached to an island")]
		[SerializeField]
		private float ATTACHED_MASS = 10f;

		/// Self-reference to the hook prefab for factory purposes
		private static GameObject _hookPrefab;

		/// The chain this hook is part of
		public Chain chain { get; private set; }

		/// The ChainSection linked to this hook
		private ChainSection _nextChain;

		/// Whether this hook is currently attached to a target
		private bool _attachedToTarget = false;

		// COMPONENTS

		/// The joint linking this hook to its target (an island)
		public FixedJoint2D targetJoint { get; private set; }

		/// The joint linking this hook to the other edge of the chain,
		/// (which will be the thrower or the second Hook of a chain). 
		/// Only used in the beginning hook.
		public DistanceJoint2D chainJoint { get; private set; }

		/// The joint used to hang visual chain sections to this hook.
		/// Only used in the beginning hook to hang the first chain section.
		public HingeJoint2D visualChainJoint { get; private set; }

		public new Rigidbody2D rigidbody { get; private set; }

		/// <summary>Spawn a new hook inside a chain</summary>
		/// <param name="chain">The parent chain</param>
		/// <param name="isBeginningHook">Is this hook the beginning hook of a chain</param> 
		public static Hook Create(Chain chain, bool isBeginningHook) {
			if (_hookPrefab == null) {
				_hookPrefab = Resources.Load("Chain/Hook") as GameObject;
			}

			Hook hook = ((GameObject)Instantiate(
				_hookPrefab, 
				chain.thrower.transform.position, 
				Quaternion.Euler(0, 0, chain.thrower.aimController.aimOrientation)
			)).GetComponent<Hook>();

			hook.name = (isBeginningHook) ? "BeginningHook" : "EndingHook";
			hook.transform.parent = chain.transform;
			hook.chain = chain;

			hook.chainJoint.enabled = isBeginningHook;
			hook.visualChainJoint.enabled = isBeginningHook;

			if (isBeginningHook) {
				// Hook the beginning hook's chain joint to the player to enable physics tension
				hook.chainJoint.connectedBody = chain.thrower.rigidbody;

				// Hook the beginning hook's visual joint to the player for the chain to initialize
				hook.visualChainJoint.connectedBody = chain.thrower.rigidbody;
			}

			return hook;
		}

		public void Awake() {
			this.chainJoint = GetComponent<DistanceJoint2D>();
			this.targetJoint = GetComponent<FixedJoint2D>();
			this.visualChainJoint = GetComponent<HingeJoint2D>();
			this.rigidbody = GetComponent<Rigidbody2D>();
		}

		public void Start() {
			//_nextChain = ChainSection.Create(transform.position, _chain, _rigidbody);
			this.rigidbody.AddForce(transform.rotation * new Vector2(chain.initialForce, 0));
		}

		public void OnTriggerEnter2D(Collider2D coll) {
			if (!_attachedToTarget) {
				IslandAnchorPoints anchorPoint = coll.gameObject.GetComponent<IslandAnchorPoints>();

				if (anchorPoint != null) {
					this.AttachToIsland(anchorPoint);
				}
			}
		}
        // TODO: Destroy the chain when the distance is higher than a certain value
        void Update() {
            /*if(Vector2.Distance(this.rigidbody.position, chain.thrower.transform.position) > this.chainJoint.distance) {
                this.rigidbody.velocity = Vector2.zero;
            }*/
				
            //if (_joint.connectedBody == null) ClampDistanceWithPlayerPos(_chain.thrower.transform, _maximumDistanceBetweenPlayer);
        }

		/// <summary>Spawns the first chain section and attach to it</summary>
		/// <returns>The chain section</returns>
		public ChainSection SpawnChainSection() {
			Vector3 chainSectionPosition = transform.position - transform.right * transform.localScale.x;

			ChainSection chainSection = ChainSection.Create(
				chainSectionPosition, 
				this.rigidbody.transform.rotation, 
				this.chain, 
				this.gameObject, 
				Quaternion.identity
			);

			// Connect the new chain section to the end of the chain
			chainSection.joint.connectedBody = this.visualChainJoint.connectedBody;

			// Attach the visual joint to this first chain section
			this.visualChainJoint.connectedBody = chainSection.rigidbody;

			return chainSection;
		}

        /// <summary>Restrain the closest link of a chain with a maxDistance from the thrower's position</summary>
        /// <param name="throwerPosition"></param>
        /// <param name="maxDistance"></param>
        private void ClampDistanceWithPlayerPos(Transform throwerPosition, float maxDistance) {
            Debug.Log("ON CLAMP LE HOOK");
            float currentDistance = Vector3.Distance(this.rigidbody.position, throwerPosition.position);
            if (currentDistance > maxDistance) {
                Vector2 vect =  throwerPosition.position - (Vector3) this.rigidbody.position;
                vect = vect.normalized;
                vect *= (currentDistance - maxDistance);
                this.rigidbody.position += vect;
            }
        }

		/// <summary>Attach this Hook to the Island associated to an anchor point</summary>
		/// <param name="anchor">The anchor point</param>
		public void AttachToIsland(IslandAnchorPoints anchor) {
			this.targetJoint.enabled = true;
			this.targetJoint.connectedBody = anchor.GetIslandChunk().GetComponent<Rigidbody2D>();
			this.rigidbody.velocity = Vector2.zero;
			this.rigidbody.mass = this.ATTACHED_MASS;
		}

		/// <summary>
		/// Attach this section's joint to another rigidbody. 
		/// This is useful for deleting sections of a chain
		/// </summary>
		/// <param name="rb">The new rigidbody to attach to</param>
		public void AttachVisualJointTo(Rigidbody2D rb) {
			this.visualChainJoint.connectedBody = rb;
		}
	}
}
