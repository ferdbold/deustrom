using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DestroyGravityBodyOnImpact : MonoBehaviour {

    
    [Tooltip("NOT FOR EDIT. Layers of objects to teleport into wormhole")] [SerializeField] private LayerMask GravityLayerMask;

    [Header("SOUND FX")]
    [Tooltip("Sound to play when an object get destroyed by maelstrom")] [SerializeField] public AudioClip DestroySound;
    private AudioSource audioSource;


	void Start () {
        audioSource = GetComponent<AudioSource>();
	}
	

    /// <summary> Destroy graviy body if triggered</summary>
    /// <param name="other"></param>
    void OnTriggerEnter2D(Collider2D other) {
        if (((1 << other.gameObject.layer) & GravityLayerMask) != 0 && other.gameObject != gameObject) {
            GravityBody gravityBodyScript = other.gameObject.GetComponentInChildren<GravityBody>();
            if (gravityBodyScript != null) {
                if (gravityBodyScript.collisionEnabled == true) {
                    gravityBodyScript.DestroyGravityBody();
                    audioSource.PlayOneShot(DestroySound);
                }
            }
        }
    }
}
