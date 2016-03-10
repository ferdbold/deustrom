using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Simoncouche.Islands;
using DG.Tweening;

public class IslandFeeder : MonoBehaviour {

    /// <summary>
    /// A ChunkWithCollider contains an island chunk that is stored in the feeder and its linked Collider which prevents objects from going in it.
    /// The collider and chunk and not parented since all collisions of the chunk need to be inactive for it to be properly deactivated.
    /// </summary>
    protected struct ChunkWithCollider {
        public IslandChunk chunk { get; private set; }       
        public Collider2D collider { get; private set; }

        public ChunkWithCollider(IslandChunk c, Collider2D col) { chunk = c; collider = col; }
    }

    [Header("Continent Properties")]

    [SerializeField] [Tooltip("Prefab of normal island")]
    private GameObject _islandPrefab;
    [SerializeField] [Tooltip("DO NOT CHANGE. Prefab of island collider.")]
    private GameObject _islandTemporaryCollider;
    [SerializeField] [Tooltip("Generate to left or right")]
    private bool GENERATE_LEFT = true;
    [SerializeField] [Tooltip("Number of island in each column")]
    private int COLUMN_HEIGHT = 5;
    [SerializeField] [Tooltip("minimum amount of visible columns")]
    private int MIN_COLUMN = 5;
    [SerializeField] [Tooltip("Size of an island in X")]
    private float ISLAND_SIZE_X = 3.5f;
    [SerializeField] [Tooltip("Size of an island in Y")]
    private float ISLAND_SIZE_Y = 3.5f;

    [Header("Spawn Properties")]

    [SerializeField] [Tooltip("Default Spawn Time. An island will spawn every SPAWN_RATE seconds")]
    private float SPAWN_RATE = 5f;
    [SerializeField][Tooltip("Min and Max time the island will shake before being spawned.")]
    private Vector2 SHAKE_TIME_EXTREMUMS = new Vector2(2.5f, 10f);

    //Positions
    private int _currentColumn = 0;
    private float _currentX = 0f;
    private float _currentY = 0f;
    //Instantiated objects refs
    private List<List<ChunkWithCollider>> _islandRows;
    private GameObject _islandContainer;
    //Layers
    private int _defaultLayer = 0;
    private int _noColLayer = 13;
    //Other Values
    private float _releaseForce = 15f;
    private float _timeSinceLastSpawn = 0f;
    private float _modifiedSpawnRate = 5f;

	void Awake () {
        _islandRows = new List<List<ChunkWithCollider>>();
        _islandContainer = new GameObject();
        _islandContainer.transform.parent = transform;
        _islandContainer.transform.localPosition = Vector3.zero;
    }

    void Start() {
        for(int i = 0; i <= MIN_COLUMN; i++) {
            GenerateColumn();
        }
        StartCoroutine(UpdateSpawnParameters());
    }
	
	void Update () {
        ManageSpawn();
    }

    void ManageSpawn() {
        _timeSinceLastSpawn += Time.deltaTime;
        //Modify spawn rate based on all parameters
        _modifiedSpawnRate = SPAWN_RATE;

        if (_timeSinceLastSpawn > _modifiedSpawnRate) {
            _timeSinceLastSpawn = 0f;
            StartReleaseProcessOnRandomIsland();
        }
    }

    #region Generation

    /// <summary> Generates a new column and append it to islandRows </summary>
    private void GenerateColumn() {
        int sideMultiplier = GENERATE_LEFT ? -1 : 1;
        if ((_currentColumn) % 2 == 0) _currentY = 0.0f;
        else _currentY = 0.5f * ISLAND_SIZE_Y;
        _currentX = _currentColumn * ISLAND_SIZE_X * sideMultiplier;

        List<ChunkWithCollider> column = new List<ChunkWithCollider>();
        for (int i = 0; i < COLUMN_HEIGHT; i++) {
            column.Add(GenerateIsland());
            _currentY += ISLAND_SIZE_Y;
        }
        _islandRows.Add(column);
        _currentColumn++;
    }

