using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GravityForceZone : GravityModifier {

    
    [Tooltip("Force applied to gravity bodies inside this zone")][SerializeField] private Vector2 FORCE = Vector2.zero;
    private float gravityForceFactor = 0.1f; //Force applied by gravityForceZone is multiplied by this

	// Use this for initialization
	void Start () {
        base.Start();
	}

    /// <summary>
    /// Method called every FixedUpdate by objects affected by this gravity modifier
    /// </summary>
    /// <param name="body">object affected by this modifier</param>
    /// <returns>force to apply to object</returns>
    public override Vector2 ApplyGravityForce(GravityBody body) {
        //Calculate acceleration based on force
        Vector2 acc = GRAVITYCONSTANT * gravityForceFactor * FORCE;
        Vector2 accWeigth = CalculateAccFromWeigth(acc, body);
        acc += accWeigth;
        return acc;
    }

}
