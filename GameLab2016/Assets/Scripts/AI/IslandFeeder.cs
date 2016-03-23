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

        [SerializeField] [Tooltip("Prefab of the volcano")]
        private GameObject _volcanoPrefab;

        [SerializeField] [Tooltip("Prefab of the volcano particles")]
        private GameObject _volcanoParticlePrefab;

        [SerializeField] [Tooltip("Prefab of the volcano particles for sobek")]
        private GameObject _volcanoParticlePrefab_SO;

        [SerializeField] [Tooltip("Prefab of the volcano particles for cthulhu")]
        private GameObject _volcanoParticlePrefab_CT;

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
        private int _pVolcanoIsland = 0; //current Number of volcano islands
        private int _pNeutralIsland = 0; //current Number of neutral islands
        //Instantiated objects refs
        private List<List<ChunkWithCollider>> _islandRows;
        private GameObject _islandContainer;
        //Objects refs
        private Transform _islandParentTransform;
        //Layers
        private int _defaultLayer = 0;
        private int _noColLayer = 13;
        //Other Values
        private float _releaseForce = 15f; //force applied when island is released after shake
        private float _timeSinceLastSpawn = 0f; //current time since last island spawn
        //Tutorial Spawning
        private enum TutorialState { NoIsland, OneIsland, ThreeIsland, VolcanoPhase, EndTutorial };
        [Header("TUTORIAL")]
        [Tooltip("Start tutorial or not")]
        public bool _inTutorial = true;
        private TutorialState _state = TutorialState.NoIsland;
        private int _tutoTargetIsland = 0;
        [SerializeField] private float _tutoTimeInPhase = 0f;

        [Header("DEBUG")]
        [SerializeField][Tooltip("DO NOT TOUCH. Visible for DEBUG purposes only")]
        private float _modifiedSpawnRate = 5f; //current spawn rate

        private bool _isStarted = false;
        

        void Awake() {
            _islandRows = new List<List<ChunkWithCollider>>(); //Create data obj
            GenerateIslandContainer();

            _volcanoPrefab = (GameObject)Resources.Load("Island/Volcano");
        }

        public void OnStart() {
            if (_isStarted == false) {
                _isStarted = true;
                _islandParentTransform = GameManager.islandManager.GetIslandSubFolder(); //Get ISland Subfolder from manager
                for (int i = 0; i <= MIN_COLUMN; ++i) {
                    GenerateColumn();
                }
                _targetContinentX = 0;
                StartCoroutine(UpdateSpawnParametersCoroutine());
            }
        }

        void Update() {
            if (_isStarted) {
                if (_inTutorial) {
                    ManageTutorial();
                } else {
                    ManageSpawn();
                }
            }
        }

        void FixedUpdate() {
            if (_isStarted) {
                MoveSpawner();
            }
        }

        private void GenerateIslandContainer() {
            _islandContainer = new GameObject(); //Create GameObject Container
            _islandContainer.transform.parent = transform;
            _islandContainer.transform.localPosition = Vector3.zero;
        }

        /// <summary> Called on tick if not in tutorial. Controls island spawn. </summary>
        private void ManageSpawn() {
            //Update Spawn Timer
            _timeSinceLastSpawn += Time.deltaTime;
            if (_timeSinceLastSpawn > _modifiedSpawnRate) {
                _timeSinceLastSpawn = 0f;
                StartReleaseProcessOnRandomIsland();
            }
        }

        #region Tutorial
        /// <summary> Manages spawner while in tutorial</summary>
        private void ManageTutorial() {
            _tutoTimeInPhase += Time.deltaTime;
            int curAmtIsland = IS_SOBEK ? _pSobekIsland : _pCthulhuIsland;
            if (curAmtIsland < _tutoTargetIsland) {
                StartReleaseProcessOnRandomIsland();
                UpdateSpawnParameters();
            }

            switch (_state) {
                case(TutorialState.NoIsland) :
                    if (_tutoTimeInPhase > 5f) ChangeState(TutorialState.OneIsland);
                    break;
                case (TutorialState.OneIsland):
                    if (_tutoTimeInPhase > 8f) ChangeState(TutorialState.ThreeIsland);
                    break;
                case (TutorialState.ThreeIsland):
                    if (_tutoTimeInPhase > 10f) ChangeState(TutorialState.VolcanoPhase);
                    break;
                case (TutorialState.VolcanoPhase):
                    if (_tutoTimeInPhase > 10f) ChangeState(TutorialState.EndTutorial);
                    if (_pVolcanoIsland < 1) { //Spawn volcano if there is none
                        StartReleaseProcessOnVolcano();
                        UpdateSpawnParameters();
                    }
                    break;
            }
        }

        /// <summary> Manages spawner while in tutorial</summary>
        private void ChangeState(TutorialState newState) {
            _tutoTimeInPhase = 0f;
            _state = newState;
            switch (newState) {
                case (TutorialState.NoIsland):
                    _tutoTargetIsland = 0;                  
                    break;
                case (TutorialState.OneIsland):
                    _tutoTargetIsland = 1;
                    break;
                case (TutorialState.ThreeIsland):
                    _tutoTargetIsland = 3;
                    break;
                case (TutorialState.VolcanoPhase):
                    _tutoTargetIsland = 4;
                    break;
                case (TutorialState.EndTutorial):
                    _tutoTargetIsland = 0;
                    _inTutorial = false;
                    break;
            }
        }


        #endregion

        /// <summary> Moves the spawner to its target destination </summary>
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
            if (_islandContainer == null) GenerateIslandContainer();
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
            StartReleaseProcessOnTargetIsland(_islandRows[0][randIndex]);
        }

        /// <summary> Select an random island in column and start release process on it </summary>
        /// <param name="cwc">Chunk with collider to release</param>
        private void StartReleaseProcessOnTargetIsland(ChunkWithCollider cwc) {
            StartCoroutine(ActivateIsland(cwc));
            if(cwc.chunk.color != IslandUtils.color.volcano) GameManager.islandManager.AddPendingIslandChunk(cwc.chunk); //Remove chunk from pending chunk list
            RemoveIslandChunkFromList(cwc, 0);
        }

        /// <summary> Select a random island in column and trasnform it into a volcano and start release process </summary>
        private void StartReleaseProcessOnVolcano(){
            int randIndex = Random.Range(0, _islandRows[0].Count);
            ChunkWithCollider selectedChunk = _islandRows[0][randIndex];
            RemoveIslandChunkFromList(selectedChunk, 0);
            IslandUtils.color prevColor = selectedChunk.chunk.color;
            selectedChunk.chunk.ConvertChunkToAnotherColor(IslandUtils.color.volcano);
            GameManager.islandManager.AddPendingIslandChunk(selectedChunk.chunk);
            StartCoroutine(TransformIntoVolcano(selectedChunk, prevColor));
            
        }

        /// <summary> Coroutine that transform a basic island into a volcano then start it's spawn process</summary>
        /// <param name="cwc"> Chunk with collider to transform </param> 
        private IEnumerator TransformIntoVolcano(ChunkWithCollider cwc, IslandUtils.color destroyedChunkColor) {
            float volcanoAnimTime = 3f;
            //Get old island model references
            Transform parentTransform = cwc.chunk.transform.Find("Model").transform;
            Transform[] oldModels = parentTransform.GetComponentsInChildren<Transform>();
            //Instantiate volcano prefab
            GameObject instantiatedVolcano = ((GameObject)Instantiate(_volcanoPrefab, parentTransform.position, Quaternion.identity));
            instantiatedVolcano.transform.parent = parentTransform;
            instantiatedVolcano.transform.localScale = Vector3.one;
            instantiatedVolcano.transform.localPosition = new Vector3(0, 0, 2f);
            //Particles
            GameObject volcanoParticles = (GameObject) Instantiate(_volcanoParticlePrefab, parentTransform.position, Quaternion.identity);
            volcanoParticles.transform.parent = parentTransform;
            volcanoParticles.transform.localScale = Vector3.one;
            volcanoParticles.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
            if (destroyedChunkColor == IslandUtils.color.red) {
                GameObject volcanoParticles_SO = (GameObject)Instantiate(_volcanoParticlePrefab_SO, parentTransform.position, Quaternion.identity);
                volcanoParticles_SO.transform.parent = parentTransform;
                volcanoParticles_SO.transform.localScale = Vector3.one;
            }
            if (destroyedChunkColor == IslandUtils.color.blue) {
                GameObject volcanoParticles_CT = (GameObject) Instantiate(_volcanoParticlePrefab_CT, parentTransform.position, Quaternion.identity);
                volcanoParticles_CT.transform.parent = parentTransform;
                volcanoParticles_CT.transform.localScale = Vector3.one;
            }
            //Lerp Volcano into island
            Vector3 sPos = instantiatedVolcano.transform.localPosition;
            for (float i = 0f; i < 1f; i += Time.deltaTime / volcanoAnimTime) {
                instantiatedVolcano.transform.localPosition = Vector3.Lerp(sPos, Vector3.zero, i);
                yield return null;
            }           
            //Destroy old models
            for(int i = 0; i < oldModels.Length; i++) {
                if (oldModels[i] != parentTransform) Destroy(oldModels[i].gameObject);
            }

            StartReleaseProcessOnTargetIsland(cwc);

            volcanoParticles.GetComponentInChildren<ParticleSystem>().emissionRate = 0;
            yield return new WaitForSeconds(1f);
            Destroy(volcanoParticles);
        }

        /// <summary> Activate an island. Island will check player's position to see if it can be released early if they are far. Otherwise wait for the whole duration</summary>
        /// <param name="chunk"> chunk to be release </param>
        IEnumerator ActivateIsland(ChunkWithCollider chunkWithCollider) {
            float t = 0;
            Tweener shakeTweener = chunkWithCollider.chunk.transform.DOShakePosition(SHAKE_TIME_EXTREMUMS.y, .30f, 14, 45, false);
            

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
            GameManager.islandManager.RemovePendingIslandChunk(chunkWithCollider.chunk); //Remove chunk from pending chunk list
            GameManager.islandManager.CreatedIslandChunk(chunkWithCollider.chunk); //Add chunk to chunk list

            chunkWithCollider.chunk.transform.parent = _islandParentTransform; //Set parent
            ToggleCollisionLayer(chunkWithCollider.chunk.gameObject, true); //Toggle island collisions back on
            chunkWithCollider.chunk.gravityBody.Velocity += new Vector2(_releaseForce * (GENERATE_LEFT ? 1 : -1), 0); //Add velocity
            chunkWithCollider.chunk.gameObject.layer = gravityBodyLayer;

            //Play Audio
            chunkWithCollider.chunk.GetComponent<AudioSource>().PlayOneShot(GameManager.audioManager.islandSpecificSound.feederRelease);

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
        private IEnumerator UpdateSpawnParametersCoroutine() {
            while (true) {
                UpdateSpawnParameters();
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void UpdateSpawnParameters() {
            _pScoreDiff = 0f;
            _pIslandDiffPlayers = 0;
            _pSobekIsland = 0;
            _pCthulhuIsland = 0;
            _pVolcanoIsland = 0;
            List<IslandChunk> _CurChunks = GameManager.islandManager.GetIslandChunks();
            List<IslandChunk> _CurPendingChunks = GameManager.islandManager.GetPendingIslandChunks();


            float _pSobekScorePercent = (float)GameManager.levelManager.sobekScore / (float)GameManager.Instance.pointsGoal * 100f;
            float _pCthulhuScorePercent = (float)GameManager.levelManager.cthuluScore / (float)GameManager.Instance.pointsGoal * 100f;


            foreach (IslandChunk ic in _CurChunks)
            {
                if (ic.color == IslandUtils.color.red) ++_pSobekIsland;
                if (ic.color == IslandUtils.color.blue) ++_pCthulhuIsland;
                if (ic.color == IslandUtils.color.volcano) ++_pVolcanoIsland;
            }
            foreach (IslandChunk ic in _CurPendingChunks)
            {
                if (ic.color == IslandUtils.color.red) ++_pSobekIsland;
                if (ic.color == IslandUtils.color.blue) ++_pCthulhuIsland;
                if (ic.color == IslandUtils.color.volcano) ++_pVolcanoIsland;
            }
            if (IS_SOBEK)
            {
                _pIslandDiffPlayers = _pSobekIsland - _pCthulhuIsland;
                _pIslandDiffMin = _pSobekIsland - AMT_ISLAND_MIN;
                _pIslandDiffMax = AMT_ISLAND_MAX - _pSobekIsland;
                _pScoreDiff = _pSobekScorePercent - _pCthulhuScorePercent;
            }
            else {
                _pIslandDiffPlayers = _pCthulhuIsland - _pSobekIsland;
                _pIslandDiffMin = _pCthulhuIsland - AMT_ISLAND_MIN;
                _pIslandDiffMax = AMT_ISLAND_MAX - _pCthulhuIsland;
                _pScoreDiff = _pCthulhuScorePercent - _pSobekScorePercent;
            }

            CalculateSpawnRate();
        }
    }
}