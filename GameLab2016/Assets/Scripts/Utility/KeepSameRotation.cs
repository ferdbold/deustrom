using UnityEngine;
using System.Collections;

public class KeepSameRotation : MonoBehaviour {

    public bool lockXAxis = false;
    public bool lockYAxis = false;
    public bool lockZAxis = false;

    private float xStartRot;
    private float yStartRot;
    private float zStartRot;

    // Use this for initialization
    void Start() {
        xStartRot = transform.rotation.eulerAngles.x;
        yStartRot = transform.rotation.eulerAngles.y;
        zStartRot = transform.rotation.eulerAngles.z;
    }


    void Update() {
        float newX, newY, newZ;

        if (lockXAxis) newX = xStartRot;
        else newX = transform.rotation.eulerAngles.x;

        if (lockYAxis) newY = yStartRot;
        else newY = transform.rotation.eulerAngles.y;

        if (lockZAxis) newZ = zStartRot;
        else newZ = transform.rotation.eulerAngles.z;

        transform.rotation = Quaternion.Euler(new Vector3(newX, newY, newZ));
    }

}
