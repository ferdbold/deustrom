using UnityEngine;

/// <summary>
/// Gravity modifier that repulses Objects affected by gravity that enter it's radius
/// </summary>
public class Repulsor : Attractor {

    
    public override Vector2 ApplyGravityForce(GravityBody body) {
        //Apply force toward Attractor
        Vector2 forceDirectionAtt = transform.position - body.transform.position;  //Get force direction
        Vector2 acc = GRAVITYCONSTANT * (forceDirectionAtt.normalized * Force); //Get acceleration

        //Take into account body/attractor's distance
        Vector2 accDistance = CalculateAccFromDistance(acc, body);
        acc += accDistance;
        //Take into account body's weigth
        Vector2 accWeigth = CalculateAccFromWeigth(acc, body);
        acc += accWeigth;    

        acc *= -1f; //inverse it
        return acc;
    }
}
