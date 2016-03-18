using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class CutsceneManager : MonoBehaviour {

    public bool isDone { get; private set; }

    public enum Cutscene { Base_Loading, Intro, Sobek_Win, Cthulu_Win }

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
    private Texture2D Sobek_Win_Cutscene;

    [SerializeField]
    private Texture2D Cthulu_Win_Cutscene;

    [Header("Component")]
    [SerializeField]
    private RawImage Video;

    [SerializeField]
    private Image FadeRect;

    private bool skip = false;

    private void Awake() {
        isDone = false;
        FadeRect.color = new Color(FadeRect.color.r, FadeRect.color.g, FadeRect.color.b, 0);
        Video.gameObject.SetActive(false);
        FadeUI(true);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Joystick1Button7) || Input.GetKeyDown(KeyCode.Joystick1Button7) || Input.GetKeyDown(KeyCode.Space)) { //TODO change input
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

            case Cutscene.Sobek_Win:
                Video.material.mainTexture = Sobek_Win_Cutscene;
                StartCoroutine("WaitForImageEnd");
                break;

            case Cutscene.Cthulu_Win:
                Video.material.mainTexture = Cthulu_Win_Cutscene;
                StartCoroutine("WaitForImageEnd");
                break;

            case Cutscene.Base_Loading:
                movie = Base_Loading_Cutscene;
                Video.material.mainTexture = movie;
                movie.Play();
                StartCoroutine(WaitVideoEndToFade(movie));
                break;
        }

		
    }

    IEnumerator WaitVideoEndToFade(MovieTexture movie) {
        yield return new WaitForSeconds(timeToFade);
        Video.gameObject.SetActive(true);

        while (movie.isPlaying) {
            yield return new WaitForEndOfFrame();
            if (skip) {
                movie.Stop();
                break;
            }
        }
        isDone = true;

        FadeUI(false);


    }

    IEnumerator WaitForImageEnd() {
        yield return new WaitForSeconds(timeToFade);
        Video.gameObject.SetActive(true);

        yield return new WaitForSeconds(imageTimeStaying);

        isDone = true;
        
        FadeUI(false);
    }

    void FadeUI (bool fadeIn) {
        Video.gameObject.SetActive(false);
        FadeRect.DOFade(fadeIn ? 1 : 0, timeToFade);
    }
}
