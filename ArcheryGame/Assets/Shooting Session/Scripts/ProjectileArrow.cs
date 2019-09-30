using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Script_Tools;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using Object = System.Object;

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

    private readonly int NORMAL_HEIGHT_SAMPLES = 10;

    private Rigidbody rigidBody;
    private CameraManager camManager;
    private GameObject arrowCamera;
    private ProjectileManager projManager;
    private ShootingSessionManager shootManager;
    private Vector3 hitPoint, velocity;
    private Quaternion arrowRotation;
    private List<float> sampledHeights;
    private float lastHeight, normalHeight;
    private Vector3 lastPosition;
    private bool hit;

    private void OnEnable() {
        this.sampledHeights = new List<float>();
        this.normalHeight = -1;
        this.velocity = transform.position;

        //enable the arrow's collider
        GetComponent<CapsuleCollider>().enabled = true;
        this.rigidBody = GetComponent<Rigidbody>();
        rigidBody.velocity = Vector3.zero;
        rigidBody.isKinematic = false;

        //switch to the arrow camera view
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        this.arrowCamera = ObjectFinder.GetChild(gameObject, "Arrow Camera");
        this.camManager = monitor.GetComponent<CameraManager>();
        this.projManager = monitor.GetComponent<ProjectileManager>();

        //disable mouse look
        this.shootManager = monitor.GetComponent<ShootingSessionManager>();
        shootManager.EnterCamAnimation(true);

        //rotate the arrow at the direction of the sight
        this.hitPoint = GetHitPoint();
        this.arrowRotation = RotateArrow(hitPoint);

        //shorten hit distance by the length of the arrow
        float arrowLength = GetComponent<MeshRenderer>().bounds.size.z * (1 - thrustDepthPercent);
        Vector3 directionVector = (hitPoint - transform.position).normalized;
        hitPoint -= directionVector * arrowLength;
    }

    private void Update() {
        if (!hit) {
            lastPosition = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, hitPoint, Time.deltaTime * defaultForce);
            Pitch();
            //PreventArrowJump();
            
        }
        //observe the arrow for a while after it hit the target
        else {
            escortTime -= Time.deltaTime;
            if (escortTime <= 0) Finish();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (!hit && CollisionIsAllowed(collision)) {
            hit = true;

            //stick to the target
            transform.rotation = arrowRotation;
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            rigidBody.isKinematic = true;
        }
    }

    private void Pitch() {
        velocity = lastPosition - transform.position;
        transform.LookAt(transform.position - velocity);
        hitPoint.y += velocity.y;
        print("velocity: " + velocity);
    }

    /// <summary>
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
        Transform camTransform = Camera.main.transform;
        Vector3 camRotation = camTransform.forward + new Vector3(sightDeviation.x, sightDeviation.y, 0f);
        Ray ray = new Ray(camTransform.position, camRotation);
        float distance = defaultForce;
        Vector3 point;

        //find direction
        if (Physics.Raycast(ray, out RaycastHit rayHit, Mathf.Infinity, collisionsOnLayer) && rayHit.collider != null) {
            point = rayHit.point;
        }
        else {
            point = ray.GetPoint(distance);
            hit = true;
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