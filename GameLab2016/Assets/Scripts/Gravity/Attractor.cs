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
    [SerializeField] private float SIDE_FORCE = 1f;

    //Drag Values
    [Header("Linear Drag :")]
    [Tooltip("Min and max linear drag values to add to gravity body based on distance between attractor and body.")]
    [SerializeField] private Vector2 _dragMaximums = new Vector2(0f, 1.5f);
    private float _additionnalDragRate = 0.025f;
    private float _additionnalDragDistance = 0.75f;

    protected override void Start () {
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

    /// <summary> Calculate the force to make the gravity body rotate around the attractor </summary>
    /// <param name="body">Gravity body affected by attractor </param>
    /// <returns>Acceleration to add to the gravity body</returns>
    public override Vector2 ApplyGravityForce(GravityBody body) {
        //Apply force toward Attractor
        Vector2 accFromAttraction = ApplyAttractionForce(body);

        //Apply force for Rotation around attractor
        Vector2 accFromRotation = ApplyRotationForce(body);
        
        //Modify the body's linear drag so it is attracted to the center of the attractor
        if(body.gameObject.tag != "Player") ModifyLinearDrag(body);

        //Get final force and return it
        Vector2 finalForce = accFromAttraction + accFromRotation; //mix both acceleration
        finalForce = finalForce.normalized * accFromAttraction.magnitude; //limit final acceleration's length to the attraction force's magnitude
        return finalForce;
    }

    /// <summary> Apply a gravitational force toward the attractor's center </summary>
    /// <param name="body"> Gravity body affected by attractor </param>
    /// <returns> Acceleration from the attraction force</returns>
    private Vector2 ApplyAttractionForce(GravityBody body) {
        Vector2 forceDirectionAtt = transform.position - body.transform.position;  //Get force direction
        Vector2 accAtt = GRAVITYCONSTANT * (forceDirectionAtt.normalized * Force); //Get acceleration

        //Take into account body/attractor's distance
        Vector2 accDistance = CalculateAccFromDistance(accAtt, body);
        accAtt += accDistance;
        //Take into account body's weigth
        Vector2 accWeigth = CalculateAccFromWeigth(accAtt, body);
        accAtt += accWeigth;

        return accAtt;
    }

    /// <summary> Apply a force in the direction of the attractor's rotation direction to make the body turn around the attractor's center </summary>
    /// <param name="body"> Gravity body affected by attractor </param>
    /// <returns> Acceleration from the rotation force</returns>
    private Vector2 ApplyRotationForce(GravityBody body) {
        //Apply force toward a side of the attractor
        Vector2 forceDirectionAtt = transform.position - body.transform.position;
        Vector2 forceDirectionSide = new Vector2(-forceDirectionAtt.y, forceDirectionAtt.x);
        Vector2 accSide = SIDE_FORCE * forceDirectionSide.normalized;
        //Reduce the side force if the player is already going in that direction
        Vector3 projection = Vector3.ClampMagnitude(Vector3.Project(forceDirectionAtt, forceDirectionSide), forceDirectionAtt.magnitude);
        if (accSide.normalized == ((Vector2)projection).normalized) accSide = projection;

        return accSide;
    }

    /// <summary> Modify the linear drag so objects are gradually attracted to this attractor's center </summary>
    private void ModifyLinearDrag(GravityBody body) {
        float distance = Vector2.Distance(transform.position, body.transform.position);
        float distanceStep = 1 - (distance / radius);

        //If object is really close to maelstrom's center, progressively add drag so it slowly reaches the center
        if (distanceStep >= _additionnalDragDistance) {
            body.AdditionnalDrag += Time.deltaTime * _additionnalDragRate;
        } else {
            body.AdditionnalDrag -= Time.deltaTime * _additionnalDragRate;
        }

        //Debug.Log("drag : " + Mathf.Lerp(_dragMaximums.x, _dragMaximums.y, distanceStep) + "    With step of " + distanceStep);
        body.LinearDrag = Mathf.Lerp(_dragMaximums.x, _dragMaximums.y, distanceStep) + body.AdditionnalDrag + body.DefaultDrag;
    }

}
