using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class CutsceneManager : MonoBehaviour {

    public bool isDone { get; private set; }

    public enum Cutscene { Base_Loading, Intro, Sobek_WinMatch, Cthulu_WinMatch, Sobek_WinGame, Cthulu_WinGame }

    [Header("Parameter")]
    [SerializeField] [Tooltip("Fading time when the cutscene ends")]
    private float timeToFade = 1f;
    public float TimeToFade { get { return timeToFade; } }

    [SerializeField] [Tooltip("The time before the image loading completes")]
    private float imageTimeStaying = 1f;

    [Header("Cutscenes")]
    [SerializeField]
    private MovieTexture Base_Loading_Cutscene;

    [SerializeField]
    private MovieTexture Intro_Cutscene;

    [SerializeField]
    private MovieTexture Sobek_WinMatch_Cutscene;

    [SerializeField]
    private MovieTexture Cthulu_WinMatch_Cutscene;

    [SerializeField]
    private MovieTexture Sobek_WinGame_Cutscene;

    [SerializeField]
    private MovieTexture Cthulu_WinGame_Cutscene;

    [Header("Component")]
    [SerializeField]
    private RawImage Video;

    [SerializeField]
    private Image FadeRect;

    private bool skip = false;

    private void Awake() {
        isDone = false;
        FadeRect.DOFade(0, 0);
        Video.gameObject.SetActive(false);
        FadeUI(true);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) { //TODO change input
            skip = true;
            isDone = true;
            StopCoroutine("WaitForImageEnd");
            FadeUI(false);
        }
    }

    public void PlayCutscene(Cutscene scene) {
        MovieTexture movie = null;

        switch (scene) {
            case Cutscene.Intro:
                movie = Intro_Cutscene;
                break;

            case Cutscene.Sobek_WinMatch:
                movie = Sobek_WinMatch_Cutscene;
                break;

            case Cutscene.Cthulu_WinMatch:
                movie = Cthulu_WinMatch_Cutscene;
                break;

            case Cutscene.Sobek_WinGame:
                GameManager.audioManager.PlayAudioClipMusic(GameManager.audioManager.characterSpecificSound.sobekSpecificSound.winInGame);
                movie = Sobek_WinGame_Cutscene;
                break;

            case Cutscene.Cthulu_WinGame:
                GameManager.audioManager.PlayAudioClipMusic(GameManager.audioManager.characterSpecificSound.cthuluSpecificSound.winInGame);
                movie = Cthulu_WinGame_Cutscene;
                break;

            case Cutscene.Base_Loading:
                movie = Base_Loading_Cutscene;
                break;
        }
        Video.material.mainTexture = movie;
        movie.Play();
        if(scene!=Cutscene.Cthulu_WinMatch 
            || scene!=Cutscene.Sobek_WinMatch) GameManager.audioManager.PlayAudioClipMusic(movie.audioClip);
        StartCoroutine(WaitVideoEndToFade(movie));
    }

    IEnumerator WaitVideoEndToFade(MovieTexture movie) {
        yield return new WaitForRealSeconds(timeToFade);
        Video.gameObject.SetActive(true);

        while (movie.isPlaying) {
            yield return new WaitForRealSeconds(0.05f);
            if (skip) {
                movie.Stop();
                Video = null;
                break;
            }
        }
        movie.Stop();
        Video = null;
        isDone = true;
    }

    IEnumerator WaitForImageEnd() {
        yield return new WaitForRealSeconds(timeToFade);
        Video.gameObject.SetActive(true);
        yield return new WaitForRealSeconds(imageTimeStaying);

        isDone = true;
    }

    void FadeUI (bool fadeIn) {
        Video.gameObject.SetActive(false);
        FadeRect.DOFade(fadeIn ? 1 : 0, timeToFade);
    }
}
