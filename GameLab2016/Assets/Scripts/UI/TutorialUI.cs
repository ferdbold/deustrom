using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour {

    [SerializeField]
    private RawImage movie;

    [SerializeField]
    private MovieTexture video;

	public void StartTuto() {
        StartCoroutine(PlayTuto());
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) { //TODO change input
            GameManager.Instance.UnPause();
            Destroy(gameObject);
        }
    }

    IEnumerator PlayTuto() {
        movie.material.mainTexture = video;
        Debug.Log(video);
        GameManager.audioManager.PlayAudioClip(video.audioClip);
        video.Play();
        while (video.isPlaying) {
            yield return new WaitForRealSeconds(0.05f);
        }
        GameManager.Instance.UnPause();
        Destroy(gameObject);
    }
}
