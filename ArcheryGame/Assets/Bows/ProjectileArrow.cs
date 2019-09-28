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
    [SerializeField] private float noHitEscortTime = 2.5f;

    [Tooltip("The time it takes the camera to finish escorting the arrow after launch")]
    [SerializeField] private float escortTime = 2.5f;

    [Tooltip("The percent of the arrow that will stuck in the target")]
    [SerializeField] [Range(0, 1)] private float thrustDepthPercent = .15f;

    [Tooltip("The layer that the arrow can collide with")]
    [SerializeField] private LayerMask collisionsOnLayer;

    private Rigidbody rigidbody;
    private CameraManager camManager;
    private GameObject arrowCamera;
    private ProjectileManager projManager;
    private ShootingSessionManager shootManager;
    private Vector3 hitPoint;
    private float force;
    private bool hit;

    private void OnEnable() {
        //enable the arrow's collider
        GetComponent<CapsuleCollider>().enabled = true;
        this.rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.isKinematic = false;

        //switch to the arrow camera view
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        this.arrowCamera = ObjectFinder.GetChild(gameObject, "Arrow Camera");
        this.camManager = monitor.GetComponent<CameraManager>();
        this.projManager = monitor.GetComponent<ProjectileManager>();

        //disable mouse look
        this.shootManager = monitor.GetComponent<ShootingSessionManager>();
        shootManager.EnterCamAnimation(true);
        shootManager.EnterCamAnimation(true);

        //enable the arrow's collider
        GetComponent<CapsuleCollider>().enabled = true;
        this.rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.isKinematic = false;

        //rotate the arrow at the direction of the sight
        hitPoint = RotateTowardsTarget();

        //shorten hit distance by the length of the arrow
        float arrowLength = GetComponent<MeshRenderer>().bounds.size.z * (1 - thrustDepthPercent);
        Vector3 directionVector = (hitPoint - transform.position).normalized;
        hitPoint -= directionVector * arrowLength;
    }

    private void Update() {
        transform.position = Vector3.MoveTowards(transform.position, hitPoint, Time.deltaTime * defaultForce);
        //transform.rotation = Quaternion.LookRotation(rigidbody.velocity); //pitch
        if (!hit) {
            transform.position = Vector3.MoveTowards(transform.position, hitPoint, Time.deltaTime * defaultForce);
            //transform.rotation = Quaternion.LookRotation(rigidbody.velocity); //pitch
        }
        else {
            escortTime -= Time.deltaTime;
            if (escortTime <= 0) Finish();
        }
    }

    /// <summary>
    /// Find the target that the sight is pointing at, and rotate the arrow towards it.
    /// </summary>
    /// <returns>The point that the arrow should hit.</returns>
    private Vector3 RotateTowardsTarget() {
        Transform camTransform = Camera.main.transform;
        Ray ray = new Ray(camTransform.position, Camera.main.transform.forward);
        float distance = defaultForce;
        Vector3 point;

        //find direction
        if (Physics.Raycast(ray, out RaycastHit rayHit, Mathf.Infinity, collisionsOnLayer) && rayHit.collider != null) {
            point = rayHit.point;
            distance = rayHit.distance;
            print("hit " + rayHit.collider.gameObject.name);
        }
        else {
            print("not hit");
            point = ray.GetPoint(distance);
            hit = true;
        }

        force = distance;
        print("distance " + distance);

        //rotate
        Vector3 direction = (point - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);

        return point;
    }

    private void OnCollisionEnter(Collision collision) {
        print("colliding with game object " + collision.gameObject.name + " whose name is " + LayerMask.LayerToName(collision.gameObject.layer));

        if (!hit && CollisionIsAllowed(collision)) {
            hit = true;
            print("Arrow hit " + collision.gameObject.name + " (tagged as \"" + collision.gameObject.tag + "\")");

            //stick to the target
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            rigidbody.isKinematic = true;
        }
    }

    /// <summary>
    /// Destory unnecessary components of the arrow and progress the shooting round.
    /// </summary>
    private void Finish() {
        //enable mouse look and switch to first person camera
        shootManager.LoadArrow();
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