using UnityEngine;
using UnityEngine.SocialPlatforms;

public class ProjectileArrow : MonoBehaviour
{
    ///<header>Collision Constraints</header>
    [Header("Collision Detection:")]

    [Tooltip("The layer that the arrow can collide with.")]
    [SerializeField] private LayerMask collideOnLayers;

    [Tooltip("The percent of the arrow that will stuck in the target.")]
    [SerializeField] [Range(0, 1f)] private float thrustDepth = .15f;

    ///<header>Camera Constraints</header>
    [Header("Camera:")]

    [Tooltip("The time it takes the camera to finish escorting the arrow after launch.")]
    [SerializeField] private float escortTime = 2;

    ///<header>Force Constraints</header>
    [Header("Force:")]

    [Tooltip("The minimum force the arrow can be launched at (as a function of distance).")]
    [SerializeField] private float minForce = 30;

    [Tooltip("The maximum force the arrow can be launched at (as a function of distance).")]
    [SerializeField] private float maxForce = 100;

    ///<header>Turbulence Constaints</header>
    [Header("Turbulence and Deviation:")]

    [Tooltip("Spin the arrow by a fixed angle once per frame (arond the 'z' axis).")]
    [SerializeField] [Range(0, 359f)] private float spinAngle = 10;

    [Tooltip("Yaw the arrow back and forth by a fixed angle once per frame (over the 'y' axis).")]
    [SerializeField] [Range(0, 10f)] private float yawAngle = 2;

    [Tooltip("The amount of frames taken to yaw the arrow once (to one side).")]
    [SerializeField] [Range(0, 5)] private int yawRate = 3;

    [Tooltip("Deviation of the arrow's flight caused by an external force.")]
    [SerializeField] private Vector2 externalForce;

    private readonly string PARENT_OBJECT_NAME = "Projectile Container";
    private readonly string SPHERE_NAVIGATOR_NAME = "Navigator";
    private readonly long DISTANCE_OF_MAX_FORCE = 200;
    private readonly float MAX_FORCE_DECAY_PERCENT = 30;
    private readonly float ANCHOR_SCALE = .03f;

    private Rigidbody rigidBody;
    private CameraManager camManager;
    private GameObject arrowCamera, windEffect, carrier, navigator, anchor;
    private ProjectileManager projManager;
    private ShootingSessionManager shootManager;
    private Quaternion lastRotation;
    private Vector3 launchPoint;
    private float arrowLength, timeRemainder;
    private float launchAngle, finalAngle;
    private float force, distance, pathPercent;
    private bool hit, freeFlight, yawFlag;
    private int yawCounter;

    private void OnEnable() {
        //check that minimum and maximum force are legal values
        if (minForce > maxForce) maxForce = minForce + 1;

        this.rigidBody = GetComponent<Rigidbody>();
        this.timeRemainder = escortTime;
        this.lastRotation = transform.rotation;
        this.launchPoint = transform.position;
        this.pathPercent = 0;

        //turbulence
        this.yawCounter = 0;
        this.yawFlag = true;
        this.windEffect = transform.Find("Wind Trail").gameObject;
        windEffect.SetActive(true);

        //switch to the arrow camera view
        GameObject monitor = GameObject.FindGameObjectWithTag("Monitor");
        this.arrowCamera = transform.Find("Camera").gameObject;
        this.projManager = monitor.GetComponent<ProjectileManager>();
        this.shootManager = monitor.GetComponent<ShootingSessionManager>();
        this.camManager = monitor.GetComponent<CameraManager>();
        shootManager.EnterCamAnimation(true);

        //generate a navigator
        this.arrowLength = GetComponent<MeshRenderer>().bounds.size.z;
        Vector3 hitPoint = GenerateHitPoint();
        this.distance = Vector3.Distance(transform.position, hitPoint);
        this.force = CalculateForce(distance);

        RotateArrow(hitPoint);
        InitNavigationUnit(hitPoint);
        this.launchAngle = transform.eulerAngles.x;
        if (launchAngle > 90) launchAngle -= 360;
        this.finalAngle = CalculateFinalAngle(launchAngle, force);
    }

