using UnityEngine;


/// <summary>
/// Gravity Modifier that attracts objects affected
/// </summary>
public class Attractor : GravityModifier {



    [Tooltip("Force of the attraction toward the center of the whirlpool")]
    [SerializeField] private float FORCE = 1f;
    public float Force { get { return FORCE; } private set { FORCE = value; }}

    /// <summary> multiplier of force applied from body's velocity compared to force towards body </summary>
    [Tooltip("Force of the rotation of the attractor. Clockwise if positive, counter-clockwise if negative.")]
    [SerializeField] private float SIDE_FORCE = 0.1f;


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


        //Apply force toward a side of the attractor
        Vector2 forceDirectionSide = new Vector2(-forceDirectionAtt.y, forceDirectionAtt.x);
        Vector2 accSide = SIDE_FORCE * forceDirectionSide.normalized;
        //Reduce the side force if the player is already going in that direction
        Vector3 projection = Vector3.ClampMagnitude(Vector3.Project(forceDirectionAtt, forceDirectionSide), forceDirectionAtt.magnitude);
        if (accSide.normalized == ((Vector2)projection).normalized) accSide = projection;

        //Get final force
        Vector2 finalForce = accAtt + accSide;
        finalForce = finalForce.normalized * accAtt.magnitude;
        return finalForce;
    }

}
