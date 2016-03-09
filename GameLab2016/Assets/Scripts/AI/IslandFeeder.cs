using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Simoncouche.Islands;

public class IslandFeeder : MonoBehaviour {

    [Header("Continent Properties")]

    [SerializeField] [Tooltip("Prefab of normal island")]
    private GameObject _islandPrefab;
    [SerializeField] [Tooltip("Number of island in each column")]
    private int COLUMN_HEIGHT = 5;
    [SerializeField] [Tooltip("minimum amount of visible columns")]
    private int MIN_COLUMN = 5;
    [SerializeField] [Tooltip("Size of an island in X")]
    private float ISLAND_SIZE_X = 3.5f;
    [SerializeField] [Tooltip("Size of an island in Y")]
    private float ISLAND_SIZE_Y = 3.5f;

    [Header("Spawn Properties")]

    [SerializeField] [Tooltip("Basic Island Spawn Rate")]
    private float _spawnRate;

    //Positions
    private int currentColumn = 0;
    private float currentX = 0f;
    private float currentY = 0f;
    //Instantiated objects refs
    private List<List<IslandChunk>> IslandRows;
    private GameObject IslandContainer;
    //Layers
    private int defaultLayer = 0;
    private int collisionOffLayer = 13;

	void Awake () {
        IslandRows = new List<List<IslandChunk>>();
        IslandContainer = new GameObject();
        IslandContainer.transform.parent = transform;
        IslandContainer.transform.localPosition = Vector3.zero;
    }

    void Start() {
        for(int i = 0; i <= MIN_COLUMN; i++) {
            GenerateColumn();
        }
    }
	
	void Update () {
	
	}

    #region Generation

    /// <summary> Generates a new column and append it to islandRows </summary>
    private void GenerateColumn() {
        if ((currentColumn) % 2 == 0) currentY = 0.0f;
        else currentY = 0.5f * ISLAND_SIZE_Y;
        currentX = currentColumn * ISLAND_SIZE_X;

        List<IslandChunk> column = new List<IslandChunk>();
        for (int i = 0; i < COLUMN_HEIGHT; i++) {
            column.Add(GenerateIsland());
            currentY += ISLAND_SIZE_Y;
        }
        currentColumn++;
    }
    /// <summary> Generate an island and place it in given Island list</summary>
    private IslandChunk GenerateIsland() {
        IslandChunk generatedChunk = (  GameObject.Instantiate(_islandPrefab,
                                                               IslandContainer.transform.position + new Vector3(currentX, currentY, 0),
                                                               Quaternion.identity) 
                                        as GameObject).GetComponent<IslandChunk>();

        ToggleCollisionLayer(generatedChunk.gameObject, false);
        return generatedChunk;
    }
    
    /// <summary> Set all collisions of collider in given gameobject to default layer if on is true and to NoCollision layer if on is false </summary>
    /// <param name="go"> GameObject to get all children colliders </param>
    /// <param name="on"> Should collision be active </param>
    private void ToggleCollisionLayer(GameObject go, bool on) {
        Collider2D[] cols = go.GetComponentsInChildren<Collider2D>();
        for(int i =0; i < cols.Length; i++) {
            if (on) cols[i].gameObject.layer = defaultLayer;
            else cols[i].gameObject.layer = collisionOffLayer;
        }
    }

    #endregion
}
