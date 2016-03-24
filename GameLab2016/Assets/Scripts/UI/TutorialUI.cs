﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialUI : MonoBehaviour {

    [SerializeField]
    private float timeToOpen = 1f;

    [SerializeField]
    private RawImage movie;

    [SerializeField]
    private MovieTexture videoOne;

    [SerializeField]
    private MovieTexture videoTwo;

    public bool isCompleted { get; private set; }

    private bool skip = false;

    public enum TutoChoice { one, two }

	public void StartTuto(TutoChoice choice) {
        MovieTexture video = null;
        this.skip = false;
        this.isCompleted = false;

        switch (choice) {
            case TutoChoice.one:
                video = videoOne;
                break;

            case TutoChoice.two:
                video = videoTwo;
                break;
        }

        StartCoroutine(PlayTuto(video));
    }

    void Awake() {
        isCompleted = false;
        GetComponent<RectTransform>().DOScale(0, 0);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Joystick2Button0) || Input.GetKeyDown(KeyCode.F1)) { 
            skip = true;
        }
    }

    IEnumerator PlayTuto(MovieTexture video) {
        OpenClose(true, timeToOpen);
        yield return new WaitForSeconds(timeToOpen);
        movie.material.mainTexture = video;
        video.Play();
        video.loop = true;
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
