using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// RandomizeIslandVisual randomizes the island's model so it looks different from the other chunks of island
/// </summary>
public class RandomizeIslandVisual : MonoBehaviour {

    [Header("Cthulhu :")]
    [SerializeField] [Tooltip("List of island base. Base of the island will be randomized on play.")]
    private List<GameObject> ISLANDS_BASE_CTHULHU;

    [SerializeField] [Tooltip("List of island props configurations. Props will be randomized on play.")]
    private List<GameObject> ISLANDS_PROPS_CTHULHU;

    [Header("Sobek :")]
    [SerializeField] [Tooltip("List of island base. Base of the island will be randomized on play.")]
    private List<GameObject> ISLANDS_BASE_SOBEK;

    [SerializeField] [Tooltip("List of island props configurations. Props will be randomized on play.")]
    private List<GameObject> ISLANDS_PROPS_SOBEK;

    [Header("Other :")]
    [Tooltip("Minimum scale for each values of transform")][SerializeField]
    private Vector3 VARIATION_SCALE_MIN = new Vector3(0.95f, 0.95f, 0.975f);

    [Tooltip("Maximum scale for each values of transform")] [SerializeField] 
    private Vector3 VARIATION_SCALE_MAX = new Vector3(1.05f, 1.05f, 1.025f);

    [Tooltip("Minimum rotation for each values of transform")] [SerializeField] 
    private Vector3 VARIATION_ROTATION_MIN = new Vector3(0f, 0f, 0f);

    [Tooltip("Minimum rotation for each values of transform")] [SerializeField] 
    private Vector3 VARIATION_ROTATION_MAX = new Vector3(0f, 0f, 360f);

    /// <summary> Model of the gameobject </summary>
    private Transform _islandModel;
    private GameObject _CT_island;
    private GameObject _SO_island;

    // Get Components
    void Awake() {
        _islandModel = transform.Find("Model");
    }
	
	void Start () {
        if (_islandModel != null) {
            RandomizeIsland();
        } else Debug.Log("Model of " + gameObject.name + " was not found !");
    }

    private void RandomizeIsland() {
        //remove previously created objects
        RemoveAllModelChildren();
        //create basic empty islansd for both sides
        _CT_island = new GameObject();
        _CT_island.name = "CTHULHU_ISLAND";
        _SO_island = new GameObject();
        _SO_island.name = "SOBEK_ISLAND";
        //Create random islands and populate them
        RandomizeIslandBase();
        RandomizeIslandProps();
        ApplyVariation();
    }

    private void RemoveAllModelChildren() {
        Transform[] islandTransforms = _islandModel.GetComponentsInChildren<Transform>();
        foreach(Transform t in islandTransforms) {
            if (t != _islandModel.transform) Destroy(t.gameObject);
        }
    }

    /// <summary> </summary>
    private void RandomizeIslandBase() {

    }

    /// <summary> </summary>
    private void RandomizeIslandProps() {

    }

    /// <summary> Modify the visuals of the island's model </summary>
    private void ApplyVariation() {
        //Change Scale
        _islandModel.localScale = new Vector3(  Random.Range(VARIATION_SCALE_MIN[0], VARIATION_SCALE_MAX[0]),
                                                Random.Range(VARIATION_SCALE_MIN[1], VARIATION_SCALE_MAX[1]),
                                                Random.Range(VARIATION_SCALE_MIN[2], VARIATION_SCALE_MAX[2]));
        //Change Rotation
        _islandModel.localRotation = Quaternion.Euler(  Random.Range(VARIATION_ROTATION_MIN[0], VARIATION_ROTATION_MAX[0]),
                                                        Random.Range(VARIATION_ROTATION_MIN[1], VARIATION_ROTATION_MAX[1]),
                                                        Random.Range(VARIATION_ROTATION_MIN[2], VARIATION_ROTATION_MAX[2]));

    }
}