    /// <summary>
    /// Initialize the objects that navigate the arrow around the scene.
    /// </summary>
    /// <param name="hitPoint">The point that the arrow should eventually hit</param>
    private void InitNavigationUnit(Vector3 hitPoint) {
        this.carrier = new GameObject(PARENT_OBJECT_NAME);
        carrier.transform.position = transform.position;
        carrier.transform.LookAt(hitPoint);
        transform.SetParent(carrier.transform);

        this.navigator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        navigator.name = SPHERE_NAVIGATOR_NAME;
        navigator.GetComponent<SphereCollider>().enabled = false;
        navigator.GetComponent<MeshRenderer>().enabled = false;
        navigator.transform.localScale *= distance * 2;
        navigator.transform.position = transform.position;
        navigator.transform.LookAt(hitPoint);
        navigator.transform.SetParent(carrier.transform);

        this.anchor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        anchor.GetComponent<SphereCollider>().enabled = false;
        anchor.GetComponent<MeshRenderer>().enabled = false;
        anchor.transform.localScale *= ANCHOR_SCALE;
        anchor.transform.position = hitPoint;
        anchor.transform.SetParent(navigator.transform);
    }

    private void Update() {
        if (!hit) {
            Navigate(Time.deltaTime);
            navigator.transform.localPosition = transform.localPosition; //align navigator
            Pitch();
            Spin();
            Yaw();
        }
        //observe the arrow for a while after it hit the target
        if (hit || freeFlight) {
            timeRemainder -= Time.deltaTime;
            if (timeRemainder <= 0) Finish();
        }

        lastRotation = transform.rotation;
    }

    /// <summary>
    /// Move the arrow around the scene towards the hit point.
    /// </summary>
    /// <param name="delta">Frame rate delta time</param>
    private void Navigate(float delta) {
        //move the arrow towards the anchor and save the distance it made
        Vector3 anchorPoint = anchor.transform.position;
        Vector3 lastPosition = carrier.transform.position;
        float currentDistance = Vector3.Distance(lastPosition, anchorPoint);
        float decayedforce = DecayForce(delta, force, CalculateTargetDistance());
        carrier.transform.position = Vector3.MoveTowards(lastPosition, anchorPoint, delta * decayedforce);
        pathPercent = CalculatePathPercent(currentDistance);

        //check if the arrow is yet to reach the anchor point
        if (!HasReachPoint(launchPoint, carrier.transform.position, anchorPoint)) {
            float diff = Vector3.Distance(carrier.transform.position, lastPosition) * 2;
            Vector3 diffVector = Vector3.one * diff;

            //relatively scale down the navigator sphere
            float thrustedArrowLength = arrowLength * (2 - (1 - thrustDepth));
            if (navigator.transform.localScale.x > thrustedArrowLength)
                navigator.transform.localScale -= diffVector;
        }
    }

    /// <summary>
    /// Calculate the current force of the arrow, considering the remaining distance from the hit point.
    /// </summary>
    /// <param name="delta">Frame rate delta time</param>
    /// <param name="force">The starting launch force</param>
    /// <param name="currentDistance">Current distance from the hit point</param>
    /// <returns>A decreased flight force.</returns>
    private float DecayForce(float delta, float force, float currentDistance) {
        float distanceTrigger = arrowLength * 2; //longest distance that's not allowed

        if (currentDistance <= distanceTrigger || CalculateTargetDistance() <= distanceTrigger) {
            float thrustLength = Mathf.Clamp(thrustDepth, .2f, thrustDepth) * arrowLength;
            float remainDistance = currentDistance - arrowLength + thrustLength;
            return remainDistance / delta; //neutralize the delta parameter to move the exact distance
        }
        else {
            float remainForce = 100 - (pathPercent * MAX_FORCE_DECAY_PERCENT / 100);
            return force * remainForce / 100;
        }
    }

    /// <returns>The distance from the closest target that the arrow is about to hit</returns>
    private float CalculateTargetDistance() {
        Vector3 forwardVector = Vector3.Normalize(anchor.transform.position - transform.position);
        Ray ray = new Ray(transform.position, forwardVector);
        bool rayCast = Physics.Raycast(ray, out RaycastHit rayHit, distance, collideOnLayers);

        if (rayCast && rayHit.collider != null) return rayHit.distance;
        else return navigator.transform.localScale.x;
    }

    /// <returns>The percentage of the total distance that the arrow has already passed.</returns>
    private float CalculatePathPercent(float currentDistance) {
        float passedDistance = distance - currentDistance;
        return passedDistance / distance * 100;
    }

    /// <summary>
    /// Calculate the force needed to launch the arrow considering the distance it's ought to fly.
    /// </summary>
    /// <param name="distance">The distance to the hit point</param>
    /// <returns>A starting force for the arrow.</returns>
    private float CalculateForce(float distance) {
        float distancePercent = Mathf.Clamp(distance / DISTANCE_OF_MAX_FORCE * 100, 0, 250);
        return (distancePercent * (maxForce - minForce) / 100) + minForce;
    }

