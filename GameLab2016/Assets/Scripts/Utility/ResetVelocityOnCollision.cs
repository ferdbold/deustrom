using UnityEngine;
using System.Collections;

public class ResetVelocityOnCollision : MonoBehaviour {
    
   [SerializeField] [Tooltip("Block on x Axis")] private bool BLOCK_X;
   [SerializeField] [Tooltip("Block on y Axis")] private bool BLOCK_Y;
    
	void OnCollisionEnter2D(Collision2D coll) {
        Rigidbody2D rb2d = coll.collider.GetComponentInChildren<Rigidbody2D>();
        if (rb2d == null) Debug.Log("Collided with object with no rigidbody " + coll.collider);
        else {
            if (BLOCK_X && BLOCK_Y) rb2d.velocity = Vector2.zero;
            else if (BLOCK_X) rb2d.velocity = new Vector2(0, rb2d.velocity.y/2f);
            else if (BLOCK_Y) rb2d.velocity = new Vector2(rb2d.velocity.x/2f, 0);
            else Debug.LogWarning("ResetVelocity Script is set to do nothing. If intended, script should be removed.");
        }
    }
}
