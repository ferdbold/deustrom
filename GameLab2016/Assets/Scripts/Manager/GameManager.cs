using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Simoncouche.Islands;
using System.Collections;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(IslandManager))]
[RequireComponent(typeof(AudioManager))]
[RequireComponent(typeof(UIManager))]
public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }

    [Header("Scoring")]
    [SerializeField]
    [Tooltip("The number of points needed to fill the bar")]
    private int _pointsGoal = 10;
    public int pointsGoal { get { return _pointsGoal; } }

    [SerializeField]
    [Tooltip("Disable scoring (for debug purposes only")]
    private bool _disableScoring = true;
    public bool disableScoring { get { return _disableScoring; } set { _disableScoring = value; } }

    [Tooltip("Number of match to win to finish the game")]
    [SerializeField]
    private int _matchToWin = 2;

    public LevelManager.Player lastWinner = LevelManager.Player.cthulu;

    #region Managers
    public static InputManager inputManager { get; private set; }
    public static IslandManager islandManager { get; private set; }
    public static AudioManager audioManager { get; private set; }
    public static UIManager uiManager { get; private set; }
    public static LevelManager levelManager { get; private set; }
    #endregion

    #region Level Related

    public enum Scene { Menu, PlayLevel, BibleWriter, BibleReader }

    [Header("Scene Related")]
    [SerializeField] [Tooltip("The current scene")]
    private Scene _currentScene;
    public Scene currentScene { get { return _currentScene; } }

    [SerializeField]
    private string SCENE_MENU_NAME = "Menu";

    [SerializeField]
    private string SCENE_CUTSCENE = "Cutscene";

    [SerializeField]
    private string SCENE_BIBLE_WRITING = "BibleWriter";

    [SerializeField]
    private string SCENE_BIBLE_READING = "BibleReader";

    [SerializeField]
    private string[] SCENE_NAMES;

    [SerializeField]
    private float timeForTuto = 1f;


    /// <summary>Amount of time players have to wait until controllers are enabled in a match</summary>
    [Tooltip("Amount of time players have to wait until chains are enabled in a level")]
    [SerializeField]
    private float _timeUntilControllersAreEnabled=0.1f;
    public float timeUntilControllersAreEnabled { get { return _timeUntilControllersAreEnabled; } }


    /// <summary>Amount of rounds players have to wait until chains are enabled in a match</summary>
    [Tooltip("Amount of rounds players have to wait until chains are enabled in a match")]
    [SerializeField]
    private int _amountOfRoundsUntilChainsEnabled = 1;
    public int amountOfRoundsUntilChainsEnabled { get { return _amountOfRoundsUntilChainsEnabled; } }

    /// <summary>
    /// This allows to turn on/off the hook thrower
    /// </summary>
    public bool mustEnableChainThrower { get { return _mustEnableChainThrower; } set { _mustEnableChainThrower = value; } }
    private bool _mustEnableChainThrower;


    #endregion

    #region Utils Variables
    public bool gameStarted { get; private set; }
    public bool isPaused { get; private set; }
    public bool isPausedByTutorial { get; private set; }
    


    private FadeUI fadeUI;
    #endregion

    #region Events

    public UnityEvent Paused { get; private set; }
    public UnityEvent Unpaused { get; private set; } 

    #endregion

    void Awake() {
        this.isPausedByTutorial = false;
        this.gameStarted = false;

        if (Instance == null) {
            Instance = this;
            Application.targetFrameRate = 60; //Set target framerate

            GameManager.inputManager = GetComponent<InputManager>();
            GameManager.islandManager = GetComponent<IslandManager>();
            GameManager.audioManager = GetComponent<AudioManager>();
            GameManager.uiManager = GetComponent<UIManager>();
            fadeUI = GetComponentInChildren<FadeUI>();

            this.Paused = new UnityEvent();
            this.Unpaused = new UnityEvent();

            DontDestroyOnLoad(gameObject);

            Scene_OnOpen(currentScene);
        } else {
            Destroy(gameObject);
        }
    }

    void Update() {
        if (GameManager.levelManager != null && GameManager.levelManager._waitingForMatchEndInput && (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Joystick2Button0))) {
            GameManager.levelManager._waitingForMatchEndInput = false;

            if (GameManager.levelManager.sobekMatchWon >= _matchToWin || GameManager.levelManager.cthuluMatchWon >= _matchToWin) {
                GameManager.Instance.SwitchScene(
                    GameManager.Scene.BibleWriter, 
                    (lastWinner == LevelManager.Player.sobek) ? CutsceneManager.Cutscene.Sobek_WinGame : CutsceneManager.Cutscene.Cthulu_WinGame
                );
            } else {
                GameManager.Instance.SwitchScene(
                    GameManager.Scene.PlayLevel,
                    (lastWinner == LevelManager.Player.sobek) ? CutsceneManager.Cutscene.Sobek_WinMatch : CutsceneManager.Cutscene.Cthulu_WinMatch,
                    dontClose: true
                );
            }
        }

        if (Input.GetKeyDown(KeyCode.F9)) {
            GameManager.levelManager.HardReset();
        }

		if (Input.GetButtonDown("Start")) {
			if(_currentScene == Scene.PlayLevel) OnStartButton();
		}

        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha0)) {
            Time.timeScale = 0f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Time.timeScale = 1f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            Time.timeScale = 5f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            Time.timeScale = 25f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            Time.timeScale = 50f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            Time.timeScale = 100f;
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            if (_currentScene == Scene.PlayLevel && levelManager!=null) levelManager.AddScore(LevelManager.Player.cthulu, 50, Vector3.zero);
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            if (_currentScene == Scene.PlayLevel && levelManager != null) levelManager.AddScore(LevelManager.Player.sobek, 50, Vector3.zero);
        }
        #endif
    }

    #region Switch Scene

    /// <summary>
    /// Change scene to another one and throws event when closing last scene and opening new one
    /// </summary>
    /// <param name="scene">Scene to be loaded</param>
    /// <param name="level">if it's a play level, the index of the level</param>
    public void SwitchScene(Scene scene, CutsceneManager.Cutscene cutscene = CutsceneManager.Cutscene.Base_Loading, int level = 0, bool dontClose = false) {
        if (!dontClose) Scene_OnClose(_currentScene);
        inputManager.ResetInputs();
        _currentScene = scene;

        string sceneToLoad = "";

        switch (scene) {
            case Scene.Menu:
                sceneToLoad = SCENE_MENU_NAME;
                break;

            case Scene.PlayLevel:
                if (level < SCENE_NAMES.Length) {
                    sceneToLoad = SCENE_NAMES[level];
                } else {
                    Debug.LogError("The scene you are trying to access does not exist. The scene names array is out of range");
                }
                break;

            case Scene.BibleWriter:
                sceneToLoad = SCENE_BIBLE_WRITING;
                break;

            case Scene.BibleReader:
                sceneToLoad = SCENE_BIBLE_READING;
                break;
        }


        StartCoroutine(WaitForSceneToLoad(sceneToLoad, scene, cutscene));
    }

    /// <summary>
    /// Waits for the scene to be loaded and calls respective event
    /// </summary>
    /// <param name="loading">the async operation of the scene being loaded</param>
    /// <param name="scene">the scene being loaded</param>
    /// <returns></returns>
    private IEnumerator WaitForSceneToLoad(string sceneToLoad, Scene scene, CutsceneManager.Cutscene cutsceneVideo) {
        SceneManager.LoadSceneAsync(SCENE_CUTSCENE);
        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
        loading.allowSceneActivation = false;

        CutsceneManager cutscene = null;
        int debugCount = 0;
        while (cutscene == null) {
            cutscene = GameObject.FindObjectOfType<CutsceneManager>();
            yield return new WaitForRealSeconds(0.1f);
            if (++debugCount > 1000) {
                Debug.LogError("Cutscene not loaded or no cutscene object in scene");
                break;
            }
        }

        cutscene.PlayCutscene(cutsceneVideo);

        while (!loading.isDone || !cutscene.isDone) {
            yield return new WaitForRealSeconds(0.05f);
            if (cutscene.isDone) {
                loading.allowSceneActivation = true;
            }
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
        Scene_OnOpen(scene);
        SceneManager.UnloadScene(SCENE_CUTSCENE);
    }

    

    /// <summary>
    /// Event called when the scene is loaded
    /// </summary>
    /// <param name="scene">The scene loaded</param>
    private void Scene_OnOpen(Scene scene) {
        fadeUI.StartFadeAnim(false);
        switch (scene) {
            case Scene.Menu:
                audioManager.ToggleAmbiantSounds(false);
                if(!audioManager.IsMenuPlaying())audioManager.PlayMusic(MusicSound.Choice.menu);
                break;

            case Scene.PlayLevel:
                audioManager.ToggleGameplaySounds(true);
                audioManager.ToggleAmbiantSounds(true);
                audioManager.PlayMusic(MusicSound.Choice.play);
                islandManager.Setup();
                if (levelManager == null) {
                    levelManager = new LevelManager(_matchToWin);
                } else {
                    levelManager.Setup();
                }

                uiManager.Setup();
                
                // Start Feeder
                IslandFeeder[] feeders = GameObject.FindObjectsOfType<IslandFeeder>();
                for (int i = 0; i < feeders.Length; i++) {
                    if (levelManager.currentRound == 0)
                        feeders[i]._inTutorial = true;
                    else
                        feeders[i]._inTutorial = false;
                    feeders[i].OnStart();
                }

                audioManager.ToggleAmbiantSounds(true);              
                StartCoroutine("CalculateScoreCoroutine");
                this.gameStarted = true;

                break;

            case Scene.BibleWriter:
                audioManager.PlayMusic(MusicSound.Choice.menu);
                audioManager.ToggleAmbiantSounds(false);
                break;

            case Scene.BibleReader:
                audioManager.ToggleAmbiantSounds(false);
                break;
        }
    }

    /// <summary>
    /// Event called when the scene is closed
    /// </summary>
    /// <param name="scene">the scene closed</param>
    private void Scene_OnClose(Scene scene) {
        this.gameStarted = false;
        fadeUI.StartFadeAnim(false);

        switch (scene) {
            case Scene.Menu:
                break;

            case Scene.PlayLevel:
                levelManager = null;
                GameManager.inputManager.ResetInputs();
                StopCoroutine("CalculateScoreCoroutine");
                break;

            case Scene.BibleWriter:
                break;

            case Scene.BibleReader:
                break;
        }
    }

    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame() {
        Application.Quit();
    }

    #endregion

    #region Tutorial

    public void StartVideoTutorial(TutorialUI.TutoChoice choice) {
        GameManager.uiManager.SetupTutoWidget();
        GameManager.uiManager._tutoWidget.StartTuto(choice);
    }

    #endregion

    #region Utils

	private void OnStartButton() {
		Debug.Log ("Start");

		if (_currentScene == Scene.PlayLevel && gameStarted) {
            if (this.isPaused && !this.isPausedByTutorial) {
                GameManager.audioManager.ToggleGameplaySounds(true);
                GameManager.audioManager.ToggleAmbiantSounds(true);
                UnPause();
            } else {
                Pause();
            }
		}
	}

    /// <summary>
    /// Pause the game
    /// </summary>
    public void Pause() {
        Debug.LogWarning("The game was paused, don't freak out");
        ChangePauseStatus(true);
        GameManager.audioManager.ToggleGameplaySounds(false);
        GameManager.audioManager.ToggleAmbiantSounds(false);
        GameManager.audioManager.ToggleLowMusicVolume(true);

        this.Paused.Invoke();
    }

    /// <summary>
    /// UnPause the game
    /// </summary>
    public void UnPause() {
        ChangePauseStatus(false);
        GameManager.audioManager.ToggleGameplaySounds(true);
        GameManager.audioManager.ToggleAmbiantSounds(true);
        GameManager.audioManager.ToggleLowMusicVolume(false);

        this.Unpaused.Invoke();
    }

    public void UnPauseFromTutorial() {
        ChangePauseStatus(false);
        GameManager.audioManager.ToggleGameplaySounds(true);
        GameManager.audioManager.ToggleAmbiantSounds(true);
        GameManager.audioManager.ToggleLowMusicVolume(false);
        GameManager.uiManager.OnTutoComplete();

        this.Unpaused.Invoke();
        isPausedByTutorial = false;
    }

    public void PauseFromTutorial() {
        ChangePauseStatus(false);
        GameManager.audioManager.ToggleGameplaySounds(true);
        GameManager.audioManager.ToggleAmbiantSounds(true);
        GameManager.audioManager.ToggleLowMusicVolume(false);
        this.Unpaused.Invoke();
        isPausedByTutorial = true;
    }

    private void ChangePauseStatus(bool pause) {
        isPaused = pause;
        Time.timeScale = isPaused ? 0 : 1;
        inputManager.isDisabled = pause;

    }

    #endregion
    
    private IEnumerator CalculateScoreCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (levelManager == null) break;
            foreach (IslandChunk ic in islandManager.GetIslandChunks())
            {
                if (ic.color == IslandUtils.color.red) levelManager.AddScore(LevelManager.Player.sobek, 1, ic.transform.position);
                else if (ic.color == IslandUtils.color.blue) levelManager.AddScore(LevelManager.Player.cthulu, 1, ic.transform.position);
            }

        }
    }

    public IEnumerator HardResetGivePoint(int cthuluScore, int sobekScore) {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < sobekScore; i++) {
            if (GameManager.uiManager != null) GameManager.uiManager.AddPoint(LevelManager.Player.sobek, Vector3.zero);
        }
        for (int i = 0; i < cthuluScore; i++) {
            if (GameManager.uiManager != null) GameManager.uiManager.AddPoint(LevelManager.Player.cthulu, Vector3.zero);
        }
    }
}
