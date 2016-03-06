using UnityEngine;


public class Camera2DFollow : MonoBehaviour {

    [Tooltip("Object to follow. If null, will follow the object with the player tag")]
    public Transform target;

    [Tooltip("Lerp factor to the current target position.")] [SerializeField] 
    private float LERP_SPEED = 0.25f;

    [Tooltip("Speed at which the target positionof the camera moves.")] [SerializeField] 
    private float INTERP_SPEED = 10f;

    [Tooltip("Position on the Z axis of the camera")] [SerializeField] 
    private float Z_POSITION = -10f;

    void Start () {
        if (target == null)  target = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = new Vector3(target.position.x, target.position.y, Z_POSITION);
    }
    
    // Update is called once per frame
    void Update () {
        Vector2 targetDirection = target.position - transform.position;
        Vector2 targetPosition = (Vector2)transform.position + (targetDirection.normalized * targetDirection.magnitude * INTERP_SPEED * Time.deltaTime);
        //Debug.Log(targetPosition);
        targetPosition = Vector2.Lerp(transform.position, targetPosition, LERP_SPEED);
        transform.position = new Vector3(targetPosition.x, targetPosition.y, Z_POSITION);
        
    }

   
}
