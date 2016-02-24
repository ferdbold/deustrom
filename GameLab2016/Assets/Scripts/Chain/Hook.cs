using UnityEngine;
using Simoncouche.Islands;

namespace Simoncouche.Chain {

	/// <summary>
	/// A Hook is an ending element of a Chain that can snap itself either to a character or an IslandAnchorPoint.
	/// </summary>
	[RequireComponent(typeof(DistanceJoint2D))]
	[RequireComponent(typeof(FixedJoint2D))]
	public class Hook : MonoBehaviour {

		[Tooltip("The mass of this hook once attached to an island")]
		[SerializeField]
		private float ATTACHED_MASS = 10f;

		/// <summary>
		/// Self-reference to the hook prefab for factory purposes
		/// </summary>
		private static GameObject _hookPrefab;

		/// <summary>
		/// The chain this hook is part of
		/// </summary>
		private Chain _chain;

		/// <summary>
		/// The ChainSection linked to this hook
		/// </summary>
		private ChainSection _nextChain;

		/// <summary>
		/// Whether this hook is currently attached to a target
		/// </summary>
		private bool _attachedToTarget = false;

		// COMPONENTS

		public FixedJoint2D targetJoint { get; private set; }

		/// <summary>
		/// The joint linking this hook to the other edge of the chain,
		/// (which will be the thrower or the second Hook of a chain). 
		/// This joint is only used by the beginning Hook of a Chain.
		/// </summary>
		public DistanceJoint2D chainJoint { get; private set; }

		public new Rigidbody2D rigidbody { get; private set; }

		/// <summary>
		/// Spawn a new hook inside a chain
		/// </summary>
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

			hook.transform.parent = chain.transform;
			hook.name = (isBeginningHook) ? "BeginningHook" : "EndingHook";
			hook.chainJoint.enabled = isBeginningHook;
			hook.SetChain(chain);

			return hook;
		}

		public void Awake() {
			this.chainJoint = GetComponent<DistanceJoint2D>();
			this.targetJoint = GetComponent<FixedJoint2D>();
			this.rigidbody = GetComponent<Rigidbody2D>();
		}

		public void Start() {
			//_nextChain = ChainSection.Create(transform.position, _chain, _rigidbody);
			this.rigidbody.AddForce(transform.rotation * new Vector2(_chain.initialForce, 0));
		}

		public void OnTriggerEnter2D(Collider2D coll) {
			if (!_attachedToTarget) {
				IslandAnchorPoints anchorPoint = coll.gameObject.GetComponent<IslandAnchorPoints>();

				if (anchorPoint != null) {
					this.AttachTo(anchorPoint);
				}
			}
		}
        //TODO: Destroy the chain when the distance is higher than a certain value
        void Update() {
            if(Vector2.Distance(this.rigidbody.position, _chain.thrower.transform.position) > this.chainJoint.distance) {
                this.rigidbody.velocity = Vector2.zero;
            }
				
            //if (_joint.connectedBody == null) ClampDistanceWithPlayerPos(_chain.thrower.transform, _maximumDistanceBetweenPlayer);
        }

        /// <summary>
        /// Restrain the closest link of a chain with a maxDistance from the thrower's position
        /// </summary>
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

		public void AttachTo(IslandAnchorPoints anchor) {
			this.targetJoint.enabled = true;
			this.targetJoint.connectedBody = anchor.GetIslandChunk().GetComponent<Rigidbody2D>();
			this.rigidbody.velocity = Vector2.zero;
			this.rigidbody.mass = this.ATTACHED_MASS;
		}
		
		public void SetChain(Chain value) {
			_chain = value;
		}
	}
}
