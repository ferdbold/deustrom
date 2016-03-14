using UnityEngine;
using Simoncouche.Islands;
using System.Collections;
using UnityEngine.SceneManagement;

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

    [Tooltip("Number of match to win to finish the game")]
    [SerializeField]
    private int _matchToWin = 2;

    #region Managers
    public static InputManager inputManager { get; private set; }
    public static IslandManager islandManager { get; private set; }
    public static AudioManager audioManager { get; private set; }
    public static UIManager uiManager { get; private set; }
    public static LevelManager levelManager { get; private set; }
    #endregion

    #region Level Related

    public enum Scene { Menu, PlayLevel, BibleWriting }

    [Header("Scene Related")]
    [SerializeField] [Tooltip("The current scene")]
    private Scene _currentScene;
    public Scene currentScene { get { return _currentScene; } }

    [SerializeField]
    private string SCENE_MENU_NAME = "Menu";

    [SerializeField]
    private string SCENE_CUTSCENE = "Cutscene";

    [SerializeField]
    private string SCENE_BIBLE_WRITING = "Bible";

    [SerializeField]
    private string[] SCENE_NAMES;

    #endregion

    void Awake() {
        if (Instance == null) {
            Instance = this;

            GameManager.inputManager = GetComponent<InputManager>();
            GameManager.islandManager = GetComponent<IslandManager>();
            GameManager.audioManager = GetComponent<AudioManager>();
            GameManager.uiManager = GetComponent<UIManager>();

            DontDestroyOnLoad(gameObject);

            Scene_OnOpen(currentScene);
        } else {
            Destroy(gameObject);
        }
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.R)) {
            SwitchScene(currentScene);
            inputManager.ResetInputs();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
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
        #endif
    } 

    #region Switch Scene

    /// <summary>
    /// Change scene to another one and throws event when closing last scene and opening new one
    /// </summary>
    /// <param name="scene">Scene to be loaded</param>
    /// <param name="level">if it's a play level, the index of the level</param>
    public void SwitchScene(Scene scene, CutsceneManager.Cutscene cutscene = CutsceneManager.Cutscene.Base_Loading, int level = 0) {
        Scene_OnClose(currentScene);
        _currentScene = scene;

        SceneManager.LoadScene(SCENE_CUTSCENE);
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

            case Scene.BibleWriting:
                sceneToLoad = SCENE_BIBLE_WRITING;
                break;
        }

        
        StartCoroutine(WaitForSceneToLoad(sceneToLoad, scene));
    }

    /// <summary>
    /// Waits for the scene to be loaded and calls respective event
    /// </summary>
    /// <param name="loading">the async operation of the scene being loaded</param>
    /// <param name="scene">the scene being loaded</param>
    /// <returns></returns>
    private IEnumerator WaitForSceneToLoad(string sceneToLoad, Scene scene) {
		AsyncOperation loading = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
		loading.allowSceneActivation = false;

		CutsceneManager cutscene = null;
		int debugCount = 0;
		while (cutscene == null) {
			cutscene = GameObject.FindObjectOfType<CutsceneManager>();
			yield return new WaitForSeconds(0.1f);
			if (++debugCount > 500) {
				Debug.LogError("Cutscene not loaded or no cutscene object in scene");
				break;
			}
		}

        while (!loading.isDone && !cutscene.isDone) { yield return new WaitForEndOfFrame(); }
        SceneManager.UnloadScene(SCENE_CUTSCENE);
        loading.allowSceneActivation = true;
        Scene_OnOpen(scene);
    }

    /// <summary>
    /// Event called when the scene is loaded
    /// </summary>
    /// <param name="scene">The scene loaded</param>
    private void Scene_OnOpen(Scene scene) {
        switch (scene) {
            case Scene.Menu:
                break;

            case Scene.PlayLevel:
                levelManager = new LevelManager(_pointsGoal, _matchToWin);
                break;

            case Scene.BibleWriting:
                break;
        }
    }

    /// <summary>
    /// Event called when the scene is closed
    /// </summary>
    /// <param name="scene">the scene closed</param>
    private void Scene_OnClose(Scene scene) {
        switch (scene) {
            case Scene.Menu:
                break;

            case Scene.PlayLevel:
                levelManager = null;
                break;

            case Scene.BibleWriting:
                break;
        }
    }

    #endregion
}
