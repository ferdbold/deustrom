using UnityEngine;
using System.Collections;

public class PlayIslandDestructionSound : MonoBehaviour {

	void Start() {
        GetComponent<AudioSource>().PlayOneShot(GameManager.audioManager.islandSpecificSound.destructionSound);
    }
}
