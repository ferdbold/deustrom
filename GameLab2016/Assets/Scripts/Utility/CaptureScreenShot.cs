#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

public class CaptureScreenShot : MonoBehaviour {

	[MenuItem("SimonCouche/Take ScreenShot")]
	static void TakeScreenShot () {
		Application.CaptureScreenshot("Screenshots.png", 15);
	}
}
#endif
