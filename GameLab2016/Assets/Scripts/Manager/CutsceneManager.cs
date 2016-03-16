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
    private RawImage FadeRect;

    private void Awake() {
        isDone = false;
    }

    public void PlayCutscene(Cutscene scene) {
        MovieTexture movie = null;
        Texture2D texture = null;

        switch (scene) {
            case Cutscene.Intro:
                movie = Intro_Cutscene;
                FadeRect.material.mainTexture = movie;
                movie.Play();
                StartCoroutine(WaitVideoEndToFade(movie));
                break;

            case Cutscene.Sobek_Win:
                texture = Sobek_Win_Cutscene;
                StartCoroutine(WaitForImageEnd());
                break;

            case Cutscene.Cthulu_Win:
                texture = Cthulu_Win_Cutscene;
                StartCoroutine(WaitForImageEnd());
                break;

            case Cutscene.Base_Loading:
                movie = Base_Loading_Cutscene;
                FadeRect.material.mainTexture = movie;
                movie.Play();
                StartCoroutine(WaitVideoEndToFade(movie));
                break;
        }

		
    }

    IEnumerator WaitVideoEndToFade(MovieTexture movie) {
        while (movie.isPlaying) {
            yield return new WaitForEndOfFrame();
        }

        isDone = true;

        //Fade UI
        FadeRect.DOFade(0, timeToFade);
        yield return new WaitForSeconds(timeToFade);

        
    }

    IEnumerator WaitForImageEnd() {
        yield return new WaitForSeconds(imageTimeStaying);

        isDone = true;

        //Fade UI
        FadeRect.DOFade(0, timeToFade);
        yield return new WaitForSeconds(timeToFade);
    }
}
