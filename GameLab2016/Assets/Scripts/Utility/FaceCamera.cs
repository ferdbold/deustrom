using UnityEngine;
using System.Collections;

public class FaceCamera : MonoBehaviour {

    public bool onlyOnStart = false;

	// Use this for initialization
	void OnEnable () {
        FaceTheCamera();
	}
	
	void Update () {
        if (!onlyOnStart) FaceTheCamera();
	}

    void FaceTheCamera () {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.back, Camera.main.transform.rotation * Vector3.up); 
    }

}
