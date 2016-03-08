using UnityEngine;
using System.Collections;

public class IslandFeeder : MonoBehaviour {

    [Header("Continent Properties")]
    [SerializeField] [Tooltip("Prefab of normal island")]
    private GameObject _islandPrefab;
    [SerializeField]
    [Tooltip("Number of island in each column")]
    private int height = 5;

    [Header("Spawn Properties")]
    [SerializeField] [Tooltip("Basic Island Spawn Rate")]
    private float _spawnRate;




	void Start () {
	
	}
	
	void Update () {
	
	}
}
