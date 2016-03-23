using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Simoncouche.Islands {

    /// <summary>
    /// RandomizeIslandVisual randomizes the island's model so it looks different from the other chunks of island
    /// </summary>
    public class RandomizeIslandVisual : MonoBehaviour {

        [Header("Cthulhu :")]
        [SerializeField]
        [Tooltip("List of island base. Base of the island will be randomized on play.")]
        private List<GameObject> ISLANDS_BASE_CTHULHU;

        [SerializeField]
        [Tooltip("List of island props configurations. Props will be randomized on play.")]
        private List<GameObject> ISLANDS_PROPS_CTHULHU;

        [Header("Sobek :")]
        [SerializeField]
        [Tooltip("List of island base. Base of the island will be randomized on play.")]
        private List<GameObject> ISLANDS_BASE_SOBEK;

        [SerializeField]
        [Tooltip("List of island props configurations. Props will be randomized on play.")]
        private List<GameObject> ISLANDS_PROPS_SOBEK;

        [Header("Neutral :")]
        [SerializeField]
        [Tooltip("List of island base. Base of the island will be randomized on play.")]
        private List<GameObject> ISLANDS_BASE_NEUTRAL;

        [SerializeField]
        [Tooltip("List of island props configurations. Props will be randomized on play.")]
        private List<GameObject> ISLANDS_PROPS_NEUTRAL;

        [Header("Other :")]
        [Tooltip("Minimum scale for each values of transform")]
        [SerializeField]
        private Vector3 VARIATION_SCALE_MIN = new Vector3(0.95f, 0.95f, 0.975f);

        [Tooltip("Maximum scale for each values of transform")]
        [SerializeField]
        private Vector3 VARIATION_SCALE_MAX = new Vector3(1.05f, 1.05f, 1.025f);

        [Tooltip("Minimum rotation for each values of transform")]
        [SerializeField]
        private Vector3 VARIATION_ROTATION_MIN = new Vector3(0f, 0f, 0f);

        [Tooltip("Minimum rotation for each values of transform")]
        [SerializeField]
        private Vector3 VARIATION_ROTATION_MAX = new Vector3(0f, 0f, 360f);

        /// <summary> Model of the gameobject </summary>
        private Transform _islandModel;
        private GameObject _CT_island;
        private GameObject _SO_island;
        private GameObject _N_island;

        // Get Components
        void Awake() {
            _islandModel = transform.Find("Model");
            //This should be done on awake since other scripts expect _CT_island and _SO_island to be set up on Start()
            if (_islandModel != null) {
                RandomizeIsland();
            } else Debug.Log("Model of " + gameObject.name + " was not found !");
        }


        /// <summary> Toggles the island visual to show only the visual of the sent color </summary>
        /// <param name="newColor"> Wanted color. red is Sobek, blue is Cthulhu. </param>
        public void SetIslandColorVisual(IslandUtils.color newColor) {
            switch (newColor) {
                case IslandUtils.color.red:
                    _CT_island.SetActive(false);
                    _SO_island.SetActive(true);
                    _N_island.SetActive(false);
                    break;
                case IslandUtils.color.blue:
                    _CT_island.SetActive(true);
                    _SO_island.SetActive(false);
                    _N_island.SetActive(false);
                    break;
                case IslandUtils.color.neutral:
                    _CT_island.SetActive(false);
                    _SO_island.SetActive(false);
                    _N_island.SetActive(true);                
                    break;
                default:
                    Debug.LogWarning("Other Color received in SetIslandColorVisual...");
                    break;
            }
        }

        /// <summary> Randomize the island base and props </summary>
        private void RandomizeIsland() {
            //remove previously created objects
            RemoveAllModelChildren();
            //create basic empty islansd for both sides
            //Sobek
            _SO_island = new GameObject();
            _SO_island.transform.parent = _islandModel;
            _SO_island.name = "SOBEK_ISLAND";
            _SO_island.transform.localPosition = Vector3.zero;
            _SO_island.transform.localScale = Vector3.one;
            //Cthulhu
            _CT_island = new GameObject();
            _CT_island.name = "CTHULHU_ISLAND";
            _CT_island.transform.parent = _islandModel;
            _CT_island.transform.localPosition = Vector3.zero;
            _CT_island.transform.localScale = Vector3.one;
            //Neutral
            _N_island = new GameObject();
            _N_island.name = "NEUTRAL_ISLAND";
            _N_island.transform.parent = _islandModel;
            _N_island.transform.localPosition = Vector3.zero;
            _N_island.transform.localScale = Vector3.one;
            //Create random islands and populate them
            RandomizeIslandBase();
            ApplyVariation();
        }

        /// <summary> Remove all existing children of island model</summary>
        private void RemoveAllModelChildren() {
            Transform[] islandTransforms = _islandModel.GetComponentsInChildren<Transform>();
            foreach (Transform t in islandTransforms) {
                if (t != _islandModel.transform) Destroy(t.gameObject);
            }
        }

        /// <summary> Randomize the island base and call RandomizeIslandProps to populate them </summary>
        private void RandomizeIslandBase() {
            //Create Sobek Island
            int randNum = Random.Range(0, ISLANDS_BASE_SOBEK.Count);
            GameObject soBase = (GameObject)Instantiate(ISLANDS_BASE_SOBEK[randNum], _SO_island.transform.position, Quaternion.identity);
            soBase.transform.parent = _SO_island.transform;
            soBase.transform.localPosition = Vector3.zero;
            soBase.transform.localScale = Vector3.one;
            //Create cthulhu Island
            randNum = Random.Range(0, ISLANDS_BASE_CTHULHU.Count);
            GameObject ctBase = (GameObject)Instantiate(ISLANDS_BASE_CTHULHU[randNum], _CT_island.transform.position, Quaternion.identity);
            ctBase.transform.parent = _CT_island.transform;
            ctBase.transform.localPosition = Vector3.zero;
            ctBase.transform.localScale = Vector3.one;
            //Create cthulhu Island
            randNum = Random.Range(0, ISLANDS_BASE_NEUTRAL.Count);
            GameObject nBase = (GameObject)Instantiate(ISLANDS_BASE_NEUTRAL[randNum], _N_island.transform.position, Quaternion.identity);
            nBase.transform.parent = _N_island.transform;
            nBase.transform.localPosition = Vector3.zero;
            nBase.transform.localScale = Vector3.one;
            //Add Props to islands
            RandomizeIslandProps(soBase, ctBase, nBase);
        }

        /// <summary> Populate the island with props </summary>
        /// <param name="soBase"> Refrence so sobek Base island </param>
        /// <param name="ctBase"> Refrence so cthulhu Base island</param>
        private void RandomizeIslandProps(GameObject soBase, GameObject ctBase, GameObject nBase) {
            //Create Sobek Island
            int randNum = Random.Range(0, ISLANDS_PROPS_SOBEK.Count);
            GameObject soProps = (GameObject)Instantiate(ISLANDS_PROPS_SOBEK[randNum], _islandModel.transform.position, Quaternion.identity);
            soProps.transform.parent = soBase.transform;
            soProps.transform.localPosition = Vector3.zero;
            soProps.transform.localScale = Vector3.one;
            //Create cthulhu Island
            randNum = Random.Range(0, ISLANDS_PROPS_CTHULHU.Count);
            GameObject ctProps = (GameObject)Instantiate(ISLANDS_PROPS_CTHULHU[randNum], _islandModel.transform.position, Quaternion.identity);
            ctProps.transform.parent = ctBase.transform;
            ctProps.transform.localPosition = Vector3.zero;
            ctProps.transform.localScale = Vector3.one;
            //Create neutral Island
            randNum = Random.Range(0, ISLANDS_PROPS_NEUTRAL.Count);
            GameObject nProps = (GameObject)Instantiate(ISLANDS_PROPS_NEUTRAL[randNum], _islandModel.transform.position, Quaternion.identity);
            nProps.transform.parent = nBase.transform;
            nProps.transform.localPosition = Vector3.zero;
            nProps.transform.localScale = Vector3.one;
        }

        /// <summary> Modify the visuals of the island's model </summary>
        private void ApplyVariation() {
            //Change Scale
            _islandModel.localScale = new Vector3(Random.Range(VARIATION_SCALE_MIN[0], VARIATION_SCALE_MAX[0]),
                                                    Random.Range(VARIATION_SCALE_MIN[1], VARIATION_SCALE_MAX[1]),
                                                    Random.Range(VARIATION_SCALE_MIN[2], VARIATION_SCALE_MAX[2]));
            //Change Rotation
            _islandModel.localRotation = Quaternion.Euler(Random.Range(VARIATION_ROTATION_MIN[0], VARIATION_ROTATION_MAX[0]),
                                                            Random.Range(VARIATION_ROTATION_MIN[1], VARIATION_ROTATION_MAX[1]),
                                                            Random.Range(VARIATION_ROTATION_MIN[2], VARIATION_ROTATION_MAX[2]));

        }
    }
}