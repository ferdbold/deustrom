using UnityEngine;
using System.Collections;

/// <summary>
/// RandomizeIslandVisual randomizes the island's model so it looks different from the other chunks of island
/// </summary>
public class RandomizeIslandVisual : MonoBehaviour {

    [Header("Minimum scale for each values of transform")][SerializeField]
    private Vector3 VARIATION_SCALE_MIN = new Vector3(0.95f, 0.95f, 0.975f);

    [Header("Maximum scale for each values of transform")] [SerializeField] 
    private Vector3 VARIATION_SCALE_MAX = new Vector3(1.05f, 1.05f, 1.025f);

    [Header("Minimum rotation for each values of transform")] [SerializeField] 
    private Vector3 VARIATION_ROTATION_MIN = new Vector3(0f, 0f, 0f);

    [Header("Minimum rotation for each values of transform")] [SerializeField] 
    private Vector3 VARIATION_ROTATION_MAX = new Vector3(0f, 0f, 360f);

    /// <summary> Model of the gameobject </summary>
    private Transform _islandModel;
    
    // Get Components
	void Awake() {
        _islandModel = transform.Find("Model");
    }
	
	void Start () {
        if (_islandModel != null) ApplyVariation();
        else Debug.Log("Model of " + gameObject.name + " was not found !");
    }

    /// <summary>
    /// Modify the visuals of the island's model
    /// </summary>
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