    /// <summary> Generate an island and place it in given Island list</summary>
    private ChunkWithCollider GenerateIsland() {
        //Instantiate island
        IslandChunk generatedChunk = (  GameObject.Instantiate(_islandPrefab,
                                                               _islandContainer.transform.position + new Vector3(_currentX, _currentY, 0),
                                                               Quaternion.identity) 
                                        as GameObject).GetComponent<IslandChunk>();
        generatedChunk.gravityBody.Velocity = Vector2.zero; //Remove velocity
        ToggleCollisionLayer(generatedChunk.gameObject, false); //Remove all collision
        generatedChunk.transform.parent = _islandContainer.transform; //Set parent
        //Create temporary collider until island is spawned
        Collider2D generatedCollider;
        GameObject tempCollider = (GameObject) GameObject.Instantiate(_islandTemporaryCollider, generatedChunk.transform.position, Quaternion.identity);
        tempCollider.transform.parent = generatedChunk.transform; //set as parent to get relative scale
        tempCollider.transform.localScale = Vector3.one;
        tempCollider.transform.parent = _islandContainer.transform; //Remove parenting
        generatedCollider = tempCollider.GetComponent<Collider2D>();

        return (new ChunkWithCollider(generatedChunk, generatedCollider));
        
    }
    
    /// <summary> Set all collisions of collider in given gameobject to default layer if on is true and to NoCollision layer if on is false </summary>
    /// <param name="go"> GameObject to get all children colliders </param>
    /// <param name="on"> Should collision be active </param>
    private void ToggleCollisionLayer(GameObject go, bool on) {
        Collider2D[] cols = go.GetComponentsInChildren<Collider2D>();
        for(int i =0; i < cols.Length; i++) {
            if (on) cols[i].gameObject.layer = _defaultLayer;
            else cols[i].gameObject.layer = _noColLayer;
        }
    }

    /// <summary> Remove chunk from list and handle Column Creation and Destruction</summary>
    /// <param name="chunk"> chunk to remove </param>
    /// <param name="index"> column in which the chunk is</param>
    private void RemoveIslandChunkFromList(ChunkWithCollider chunkWithCol, int index) {
        _islandRows[index].Remove(chunkWithCol);
        if (_islandRows[index].Count == 0) {
            _islandRows.RemoveAt(index);
            GenerateColumn();
        }
    }

    #endregion

    #region Activating and Releasing Islands

    /// <summary> Select an random island in column and start release process on it </summary>
    private void StartReleaseProcessOnRandomIsland() {
        int randIndex = Random.Range(0, _islandRows[0].Count);
        StartCoroutine(ActivateIsland(_islandRows[0][randIndex]));
        RemoveIslandChunkFromList(_islandRows[0][randIndex], 0);
    }

    /// <summary> Activate an island. Island will check player's position to see if it can be released early if they are far. Otherwise wait for the whole duration</summary>
    /// <param name="chunk"> chunk to be release </param>
    IEnumerator ActivateIsland(ChunkWithCollider chunkWithCollider) {
        float t = 0;
        Tweener shakeTweener  = chunkWithCollider.chunk.transform.DOShakePosition(SHAKE_TIME_EXTREMUMS.y, .30f, 14, 45, false);
        while(t<= SHAKE_TIME_EXTREMUMS.y) {
            t += Time.deltaTime; //Timer

            //Release Condition
            if (t > SHAKE_TIME_EXTREMUMS.x) {
                //TODO : CHECK PLAYER POS
                bool isFar = true;
                if (isFar) t += SHAKE_TIME_EXTREMUMS.y;
            }
            yield return null;
        }
        shakeTweener.Complete();
        ReleaseIsland(chunkWithCollider);
    }

    /// <summary> Reactivates the island and release it into the maelstrom. </summary>
    /// <param name="chunk">chunk to release </param>
    private void ReleaseIsland(ChunkWithCollider chunkWithCollider) {
        Destroy(chunkWithCollider.collider.gameObject); //remove temporary collider
        ToggleCollisionLayer(chunkWithCollider.chunk.gameObject, true); //Toggle island collisions back on
        chunkWithCollider.chunk.gravityBody.Velocity += new Vector2(_releaseForce * (GENERATE_LEFT ? 1 : -1), 0);
    }


    #endregion

    /// <summary> Update the spawn paramaters in a timed loop</summary>
    private IEnumerator UpdateSpawnParameters() {
        while (true) {
            //Update PARAMETERS

            yield return new WaitForSeconds(1f);
        }
    }
}
