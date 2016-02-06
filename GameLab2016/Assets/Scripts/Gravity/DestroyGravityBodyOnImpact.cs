using UnityEngine;
using System.Collections;

public class DestroyGravityBodyOnImpact : MonoBehaviour {

    [Tooltip("NOT FOR EDIT. Layers of objects to teleport into wormhole")] [SerializeField] private LayerMask GravityLayerMask;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary> Destroy graviy body if triggered</summary>
    /// <param name="other"></param>
    void OnTriggerEnter2D(Collider2D other) {
        if (((1 << other.gameObject.layer) & GravityLayerMask) != 0 && other.gameObject != gameObject) {
            GravityBody gravityBodyScript = other.gameObject.GetComponentInChildren<GravityBody>();
            if (gravityBodyScript != null) {
                if (gravityBodyScript.collisionEnabled == true) gravityBodyScript.DestroyGravityBody();
            }
        }
    }
}
