using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace Simoncouche.Islands {
    public class IslandFeeder : MonoBehaviour {

        /// <summary>
        /// A ChunkWithCollider contains an island chunk that is stored in the feeder and its linked Collider which prevents objects from going in it.
        /// The collider and chunk and not parented since all collisions of the chunk need to be inactive for it to be properly deactivated.
        /// </summary>
        public struct ChunkWithCollider {
            public IslandChunk chunk;
            public Collider2D collider;
            public ChunkWithCollider(IslandChunk c, Collider2D col) { chunk = c; collider = col; }
        }

        [Header("Continent Properties")]

        [SerializeField] [Tooltip("Prefab of normal island")]
        private GameObject _islandPrefab;

        [SerializeField] [Tooltip("DO NOT CHANGE. Prefab of island collider.")]
        private GameObject _islandTemporaryColliderPrefab;

        [SerializeField] [Tooltip("Generate to left or right")]
        private bool GENERATE_LEFT = true;

        [SerializeField] [Tooltip("Use Sobek or Cthulhu Data for spawn properties ")]
        private bool IS_SOBEK = true;

        [SerializeField] [Tooltip("Number of island in each column")]
        private List<Transform> SpawnAnchors;

        [SerializeField] [Tooltip("minimum amount of visible columns")]
        private int MIN_COLUMN = 5;

        [SerializeField] [Tooltip("Size of an island in X")]
        private float ISLAND_SIZE_X = 3.5f;

        [SerializeField] [Tooltip("Size of an island in Y")]
        private float ISLAND_SIZE_Y = 3.5f;

        [Header("Spawn Properties")]

        //Gravity body layer
        private int gravityBodyLayer = 10;

        [SerializeField] [Tooltip("Default Spawn Time. An island will spawn every SPAWN_RATE seconds")]
        private float SPAWN_RATE = 5f;

        [SerializeField] [Tooltip("Default Target Amount of island. If under this amt of islands, spawn will aceclerate")]
        private int AMT_ISLAND_MIN = 5;

        [SerializeField] [Tooltip("Default Target Amount of island. If over this amt of islands, spawn will reduce")]
        private int AMT_ISLAND_MAX = 10;

        [SerializeField] [Tooltip("Spawn rate change in % per percentage of score difference between players.")]
        private float SPAWN_CHANGE_PER_SCORE_DIFFERENCE = 2f;

        [SerializeField] [Tooltip("Spawn rate change in % for each island difference between players.")]
        private float SPAWN_CHANGE_PER_ISLAND_DIFFERENCE_BETWEEN_PLAYERS = 15f;

        [SerializeField] [Tooltip("Spawn rate change in % for each island difference between median.")]
        private float SPAWN_CHANGE_PER_ISLAND_DIFFERENCE_BETWEEN_MIN = 10f;

        [SerializeField] [Tooltip("Spawn rate change in % for each island difference between median.")]
        private float SPAWN_CHANGE_PER_ISLAND_DIFFERENCE_BETWEEN_MAX = 40f;
        
        [Tooltip("If true, Spawn change will act in a multiplicative manner. If false, will simply add % together.")]
        private bool SPAWN_CHANGE_MULTIPLICATIVE = false;


        [Header("Visual")]

        [SerializeField] 
        [Tooltip("Min and Max time the island will shake before being spawned.")]
        private Vector2 SHAKE_TIME_EXTREMUMS = new Vector2(2.5f, 10f);

        //Positions
        private int _currentColumn = 0;
        private float _currentX = 0f;
        private float _currentY = 0f;
        private float _targetContinentX = 0;
        private bool _isMoving = false;
        //Spawn Parameters 
        private float _pScoreDiff = 0f; //Score diff between players in %. If positive, Spawn Faster.
        private int _pIslandDiffPlayers = 0; //Number of island difference between players.  If positive, Spawn Faster.
        private int _pIslandDiffMin = 0; //Number of island difference between min.  If positive, Spawn Faster.
        private int _pIslandDiffMax = 0; //Number of island difference between max.  If positive, Spawn Slower.
        private int _pSobekIsland = 0; //current Number of island of sobek
        private int _pCthulhuIsland = 0; //current Number of island of cthulhu
        //Instantiated objects refs
        private List<List<ChunkWithCollider>> _islandRows;
        private GameObject _islandContainer;
        //Objects refs
        private IslandManager _islandManager;
        private Transform _islandParentTransform;
        //Layers
        private int _defaultLayer = 0;
        private int _noColLayer = 13;
        //Other Values
        private float _releaseForce = 15f; //force applied when island is released after shake
        private float _timeSinceLastSpawn = 0f; //current time since last island spawn
        [Header("DEBUG")]
        [SerializeField][Tooltip("DO NOT TOUCH. Visible for DEBUG purposes only")] private float _modifiedSpawnRate = 5f; //current spawn rate

        

        void Awake() {
            _islandRows = new List<List<ChunkWithCollider>>(); //Create data obj
            _islandContainer = new GameObject(); //Create GameObject Container
            _islandContainer.transform.parent = transform;
            _islandContainer.transform.localPosition = Vector3.zero;
 
            _islandManager = FindObjectOfType<IslandManager>().GetComponent<IslandManager>(); //Get Island Manager
        }

        void Start() {
            _islandParentTransform = _islandManager.GetIslandSubFolder(); //Get ISland Subfolder from manager
            for (int i = 0; i <= MIN_COLUMN; ++i) {
                GenerateColumn();
            }
            _targetContinentX = 0;
            StartCoroutine(UpdateSpawnParameters());
        }

        void Update() {
            ManageSpawn();
        }

        void FixedUpdate() {
            MoveSpawner();
        }

        private void ManageSpawn() {
            //Update Spawn Timer
            _timeSinceLastSpawn += Time.deltaTime;
            if (_timeSinceLastSpawn > _modifiedSpawnRate) {
                _timeSinceLastSpawn = 0f;
                StartReleaseProcessOnRandomIsland();
            }
        }

        private void MoveSpawner() {
            if (_isMoving) {
                Vector3 tPos = new Vector3(_targetContinentX, 0, 0);
                if (Vector3.Distance(_islandContainer.transform.localPosition, tPos) > 0.15f) {
                    _islandContainer.transform.localPosition = Vector3.Lerp(_islandContainer.transform.localPosition,
                                                                            tPos,
                                                                            0.8f * Time.deltaTime);
                } else {
                    _isMoving = false;
                    _islandContainer.transform.localPosition = tPos;
                }
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
            for (int i = 0; i < SpawnAnchors.Count; i++) {
                column.Add(GenerateIsland(SpawnAnchors[i].localPosition + new Vector3(_currentX,0,0)));
                _currentY += ISLAND_SIZE_Y;
            }
            _islandRows.Add(column);
            _currentColumn++;
            _targetContinentX += GENERATE_LEFT ? ISLAND_SIZE_X : -ISLAND_SIZE_X;
            _isMoving = true;
        }

        /// <summary> Generate an island and place it in given Island list</summary>
        private ChunkWithCollider GenerateIsland(Vector3 position) {
            //Instantiate island
            IslandChunk generatedChunk = (GameObject.Instantiate(_islandPrefab,
                                                                   _islandContainer.transform.position + position,
                                                                   Quaternion.identity)
                                            as GameObject).GetComponent<IslandChunk>();
            generatedChunk.gravityBody.Velocity = Vector2.zero; //Remove velocity
            ToggleCollisionLayer(generatedChunk.gameObject, false); //Remove all collision
            generatedChunk.transform.parent = _islandContainer.transform; //Set parent
            //Create temporary collider until island is spawned
            Collider2D generatedCollider;
            GameObject tempCollider = (GameObject)GameObject.Instantiate(_islandTemporaryColliderPrefab, generatedChunk.transform.position, Quaternion.identity);
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
            for (int i = 0; i < cols.Length; i++) {
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


        private void ToggleCollidersOnColumn(int index, bool active) {
            foreach (ChunkWithCollider cc in _islandRows[index]) {
                cc.collider.gameObject.SetActive(active);
            }
        }

        private void ToggleColliderOnSpecificChunk(int column, int row, bool active) {
            _islandRows[column][row].collider.gameObject.SetActive(active);
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
            Tweener shakeTweener = chunkWithCollider.chunk.transform.DOShakePosition(SHAKE_TIME_EXTREMUMS.y, .30f, 14, 45, false);
            _islandManager.CreatedIslandChunk(chunkWithCollider.chunk); //Add chunk to chunk list

            while (t <= SHAKE_TIME_EXTREMUMS.y) {
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

            chunkWithCollider.chunk.transform.parent = _islandParentTransform; //Set parent
            ToggleCollisionLayer(chunkWithCollider.chunk.gameObject, true); //Toggle island collisions back on
            chunkWithCollider.chunk.gravityBody.Velocity += new Vector2(_releaseForce * (GENERATE_LEFT ? 1 : -1), 0); //Add velocity
            chunkWithCollider.chunk.gameObject.layer = gravityBodyLayer;

        }


        #endregion

        private void CalculateSpawnRate() {
            _modifiedSpawnRate = SPAWN_RATE;

            if (SPAWN_CHANGE_MULTIPLICATIVE) { //Add in a multiplicative manner
                if(_pScoreDiff < 0) _modifiedSpawnRate *= (100f + (_pScoreDiff * SPAWN_CHANGE_PER_SCORE_DIFFERENCE)) / 100f;
                _modifiedSpawnRate *= (100f + (_pIslandDiffPlayers * SPAWN_CHANGE_PER_ISLAND_DIFFERENCE_BETWEEN_PLAYERS)) / 100f;
                if (_pIslandDiffMin < 0) _modifiedSpawnRate *= (100f + (_pIslandDiffMin * SPAWN_CHANGE_PER_ISLAND_DIFFERENCE_BETWEEN_MIN)) / 100f;
                if (_pIslandDiffMax < 0) _modifiedSpawnRate *= (100f - (_pIslandDiffMax * SPAWN_CHANGE_PER_ISLAND_DIFFERENCE_BETWEEN_MAX)) / 100f;
            } else { //Add in a additive manner
                float additiveSpawnRate = 100f;
                if (_pScoreDiff < 0) additiveSpawnRate += (_pScoreDiff * SPAWN_CHANGE_PER_SCORE_DIFFERENCE);
                additiveSpawnRate += (_pIslandDiffPlayers * SPAWN_CHANGE_PER_ISLAND_DIFFERENCE_BETWEEN_PLAYERS);
                if (_pIslandDiffMin < 0) additiveSpawnRate += (_pIslandDiffMin * SPAWN_CHANGE_PER_ISLAND_DIFFERENCE_BETWEEN_MIN);
                if (_pIslandDiffMax < 0) additiveSpawnRate -= (_pIslandDiffMax * SPAWN_CHANGE_PER_ISLAND_DIFFERENCE_BETWEEN_MAX);

                _modifiedSpawnRate *= (additiveSpawnRate / 100f);
            }

            _modifiedSpawnRate = Mathf.Max(_modifiedSpawnRate, 0.5f);
        }

        /// <summary> Update the spawn paramaters in a timed loop</summary>
        private IEnumerator UpdateSpawnParameters() {
            while (true) {
                _pScoreDiff = 0f;
                _pIslandDiffPlayers = 0;
                _pSobekIsland = 0;
                _pCthulhuIsland = 0;
                List<IslandChunk> _CurChunks = _islandManager.GetIslandChunks();

                float _pSobekScorePercent = (float)GameManager.levelManager.sobekScore / (float)GameManager.levelManager.scoreNeededToWin * 100f;
                float _pCthulhuScorePercent = (float)GameManager.levelManager.cthuluScore / (float)GameManager.levelManager.scoreNeededToWin * 100f;



                foreach (IslandChunk ic in _CurChunks) {
                    if (ic.color == IslandUtils.color.red) ++_pSobekIsland;
                    if (ic.color == IslandUtils.color.blue) ++_pCthulhuIsland;                  
                }
                if (IS_SOBEK) {
                    _pIslandDiffPlayers = _pSobekIsland - _pCthulhuIsland;
                    _pIslandDiffMin = _pSobekIsland - AMT_ISLAND_MIN;
                    _pIslandDiffMax = AMT_ISLAND_MAX - _pSobekIsland;
                    _pScoreDiff = _pSobekScorePercent - _pCthulhuScorePercent;
                } else {
                    _pIslandDiffPlayers = _pCthulhuIsland - _pSobekIsland;
                    _pIslandDiffMin = _pCthulhuIsland - AMT_ISLAND_MIN;
                    _pIslandDiffMax = AMT_ISLAND_MAX - _pCthulhuIsland;
                    _pScoreDiff = _pCthulhuScorePercent - _pSobekScorePercent;
                }

                CalculateSpawnRate();

                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}