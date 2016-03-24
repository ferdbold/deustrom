using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialUI : MonoBehaviour {

    [SerializeField]
    private float timeToOpen = 1f;

    [SerializeField]
    private RawImage movie;

    [SerializeField]
    private RawImage tuto1;

    [SerializeField]
    private RawImage tuto2;

    public bool isCompleted { get; private set; }

    private bool skip = false;

    public enum TutoChoice { one, two }

	public void StartTuto(TutoChoice choice) {
        this.skip = false;
        this.isCompleted = false;

        switch (choice) {
            case TutoChoice.one:
                tuto1.gameObject.SetActive(true);
                StartCoroutine(PlayTuto(tuto1));
                break;

            case TutoChoice.two:
                tuto2.gameObject.SetActive(false);
                StartCoroutine(PlayTuto(tuto2));
                break;
        }
    }

    void Awake() {
        isCompleted = false;
        GetComponent<RectTransform>().DOScale(0, 0);
        tuto1.gameObject.SetActive(false);
        tuto2.gameObject.SetActive(false);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Joystick2Button0) || Input.GetKeyDown(KeyCode.F1)) { 
            skip = true;
        }
    }

    IEnumerator PlayTuto(RawImage movie) {
        OpenClose(true, timeToOpen);
        yield return new WaitForSeconds(timeToOpen);
        MovieTexture video = (MovieTexture)movie.mainTexture;
        video.loop = true;
        video.Play();
        GameManager.audioManager.PlayAudioClip(video.audioClip);
        GameManager.Instance.Pause();

        while (!skip) {
            yield return new WaitForRealSeconds(0.05f);
        }

        GameManager.Instance.UnPause();
        video.Stop();
        isCompleted = true;

        OpenClose(false, timeToOpen);
        yield return new WaitForSeconds(timeToOpen);
    }

    private void OpenClose(bool open, float time) {
        GetComponent<RectTransform>().DOScale(open ? Vector3.one : Vector3.zero, time);
    }
}
