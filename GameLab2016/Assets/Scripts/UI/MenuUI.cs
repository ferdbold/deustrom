using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour {

    [SerializeField]
    private Button firstActiveButton;

    private void Awake() {
        firstActiveButton.Select();
    }

	public void PlayButton() {
        GameManager.Instance.SwitchScene(GameManager.Scene.PlayLevel, CutsceneManager.Cutscene.Intro);
    }

    public void BiblesButton() {
        GameManager.Instance.SwitchScene(GameManager.Scene.BibleReader);
    }

    public void QuitGame() {
        GameManager.Instance.QuitGame();
    }
}
