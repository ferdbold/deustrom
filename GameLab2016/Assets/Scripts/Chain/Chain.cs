using UnityEngine;
using System.Collections;

namespace Simoncouche.Chain {

    /// <summary>
    /// A Chain is a group of ChainSection game objects linked together, with one Hook object at either end.
    /// </summary>
    public class Chain : MonoBehaviour {

        [Tooltip("This is the maximum distance between the hook and the player")]
        [SerializeField]
        private float _maximumDistanceBetweenPlayer = 5.0f;

        /// <summary>
        /// Self-reference to the chain prefab for factory purposes
        /// </summary>
        private static GameObject _chainPrefab;

        [Tooltip("Reference to the Hook prefab")]
        [SerializeField]
        private Hook _hookPrefab;

        private Hook _beginningHook;
        private Hook _endingHook;

        private ChainSection _endingLinkBeginningHook=null;
        private ChainSection _endingLinkEndingHook = null;

        /// <summary>
        /// The current number of links in the biginning hook
        /// </summary>
        private int _currentLinkNumberBeginningHook;

        /// <summary>
        /// The current number of links in the biginning hook
        /// </summary>
        private int _currentLinkNumberEndingHook;

        /// <summary>
        /// The current aim orientation as set by the right analog input
        /// </summary>
        [Tooltip("Maximum number of links per chain")]
        [SerializeField]
        private int _maximumLinksPerChain = 30;

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

        public void Update() {
            if (Vector3.Distance(transform.position, thrower.joint.connectedBody.position) > thrower.spawnChainDistanceThreshold){
                if (_currentLinkNumberBeginningHook < _maximumLinksPerChain) {
                    thrower.joint.connectedBody.GetComponent<ChainSection>().SpawnNewSection();
                    _endingLinkBeginningHook = thrower.joint.connectedBody.GetComponent<ChainSection>();
                    _currentLinkNumberBeginningHook++;
                }
                //ClampDistanceWithPlayerPos(thrower.transform, _maximumDistanceBetweenPlayer, _endingLinkBeginningHook); //We clamp the distance of the closet link to the player to a maximum distance from the player
            }

            if (Vector3.Distance(transform.position, thrower.joint.connectedBody.position) > thrower.spawnChainDistanceThreshold){
                if (_currentLinkNumberEndingHook < _maximumLinksPerChain)
                {
                    thrower.joint.connectedBody.GetComponent<ChainSection>().SpawnNewSection();
                    _endingLinkEndingHook = thrower.joint.connectedBody.GetComponent<ChainSection>();
                    _currentLinkNumberEndingHook++;
                }
            }
            if(_beginningHook != null) {
                ClampDistanceWithPlayerPos(thrower.transform, _maximumDistanceBetweenPlayer, _endingLinkBeginningHook); //We clamp the distance of the closet link to the player to a maximum distance from the player
            }

            if (_endingHook != null){
                ClampDistanceWithPlayerPos(thrower.transform, _maximumDistanceBetweenPlayer, _endingLinkEndingHook); //We clamp the distance of the closet link to the player to a maximum distance from the player
            }

            if (Vector2.Distance(_beginningHook.GetComponent<Rigidbody2D>().position,thrower.GetComponent<Rigidbody2D>().position) > 15f)
            {
                _beginningHook.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                while (_beginningHook.joint.connectedBody != null)
                {
                    Rigidbody2D tempBody = _beginningHook.joint.connectedBody;

                }
            }

            if (Vector2.Distance(_endingHook.GetComponent<Rigidbody2D>().position, thrower.GetComponent<Rigidbody2D>().position) > 15f)
            {

            }
        }


        /// <summary>
        /// Restrain the closest link of a chain with a maxDistance from the thrower's position
        /// </summary>
        /// <param name="throwerPosition"></param>
        /// <param name="maxDistance"></param>
        /// <param name="chainSection"></param>
        private void ClampDistanceWithPlayerPos(Transform throwerPosition, float maxDistance, ChainSection chainSection)
        {
            float currentDistance = Vector3.Distance(chainSection.GetComponent<Rigidbody2D>().position, throwerPosition.position);
            if (currentDistance > maxDistance)
            {
                Vector2 vect = throwerPosition.position - (Vector3) chainSection.GetComponent<Rigidbody2D>().position;
                vect = vect.normalized;
                vect *= (currentDistance - maxDistance);
                chainSection.GetComponent<Rigidbody2D>().position +=  vect;
            }
        }

        public void CreateSecondHook()
        {
            _endingHook = Hook.Create(this);
        }
    }
}
