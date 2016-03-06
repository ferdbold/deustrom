using UnityEngine;
using System.Collections;

public class KeepSamePosition : MonoBehaviour {

    public bool lockXAxis = false;
    public bool lockYAxis = false;
    public bool lockZAxis = false;

    private float xStartPos;
    private float yStartPos;
    private float zStartPos;

    // Use this for initialization
    void Start () {
        xStartPos = transform.position.x;
        yStartPos = transform.position.y;
        zStartPos = transform.position.z;
    }
    

    void Update () {
        float newX, newY, newZ;

        if (lockXAxis) newX = xStartPos;
        else newX = transform.position.x;

        if (lockYAxis) newY = yStartPos;
        else newY = transform.position.y;

        if (lockZAxis) newZ = zStartPos;
        else newZ = transform.position.z;

        transform.position = new Vector3(newX, newY, newZ);
    }
}
