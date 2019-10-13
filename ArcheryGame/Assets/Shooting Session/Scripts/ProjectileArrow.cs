using Assets.Script_Tools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class ProjectileArrow : MonoBehaviour
{
    [Tooltip("The layer that the arrow can collide with.")]
    [SerializeField] private LayerMask collisionsOnLayer;

    [Tooltip("The time it takes the camera to finish escorting the arrow after launch.")]
    [SerializeField] private float escortTime = 2.5f;

    [Tooltip("Pitch the arrow by a fixed angle once per frame (arond the 'x' axis).")]
    [SerializeField] [Range(0, 45f)] private float pitchAngle = 13f;

    [Tooltip("Spin the arrow by a fixed angle once per frame (arond the 'z' axis).")]
    [SerializeField] [Range(0, 359f)] private float spinAngle = 10;

    [Tooltip("Yaw the arrow back and forth by a fixed angle once per frame (around the 'y' axis).")]
    [SerializeField] [Range(0, 10f)] private float yawAngle = 1;

    [Tooltip("The amount of frames taken to yaw the arrow once (to one side).")]
    [SerializeField] [Range(0, 5)] private int yawRate = 24;

    private readonly long MAX_FLIGHT_DISTANCE = 100000;
    private readonly int NORMAL_HEIGHT_SAMPLES = 10;

    private Rigidbody rigidBody;
    private CameraManager camManager;
    private GameObject arrowCamera;
    private ProjectileManager projManager;
    private ArrowNavigator navigator;
    private ShootingSessionManager shootManager;
    private Quaternion lastRotation;
    private Vector3 hitPoint, lastPosition;
    private List<float> sampledHeights;
    private float normalHeight, timeRemainder;
    private bool hit, freeFlight, yawFlag;
    private int yawCounter;

    private void OnEnable() {
        this.sampledHeights = new List<float>();
        this.normalHeight = -1;
        this.timeRemainder = escortTime;
        this.lastRotation = transform.rotation;
        this.lastPosition = transform.position;
        this.yawCounter = 0;
        this.yawFlag = true;

        //enable the arrow's collider
        this.rigidBody = GetComponent<Rigidbody>();
        rigidBody.velocity = Vector3.zero;

        //switch to the arrow camera view
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        this.arrowCamera = ObjectFinder.GetChild(gameObject, "Camera");
        this.projManager = monitor.GetComponent<ProjectileManager>();
        this.shootManager = monitor.GetComponent<ShootingSessionManager>();
        this.camManager = monitor.GetComponent<CameraManager>();
        shootManager.EnterCamAnimation(true);

        //generate a hit point
        float length = GetComponent<MeshRenderer>().bounds.size.z;
        this.hitPoint = GenerateHitPoint();

        //shorten distance by the projectile length and thrust perecent
        //hitPoint += (thrust - length) * directionVector;
        
        //generate a physical navigator
        RotateArrow(hitPoint);
        this.navigator = GetComponent<ArrowNavigator>();
        navigator.GenerateSphere(transform.position, hitPoint, length);
    }

    private void Update() {
        navigator.Align();

        if (!hit) {
            if (navigator.Navigate(Time.deltaTime)) Freeze(null); //reached hit point
            Spin();
            Yaw();
        }
        //observe the arrow for a while after it hit the target
        if (hit || freeFlight) {
            timeRemainder -= Time.deltaTime;
            if (timeRemainder <= 0) Finish();
        }

        //PreventArrowJump(); //prevent strange transform spikes

        //save last transform properties
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    /// <summary>
    /// Destory unnecessary components of the arrow and progress the shooting round.
    /// </summary>
    private void Finish() {
        shootManager.ExitCamAnimation(true, CameraEnabler.Tag.FirstPerson, false);
        camManager.DestroyCam(arrowCamera);
        projManager.DestroyLastSpawned();
        enabled = false;
    }

    private void OnCollisionEnter(Collision collision) {
        print("collision");
        Freeze(collision.collider);
    }

    private void OnTriggerEnter(Collider collider) {
        Freeze(collider);
    }

    private void Freeze(Collider collider) {
        if (!hit && CollisionIsAllowed(collider, collisionsOnLayer.value)) {
            hit = true;

            //let other objects sense the arrow's collider
            GetComponent<BoxCollider>().isTrigger = false;

            //stick to the target
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            transform.rotation = lastRotation;

            //free flight arrow gets extra escort time
            if (freeFlight) timeRemainder = escortTime;
        }
    }

    private void Exhaust() {
        float alpha = pitchAngle;
        Vector3 difference = transform.position - lastPosition;
        Vector3 lookPoint = new Vector3(0,
                                        difference.z * Mathf.Tan(alpha),
                                        difference.z - difference.z / Mathf.Cos(alpha));

        transform.rotation = Quaternion.LookRotation(transform.position + lookPoint, Vector3.up);   
    }

    private void Spin() {
        transform.Rotate(0, 0, spinAngle);
    }

    private void Yaw() {
        yawCounter++;

        if (yawCounter >= yawRate) {
            int direction = yawFlag ? -1 : 1;
            transform.Rotate(0, yawAngle * direction, 0);

            yawFlag = !yawFlag;
            yawCounter = 0;
        }
    }

    /// <summary>
    /// Find the target that the sight is pointing at, and rotate towards it.
    /// Prevent the arrow from dramatically changing its 'y' axis position.
    /// </summary>
    private void PreventArrowJump() {
        float lastHeight = lastPosition.y;

        if (normalHeight != -1 && Mathf.Abs(transform.position.y - lastHeight) > normalHeight)
            transform.position = new Vector3(transform.position.x, lastHeight, transform.position.z);
        else {
            //add last height to the list of sampled heights
            if (sampledHeights.Count < NORMAL_HEIGHT_SAMPLES) sampledHeights.Add(lastHeight);

            //calculate average height using the samples
            else if (normalHeight == -1) {
                for (int i = 1; i < NORMAL_HEIGHT_SAMPLES; i++)
                    normalHeight += Mathf.Abs(sampledHeights[i] - sampledHeights[i - 1]);

                normalHeight /= NORMAL_HEIGHT_SAMPLES;
            }
        }
    }

    /// <summary>
    /// Find the exact point that the sight is pointing at.
    /// </summary>
    /// <returns>The point that the arrow should hit.</returns>
    private Vector3 GenerateHitPoint() {
        //reference the first person camera
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        CameraManager camManager = monitor.GetComponent<CameraManager>();
        Transform camTransform = camManager.firstPersonCam.transform;

        //shoot a straight ray from the center of the camera
        Ray ray = new Ray(camTransform.position, camTransform.forward);
        Vector3 point;

        //find direction
        bool rayCast = Physics.Raycast(ray, out RaycastHit rayHit, Mathf.Infinity, collisionsOnLayer);

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
    private void RotateArrow(Vector3 hitPoint) {
        Vector3 direction = (hitPoint - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        transform.Rotate(0, -yawAngle / 2, 0); //start yawing from the left
    }

    /// <summary>
    /// Check if a collision is made with the right layer,
    /// and also that the collider isn't set to trigger.
    /// </summary>
    /// <param name="collision">The collider to check</param>
    /// <param name="layer">Allowed layer</param>
    /// <returns>True if the coliision is legal.</returns>
    private bool CollisionIsAllowed(Collider collider, int layer) {
        if (collider == null) return true;

        bool notTrigger = !collider.isTrigger;
        return notTrigger && (1 << collider.gameObject.layer) == layer;
    }
}