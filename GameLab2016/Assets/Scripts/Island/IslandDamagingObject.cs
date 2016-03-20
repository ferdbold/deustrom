using UnityEngine;
using System.Collections;
using Simoncouche.Islands;

[RequireComponent(typeof(CircleCollider2D))]
public class IslandDamagingObject : MonoBehaviour {

    [SerializeField] [Tooltip("the damage taken")]
    private int _damage;

    void OnCollisionEnter2D(Collision2D other) {
        IslandChunk chunk = other.gameObject.GetComponent<IslandChunk>();
        Debug.Log(other.gameObject.name);
        if (chunk != null) {
            chunk.TakeDamage(_damage, Vector3.zero);
            Destroy(gameObject);
        }
    }
}
