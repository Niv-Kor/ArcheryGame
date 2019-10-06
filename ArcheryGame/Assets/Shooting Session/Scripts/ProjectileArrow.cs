using Assets.Script_Tools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class ProjectileArrow : MonoBehaviour
{
    [Tooltip("The force in which the arrow is launched")]
    [SerializeField] private float defaultForce = 100;

    [Tooltip("The time it takes the camera to finish escorting the arrow after launch")]
    [SerializeField] private float escortTime = 2.5f;

    [Tooltip("The percent of the arrow that will stuck in the target")]
    [SerializeField] [Range(0, 1)] private float thrustDepthPercent = .15f;

    [Tooltip("The layer that the arrow can collide with")]
    [SerializeField] private LayerMask collisionsOnLayer;

    [Tooltip("The angle deviation of the arrow compared to the sight")]
    [SerializeField] private Vector3 sightDeviation;

    private readonly float MAX_FLIGHT_DISTANCE = 10_000;
    private readonly int NORMAL_HEIGHT_SAMPLES = 10;

    private Rigidbody rigidBody;
    private CameraManager camManager;
    private GameObject shaft, arrowCamera;
    private ProjectileManager projManager;
    private ShootingSessionManager shootManager;
    private Quaternion lastRotation;
    private Vector3 hitPoint;
    private float timeRemainder;
    private bool hit, freeFlight;

    private List<float> sampledHeights;
    private float lastHeight, normalHeight;

    private void OnEnable() {
        this.sampledHeights = new List<float>();
        this.normalHeight = -1;
        this.timeRemainder = escortTime;
        this.lastRotation = transform.rotation;

        //enable the arrow's collider
        GetComponent<BoxCollider>().enabled = true;
        this.rigidBody = GetComponent<Rigidbody>();
        rigidBody.velocity = Vector3.zero;
        rigidBody.isKinematic = false;

        //switch to the arrow camera view
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        this.arrowCamera = ObjectFinder.GetChild(gameObject, "Camera");
        this.projManager = monitor.GetComponent<ProjectileManager>();
        this.shootManager = monitor.GetComponent<ShootingSessionManager>();
        this.camManager = monitor.GetComponent<CameraManager>();
        shootManager.EnterCamAnimation(true);

        //rotate the arrow at the direction of the sight
        this.hitPoint = GetHitPoint();
        RotateArrow(hitPoint);

        //shorten hit distance by the length of the arrow
        this.shaft = ObjectFinder.GetChild(gameObject, "Shaft");
        float arrowLength = shaft.GetComponent<MeshRenderer>().bounds.size.z * thrustDepthPercent;
        Vector3 directionVector = (hitPoint - transform.position).normalized;
        hitPoint += directionVector * arrowLength;
    }

    private void Update() {
        if (!hit) {
            lastRotation = transform.rotation;
            transform.position = Vector3.MoveTowards(transform.position, hitPoint, Time.deltaTime * defaultForce);
            //transform.rotation = Quaternion.LookRotation(rigidbody.velocity); //pitch
            //Pitch();
        }
        //observe the arrow for a while after it hit the target
        else {
            timeRemainder -= Time.deltaTime;
            if (timeRemainder <= 0) Finish();
        }

        PreventArrowJump();
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

    private void OnCollisionEnter(Collision collision) {
        if (!hit && CollisionIsAllowed(collision)) {
            hit = true;

            //detect score
            GetComponent<CapsuleCollider>().enabled = true; //enable mesh collider for easier detection
            ScoreManager[] scoreManagers = FindObjectsOfType<ScoreManager>();
            foreach (ScoreManager scoreMngr in scoreManagers) scoreMngr.CheckScore();

            //stick to the target
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            rigidBody.isKinematic = true;
            transform.rotation = lastRotation;

            if (freeFlight) timeRemainder = escortTime;
        }
    }

    /*private void Pitch() {
        velocity = lastPosition - transform.position;
        transform.LookAt(transform.position - velocity);
        hitPoint.y += velocity.y;
        print("velocity: " + velocity);
    }*/

    /// <summary>
    /// Find the target that the sight is pointing at, and rotate the arrow towards it.
    /// Prevent the arrow from dramatically changing its 'y' axis position during flight.
    /// </summary>
    private void PreventArrowJump() {
        if (normalHeight != -1 && Mathf.Abs(transform.position.y - lastHeight) > normalHeight)
            transform.position = new Vector3(transform.position.x, lastHeight, transform.position.z);
        else {
            lastHeight = transform.position.y;

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
    private Vector3 GetHitPoint() {
        //reference the first person camera
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        CameraManager camManager = monitor.GetComponent<CameraManager>();
        Transform camTransform = camManager.firstPersonCam.transform;

        //shoot a straight ray from the center of the camera
        Vector3 camRotation = camTransform.forward + new Vector3(sightDeviation.x, sightDeviation.y, 0);
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
    private void RotateArrow(Vector3 hitPoint) {
        Vector3 direction = (hitPoint - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private bool CollisionIsAllowed(Collision collision) {
        return (1 << collision.gameObject.layer) == collisionsOnLayer.value;
    }
}


/*using Assets.Script_Tools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class ProjectileArrow : MonoBehaviour
{
    [Tooltip("The force in which the arrow is launched")]
    [SerializeField] private float defaultForce = 100;

    [Tooltip("The time it takes the camera to finish escorting the arrow after launch")]
    [SerializeField] private float escortTime = 2.5f;

    [Tooltip("The percent of the arrow that will stuck in the target")]
    [SerializeField] [Range(0, 1)] private float thrustDepthPercent = .15f;

    [Tooltip("The layer that the arrow can collide with")]
    [SerializeField] private LayerMask collisionsOnLayer;

    [Tooltip("The angle deviation of the arrow compared to the sight")]
    [SerializeField] private Vector3 sightDeviation;

    private readonly float MAX_FLIGHT_DISTANCE = 10_000;
    private readonly int NORMAL_HEIGHT_SAMPLES = 10;

    private Rigidbody rigidBody;
    private Collider[] collisionResults;
    private CameraManager camManager;
    private GameObject arrowHead, arrowCamera;
    private ProjectileManager projManager;
    private ShootingSessionManager shootManager;
    private Quaternion lastRotation;
    private float timeRemainder;
    private bool hit, freeFlight;

    private List<float> sampledHeights;
    private float lastHeight, normalHeight;
    private Vector3 lastPosition;

    private void OnEnable() {
        this.sampledHeights = new List<float>();
        this.normalHeight = -1;

        this.timeRemainder = escortTime;
        this.lastRotation = transform.rotation;
        this.collisionResults = new Collider[100];

        //switch to the arrow camera view
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        this.arrowCamera = ObjectFinder.GetChild(gameObject, "Camera");
        this.arrowHead = ObjectFinder.GetChild(gameObject, "Head");
        this.camManager = monitor.GetComponent<CameraManager>();
        this.projManager = monitor.GetComponent<ProjectileManager>();

        //enable the arrow's collider
        GetComponent<CapsuleCollider>().enabled = true;
        this.rigidBody = GetComponent<Rigidbody>();
        rigidBody.isKinematic = false;
        
        //disable mouse look
        this.shootManager = monitor.GetComponent<ShootingSessionManager>();
        shootManager.EnterCamAnimation(true);

        //rotate the arrow at the direction of the sight
        Vector3 hitPoint = GetHitPoint();
        RotateArrow(hitPoint);

        //fire the arrow
        rigidBody.velocity = arrowCamera.transform.forward * defaultForce;
    }

    private void FixedUpdate() {
        if (!hit) {
            lastPosition = transform.position;
            if (!hit) lastRotation = transform.rotation; //save rotation for later
            else transform.rotation = lastRotation;

            transform.LookAt(transform.position + rigidBody.velocity); //pitch arrow
            PreventArrowJump();
        }
        //observe the arrow for a while after it hit the target
        if (hit || freeFlight) {
            timeRemainder -= Time.deltaTime;
            if (timeRemainder <= 0) Finish();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (!hit) {
            int collides = Physics.OverlapSphereNonAlloc(arrowHead.transform.position, .2f, collisionResults, collisionsOnLayer.value);
            if (collides > 0) {
                print("collide");
                hit = true;

                //stick to the target
                rigidBody.constraints = RigidbodyConstraints.FreezeAll;
                rigidBody.isKinematic = true;

                //show the arrow a little more before switching cameras
                if (freeFlight) timeRemainder = escortTime;

                //restore lost rotation due to the hit
                transform.rotation = lastRotation;
            }
        }
    }

    private void PreventArrowJump() {
        if (normalHeight != -1 && Mathf.Abs(transform.position.y - lastHeight) < normalHeight)
            transform.position = new Vector3(transform.position.x, lastHeight, transform.position.z);
        else {
            lastHeight = transform.position.y;

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
    private Vector3 GetHitPoint() {
        //reference the first person camera
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        CameraManager camManager = monitor.GetComponent<CameraManager>();
        Transform camTransform = camManager.firstPersonCam.transform;

        //shoot a straight ray from the center of the camera
        Vector3 camRotation = camTransform.forward + new Vector3(sightDeviation.x, sightDeviation.y, 0);
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
    private void RotateArrow(Vector3 hitPoint) {
        Vector3 direction = (hitPoint - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
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
}*/
