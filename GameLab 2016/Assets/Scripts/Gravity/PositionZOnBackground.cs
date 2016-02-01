using UnityEngine;
using System.Collections;

/// <summary>
/// Raycast to find an object on Z Axis that is background layer and position Z of gameobject on it
/// </summary>
public class PositionZOnBackground : MonoBehaviour {

    [SerializeField] private float _zOffset = 0f;    
    [SerializeField] private LayerMask _backgroundLayerMask;

    private bool drawGizmo = false;
    private Vector3 GizmoLocation = Vector3.zero;
	// Update is called once per frame
	void Update () {
        RepositionObjectZ();
	}

    /// <summary>
    /// Reposition the object on the z axis based on the background
    /// </summary>
    void RepositionObjectZ() {
        Ray rayZ = new Ray(transform.position + new Vector3(0,0,-1) , new Vector3(0, 0, 1));
        Ray rayInverseZ = new Ray(transform.position + new Vector3(0, 0, 1), new Vector3(0, 0, -1));
        RaycastHit hit;
        float targetZ = 0;

        Debug.DrawRay(transform.position, new Vector3(0, 0, 1000));
        if (Physics.Raycast(rayZ, out hit, 1000, _backgroundLayerMask)) {
            GizmoLocation = hit.point;
            //drawGizmo = true;
            Debug.Log("blabla");
            
            targetZ = hit.point.z;        
        } else if(Physics.Raycast(rayInverseZ, out hit, 1000, _backgroundLayerMask)) {
            targetZ = hit.point.z;
        }

        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, targetZ);
        transform.position = newPosition;
    }

    void OnDrawGizmos() {
        if(drawGizmo) {
            Gizmos.DrawSphere(GizmoLocation, 1f);
        }
    }
}
