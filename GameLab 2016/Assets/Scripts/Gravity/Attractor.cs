using UnityEngine;


/// <summary>
/// Gravity Modifier that attracts objects affected
/// </summary>
public class Attractor : GravityModifier {



    [Tooltip("Force of the attraction toward the center of the whirlpool")]
    [SerializeField] private float FORCE = 1f;
    public float Force { get { return FORCE; } private set { FORCE = value; }}

    /// <summary> multiplier of force applied from body's velocity compared to force towards body </summary>
    [Tooltip("Factor of the force to be applied to the side instead of toward the attractor. The higher this is, the more force will be applied in the whirlpool's direction")]
    [SerializeField] private float SIDE_FORCE_FACTOR = 0.1f;


    void Start () {
        base.Start();
        SetupCollider();
    }

    /// <summary>
    /// Setup the _collider
    /// </summary>
    protected virtual void SetupCollider() {
        //Get Component References
        _collider = GetComponent<CircleCollider2D>();
        if (_collider == null) _collider = gameObject.AddComponent<CircleCollider2D>();
        //Set _collider values
        _collider.radius = radius;
        _collider.isTrigger = true;
    }

    public override Vector2 ApplyGravityForce(GravityBody body) {
        //Apply force toward Attractor
        Vector2 forceDirectionAtt = transform.position - body.transform.position;  //Get force direction
        Vector2 accAtt = GRAVITYCONSTANT * (forceDirectionAtt.normalized * Force); //Get acceleration

        //Take into account body/attractor's distance
        Vector2 accDistance = CalculateAccFromDistance(accAtt, body);
        accAtt += accDistance;
        //Take into account body's weigth
        Vector2 accWeigth = CalculateAccFromWeigth(accAtt, body);
        accAtt += accWeigth;
        

        //Apply force toward body's current velocity
        Vector2 forceDirectionVel = body.Velocity;  //Get force direction
        Vector2 accVel = SIDE_FORCE_FACTOR * accAtt.magnitude * forceDirectionVel.normalized;
        accVel = Vector2.zero;
       

        return accAtt + accVel;
    }

}
