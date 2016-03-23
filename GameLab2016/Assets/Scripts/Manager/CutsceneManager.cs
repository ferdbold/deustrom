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
    private Texture2D Sobek_WinMatch_Cutscene;

    [SerializeField]
    private Texture2D Cthulu_WinMatch_Cutscene;

    [SerializeField]
    private Texture2D Sobek_WinGame_Cutscene;

    [SerializeField]
    private Texture2D Cthulu_WinGame_Cutscene;

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
                Video.material.mainTexture = movie;
                movie.Play();
                GameManager.audioManager.PlayAudioClip(movie.audioClip);
                StartCoroutine(WaitVideoEndToFade(movie));
                break;

            case Cutscene.Sobek_WinMatch:
                Video.material.mainTexture = Sobek_WinMatch_Cutscene;
                StartCoroutine("WaitForImageEnd");
                break;

            case Cutscene.Cthulu_WinMatch:
                Video.material.mainTexture = Cthulu_WinMatch_Cutscene;
                StartCoroutine("WaitForImageEnd");
                break;

            case Cutscene.Sobek_WinGame:
                Video.material.mainTexture = Sobek_WinGame_Cutscene;
                StartCoroutine("WaitForImageEnd");
                break;

            case Cutscene.Cthulu_WinGame:
                Video.material.mainTexture = Cthulu_WinGame_Cutscene;
                StartCoroutine("WaitForImageEnd");
                break;

            case Cutscene.Base_Loading:
                if (Base_Loading_Cutscene != null) {
                    Video.material.mainTexture = Base_Loading_Cutscene;
                    movie.Play();
                    StartCoroutine(WaitVideoEndToFade(movie));
                } else {
                    Debug.LogWarning("No cutscene for loading screen");
                    StartCoroutine("WaitForImageEnd");
                }
                break;
        }

		
    }

    IEnumerator WaitVideoEndToFade(MovieTexture movie) {
        yield return new WaitForRealSeconds(timeToFade);
        Video.gameObject.SetActive(true);

        while (movie.isPlaying) {
            yield return new WaitForRealSeconds(0.05f);
            if (skip) {
                movie.Stop();
                break;
            }
        }
        isDone = true;

        FadeUI(false);


    }

    IEnumerator WaitForImageEnd() {
        yield return new WaitForRealSeconds(timeToFade);
        Video.gameObject.SetActive(true);

        yield return new WaitForRealSeconds(imageTimeStaying);

        isDone = true;
        
        FadeUI(false);
    }

    void FadeUI (bool fadeIn) {
        Video.gameObject.SetActive(false);
        FadeRect.DOFade(fadeIn ? 1 : 0, timeToFade);
    }
}
