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

    [Header("Cutscenes")]
    [SerializeField]
    private MovieTexture Base_Loading_Cutscene;

    [SerializeField]
    private MovieTexture Intro_Cutscene;

    [SerializeField]
    private MovieTexture Sobek_Win_Cutscene;

    [SerializeField]
    private MovieTexture Cthulu_Win_Cutscene;

    [Header("Component")]
    [SerializeField]
    private RawImage FadeRect;

    private void Awake() {
        isDone = false;
    }

    public void PlayCutscene(Cutscene scene) {
        MovieTexture movie = null;

        switch (scene) {
            case Cutscene.Intro:
                movie = Intro_Cutscene;
                break;

            case Cutscene.Sobek_Win:
                movie = Sobek_Win_Cutscene;
                break;

            case Cutscene.Cthulu_Win:
                movie = Cthulu_Win_Cutscene;
                break;

            case Cutscene.Base_Loading:
                movie = Base_Loading_Cutscene;
                break;
        }

		FadeRect.material.mainTexture = movie;
        movie.Play();
        StartCoroutine(WaitVideoEndToFade(movie));
    }

    IEnumerator WaitVideoEndToFade(MovieTexture movie) {
        while (movie.isPlaying) {
            yield return new WaitForEndOfFrame();
        }

        //Fade UI
        FadeRect.DOFade(0, timeToFade);
        yield return new WaitForSeconds(timeToFade);

        isDone = true;
    }
}
