using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimoncoucheIntro : MonoBehaviour {

    MovieTexture movie;

    public RawImage image;

	void Start () {
        movie = (MovieTexture)image.mainTexture;
        movie.Play();
        StartCoroutine(Wait());
	}

    IEnumerator Wait() {
        while (movie.isPlaying) {
            yield return new WaitForEndOfFrame();
        }
        SceneManager.LoadScene("Menu");
    }
}
