using UnityEngine;
using System.Collections;

public class BibleUIScript : MonoBehaviour {

	public void ExitButton() {
        GameManager.Instance.SwitchScene(GameManager.Scene.Menu);
    }
}
