using UnityEngine;
using System.Collections;

/// <summary>
/// Basic Gravity Object. Base class used to create objects affected by gravity forces.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class GravityObject : MonoBehaviour {

    [Header("Properties : ")]
    static public float GRAVITYCONSTANT = 10f; 

    [Tooltip("NOT YET IMPLEMENTED : Objects will only be influenced by objects of higher or equal attraction level than themselves. ")]
    [SerializeField] protected int attractionLevel = 10; //used to determine what object affter what other gravityObj
    [Tooltip("Radius of the collider. For reference, 0.5 is the default size for a sphere collision (fits the model) ")]
    [SerializeField] protected float radius = 5f;

    protected CircleCollider2D _collider;


    protected virtual void Start() {
        
    }

    protected virtual void Awake() {

    }


}
