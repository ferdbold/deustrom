using UnityEngine;
using System.Collections;

public class CameraLookAt : MonoBehaviour {

    [Tooltip("Object to follow. If null, will follow the object with the player tag")]
    public Transform target;

    [Tooltip("Offset of the camera")]
    public Vector3 offset;

    void Start() {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update() {
        transform.LookAt(target.position + offset);

    }

}
