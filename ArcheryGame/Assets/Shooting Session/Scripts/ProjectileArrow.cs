using Assets.Script_Tools;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class ProjectileArrow : MonoBehaviour
{
    [Tooltip("The force in which the arrow is launched")]
    [SerializeField] private float defaultForce = 100f;

    [Tooltip("The time it takes the camera to finish escorting the arrow after launch")]
    [SerializeField] private float escortTime = 2.5f;

    [Tooltip("The percent of the arrow that will stuck in the target")]
    [SerializeField] [Range(0, 1)] private float thrustDepthPercent = .15f;

    [Tooltip("The layer that the arrow can collide with")]
    [SerializeField] private LayerMask collisionsOnLayer;

    [Tooltip("The angle deviation of the arrow compared to the sight")]
    [SerializeField] private Vector3 sightDeviation;

    private readonly float MAX_FLIGHT_DISTANCE = 10_000;

    private Rigidbody rigidBody;
    private CameraManager camManager;
    private GameObject arrowCamera;
    private ProjectileManager projManager;
    private ShootingSessionManager shootManager;
    private Vector3 hitPoint, velocity, lastPosition;
    private Quaternion arrowRotation;
    private float timeRemainder;
    private bool hit, freeFlight;

    private void OnEnable() {
        this.velocity = transform.position;
        this.timeRemainder = escortTime;

        //switch to the arrow camera view
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        this.arrowCamera = ObjectFinder.GetChild(gameObject, "Camera");
        this.camManager = monitor.GetComponent<CameraManager>();
        this.projManager = monitor.GetComponent<ProjectileManager>();

        //enable the arrow's collider
        GetComponent<CapsuleCollider>().enabled = true;
        this.rigidBody = GetComponent<Rigidbody>();
        rigidBody.velocity = Vector3.zero;
        rigidBody.isKinematic = false;

        //disable mouse look
        this.shootManager = monitor.GetComponent<ShootingSessionManager>();
        shootManager.EnterCamAnimation(true);

        //rotate the arrow at the direction of the sight
        this.hitPoint = GetHitPoint();
        this.arrowRotation = RotateArrow(hitPoint);

        //shorten hit distance by the length of the arrow
        GameObject shaft = ObjectFinder.GetChild(gameObject, "Shaft");
        float arrowLength = shaft.GetComponent<MeshRenderer>().bounds.size.z * (1 - thrustDepthPercent);
        Vector3 directionVector = (hitPoint - transform.position).normalized;
        hitPoint += directionVector * arrowLength;
    }

    private void Update() {
        if (!hit) {
            lastPosition = transform.position; //save last position for the pitch method
            transform.position = Vector3.MoveTowards(transform.position, hitPoint, Time.deltaTime * defaultForce);
            //Pitch();
        }
        //observe the arrow for a while after it hit the target
        if (hit || freeFlight) {
            timeRemainder -= Time.deltaTime;
            if (timeRemainder <= 0) Finish();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (!hit && CollisionIsAllowed(collision)) {
            hit = true;
            
            //maintain the same angle on the 'y' and 'z' axes
            /*transform.eulerAngles = new Vector3(arrowRotation.eulerAngles.x,
                                                arrowRotation.eulerAngles.y,
                                                arrowRotation.eulerAngles.z);*/

            //stick to the target
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            rigidBody.isKinematic = true;

            //show the arrow a little more before switching cameras
            if (freeFlight) timeRemainder = escortTime;
        }
    }

    /// <summary>
    /// Pitch the arrow (rotate on the 'x' axis) as it flies.
    /// </summary>
    private void Pitch() {                          
        velocity = lastPosition - transform.position;
        hitPoint.y += velocity.y;
        Vector3 direction = transform.position + new Vector3(-velocity.x, velocity.y * 2, -velocity.z);
        transform.LookAt(direction);
        arrowRotation = transform.rotation;
    }

    /// <summary>
    /// Find the exact point that the sight is pointing at.
    /// </summary>
    /// <returns>The point that the arrow should hit.</returns>
    private Vector3 GetHitPoint() {
        //reference the first person camera
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        CameraManager camManager = monitor.GetComponent<CameraManager>();
        Transform camTransform = camManager.firstPersonCam.transform;

        //shoot a straight ray from the center of the camera
        Vector3 camRotation = camTransform.forward + new Vector3(sightDeviation.x, sightDeviation.y, 0f);
        Ray ray = new Ray(camTransform.position, camRotation);
        float distance = Mathf.Infinity;
        Vector3 point;

        //find direction
        bool rayCast = Physics.Raycast(ray, out RaycastHit rayHit, distance, collisionsOnLayer);

        if (rayCast && rayHit.collider != null) point = rayHit.point; //known direction
        else { //free flight
            point = ray.GetPoint(MAX_FLIGHT_DISTANCE);
            freeFlight = true;
            timeRemainder *= 2;
        }

        return point;
    }

    /// <summary>
    /// Rotate the arrow towards the point it should be hitting.
    /// </summary>
    /// <param name="hitPoint">The point that the arrow should hit</param>
    /// <returns>The exact rotation towards the hit point.</returns>
    private Quaternion RotateArrow(Vector3 hitPoint) {
        Vector3 direction = (hitPoint - transform.position).normalized;
        arrowRotation = Quaternion.LookRotation(direction);
        transform.rotation = arrowRotation;

        return arrowRotation;
    }

    /// <summary>
    /// Destory unnecessary components of the arrow and progress the shooting round.
    /// </summary>
    private void Finish() {
        //enable mouse look and switch to first person camera
        shootManager.ExitCamAnimation(true, CameraEnabler.Tag.FirstPerson, false);
        camManager.DestroyCam(arrowCamera);
        projManager.DestroyLastSpawned();

        enabled = false;
    }

    /// <summary>
    /// Check if the collision with a specific layer is allowed (as determined by "collisionsOnLayer").
    /// </summary>
    /// <param name="collision">The layer to check</param>
    /// <returns>True if the collision with that layer should not be ignored.</returns>
    private bool CollisionIsAllowed(Collision collision) {
        return (1 << collision.gameObject.layer) == collisionsOnLayer.value;
    }
}