using UnityEngine;
using System.Collections;

public class MenuUI : MonoBehaviour {

	public void PlayButton() {
        GameManager.Instance.SwitchScene(GameManager.Scene.PlayLevel);
    }
}
