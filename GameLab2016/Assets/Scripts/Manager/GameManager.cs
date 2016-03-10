using UnityEngine;
using Simoncouche.Islands;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(IslandManager))]
[RequireComponent(typeof(AudioManager))]
[RequireComponent(typeof(UIManager))]
public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }

    // Link to every manager
    public static InputManager inputManager { get; private set; }
    public static IslandManager islandManager { get; private set; }
    public static AudioManager audioManager { get; private set; }
    public static UIManager uiManager { get; private set; }

    void Awake() {
        if (Instance == null) {
            Instance = this;

            GameManager.inputManager = GetComponent<InputManager>();
            GameManager.islandManager = GetComponent<IslandManager>();
            GameManager.audioManager = GetComponent<AudioManager>();
            GameManager.uiManager = GetComponent<UIManager>();

            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene(0);
            inputManager.ResetInputs();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
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
    }
}
