using UnityEngine;

/// <summary>
/// Gravity Objects that modify the behaviour of objects that enter its radius.
/// </summary>
public abstract class GravityModifier : GravityObject {

    [Tooltip("Importance of the weigth of the gravity body. For example, if this is 0, force will always be the same no matter the body's weigth. Normal Value : 1.")]
    [SerializeField] protected float WEIGTH_IMPORTANCE = 1f;

    [Tooltip("Importance of the distance between player and attractor. For example, if this is 0, force will always be at its maximum no matter the distance. Normal Value : 1.")]
    [SerializeField] protected float DISTANCE_IMPORTANCE = 1f;

    /// <summary> Distance from the attractor at which the force reaches its maximum power </summary>
    protected float MIN_DIST_MULTIPLIER = 2f;

    // Use this for initialization
    protected virtual void Start() {
        base.Start();
        if (gameObject.layer != 9) gameObject.layer = 8; //If not player layer, set gravityModifier Layer
    }

    /// <summary>
    /// Method called every FixedUpdate by objects affected by this gravity modifier
    /// </summary>
    /// <param name="body">object affected by this modifier</param>
    /// <returns>force to apply to object</returns>
    public abstract Vector2 ApplyGravityForce(GravityBody body);

    /// <summary>
    /// Calculate the impact of gravity body's weigth on a given force depending on weigthImportance and return the force to apply as a Vector2.
    /// </summary>
    /// <param name="AccForce">Current Force</param>
    /// <param name="body">gravity body's info</param>
    /// <returns></returns>
    protected Vector2 CalculateAccFromWeigth(Vector2 accForce, GravityBody body) {
        Vector2 accWeigth = accForce / body.Weigth;
        accWeigth = accWeigth - accForce;
        accWeigth = accWeigth * WEIGTH_IMPORTANCE;
        return accWeigth;
    }

    /// <summary>
    /// Calculate the impact of gravity body's distance on a given force depending on distanceImportance and return the force to apply as a Vector2.
    /// </summary>
    /// <param name="AccForce"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    protected Vector2 CalculateAccFromDistance(Vector2 accForce, GravityBody body) {
        float bodyDistance = Vector3.Distance(transform.position, body.transform.position);
        Vector2 accDistance = accForce / Mathf.Max(bodyDistance, MIN_DIST_MULTIPLIER);
        accDistance = accDistance - accForce;
        accDistance = accDistance * DISTANCE_IMPORTANCE;
        return accDistance;
    }

}