    /// <summary>
    /// Calculate the angle that the arrow should eventually land at.
    /// </summary>
    /// <param name="launchAngle">The angle in which the arrow had been launched at</param>
    /// <returns>The angle that the arrow should land at.</returns>
    private float CalculateFinalAngle(float launchAngle, float force) {
        float angleRange;

        if (launchAngle < 0) {
            float maximalAngle = -launchAngle;
            angleRange = Mathf.Abs(launchAngle) + maximalAngle;
        }
        else {
            float maximalAngle = launchAngle * 2;
            angleRange = maximalAngle - launchAngle;
        }

        float forcePercent = (force - minForce) / (maxForce - minForce) * 100;
        forcePercent = Mathf.Clamp(forcePercent, 0, 100);
        float angleAddition = forcePercent * angleRange / 100;
        return launchAngle + angleAddition;
    }

    /// <summary>
    /// Rotate the arrow around the 'x' axis.
    /// </summary>
    private void Pitch() {
        float pitchRange;
        if (launchAngle < 0) pitchRange = Mathf.Abs(launchAngle) + finalAngle;
        else pitchRange = finalAngle - launchAngle;
        float pitchAddition = pathPercent * pitchRange / 100;
        float newPitch = launchAngle + pitchAddition;

        //rotate navigator - for the arrow to fall down
        navigator.transform.rotation = Quaternion.Euler(newPitch,
                                                        navigator.transform.eulerAngles.y,
                                                        navigator.transform.eulerAngles.z);

        //rotate the arrow - for the pitch effect
        transform.rotation = Quaternion.Euler(newPitch,
                                              transform.eulerAngles.y,
                                              transform.eulerAngles.z);
    }

    /// <summary>
    /// Rotate the arrow around the 'z' axis.
    /// </summary>
    private void Spin() {
        transform.Rotate(0, 0, spinAngle);
    }

    /// <summary>
    /// Yaw the arrow back and forth over the 'y' axis.
    /// </summary>
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
    /// Find the exact point that the sight is pointing at.
    /// </summary>
    /// <returns>The point that the arrow should hit.</returns>
    private Vector3 GenerateHitPoint() {
        //reference the first person camera
        CameraManager camManager = FindObjectOfType<CameraManager>();
        Transform camTransform = camManager.GetCam(CameraEnabler.Tag.FirstPerson).transform;
        Vector3 viewerPos = camTransform.position + (Vector3) externalForce;

        //shoot a straight ray from the center of the camera
        Ray ray = new Ray(viewerPos, camTransform.forward);
        bool rayCast = Physics.Raycast(ray, out RaycastHit rayHit, Mathf.Infinity, collideOnLayers);
        Vector3 point;

        //known direction
        if (rayCast && rayHit.collider != null) point = rayHit.point; 
        else { //free flight
            point = ray.GetPoint(DISTANCE_OF_MAX_FORCE);
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
    /// Chech if the arrow has reached its final hit point.
    /// </summary>
    /// <param name="sourcePoint">The point from which the arrow had been launched</param>
    /// <param name="pointA">The current position of the arrow</param>
    /// <param name="pointB">The current position of the final hit point</param>
    /// <returns>True if the arrow has reached the final hit point.</returns>
    private bool HasReachPoint(Vector3 sourcePoint, Vector3 pointA, Vector3 pointB) {
        float originDistance = Vector3.Distance(sourcePoint, pointB);
        float passedDistance = Vector3.Distance(sourcePoint, pointA);
        return passedDistance >= originDistance;
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

    private void OnCollisionEnter(Collision collision) {
        Freeze(collision.collider);
    }

    private void OnTriggerEnter(Collider collider) {
        Freeze(collider);
    }

    /// <summary>
    /// Freeze the arrow in its current position and rotation.
    /// This method is irreversible.
    /// </summary>
    /// <param name="collider">The collider that the arrow hit (null if it hit nothing)</param>
    private void Freeze(Collider collider) {
        if (!hit && CollisionIsAllowed(collider, collideOnLayers.value)) {
            hit = true;

            //let other objects sense the arrow's collider
            GetComponent<BoxCollider>().isTrigger = false;

            //stick to the target
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            transform.rotation = lastRotation;
            windEffect.SetActive(false);

            //free flight arrow gets extra escort time
            if (freeFlight) timeRemainder = escortTime;
        }
    }

    /// <summary>
    /// Destory unnecessary components of the arrow and progress the shooting round.
    /// </summary>
    private void Finish() {
        shootManager.ExitCamAnimation(true, CameraEnabler.Tag.FirstPerson);
        camManager.DestroyCam(arrowCamera);
        projManager.DestroyLastSpawned();
        enabled = false;
    }
}