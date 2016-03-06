using UnityEngine;
using System.Collections;

/// <summary>
/// Raycast to find an object on Z Axis that is background layer and position Z of gameobject on it
/// </summary>
public class PositionZOnBackground : MonoBehaviour {

    [Tooltip("Offset on z Axis from the z value of the background")][SerializeField] public float _zOffset = 0f;    
    [Tooltip("DO NOT TOUCH. Layers of the background collision.")][SerializeField] private LayerMask _backgroundLayerMask;

    // Update is called once per frame
    void Update () {
        RepositionObjectZ();
    }

    /// <summary>
    /// Reposition the object on the z axis based on the background
    /// </summary>
    void RepositionObjectZ() {
        Ray rayZ = new Ray(transform.position + new Vector3(0,0,-100) , new Vector3(0, 0, 1)); 
        Ray rayInverseZ = new Ray(transform.position + new Vector3(0, 0, 100), new Vector3(0, 0, -1));
        RaycastHit hit;
        float targetZ = 0;

        //Debug.DrawRay(transform.position, new Vector3(0, 0, 1000));
        //Debug.DrawRay(transform.position, new Vector3(0, 0, -1000));
        if (Physics.Raycast(rayZ, out hit, 1000, _backgroundLayerMask)) {       
            targetZ = hit.point.z;        
        } else if(Physics.Raycast(rayInverseZ, out hit, 1000, _backgroundLayerMask)) {
            targetZ = hit.point.z;
        }

        targetZ += _zOffset;
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, targetZ);
        transform.position = newPosition;
    }

}
