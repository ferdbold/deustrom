using UnityEngine;
using System.Collections;

public class MultiDisplay : MonoBehaviour {
	
	void Start () {
		if (Display.displays.Length > 1) {
			Display.displays[1].Activate();
		}
	}
}
