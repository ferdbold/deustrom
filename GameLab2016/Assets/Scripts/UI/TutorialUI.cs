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
        if (Input.GetKeyDown(KeyCode.Joystick1Button7) || Input.GetKeyDown(KeyCode.Joystick1Button7) || Input.GetKeyDown(KeyCode.Space)) { //TODO change input
            GameManager.Instance.UnPause();
            Destroy(gameObject);
        }
    }

    IEnumerator PlayTuto() {
        if (video != null) {
            movie.material.mainTexture = video;
            GameManager.audioManager.PlayAudioClip(video.audioClip);
            video.Play();
            while (video.isPlaying) {
                yield return new WaitForRealSeconds(0.05f);
            }
        } 
        GameManager.Instance.UnPause();
        Destroy(gameObject);
    }
}
