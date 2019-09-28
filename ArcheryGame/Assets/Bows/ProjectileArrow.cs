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
    [SerializeField] private float defaultForce = 100f;
    [SerializeField] private float noHitEscortTime = 2.5f;
    [SerializeField] private LayerMask collisionsOnLayer;

    [SerializeField] public Vector3 sightHit;

    private Rigidbody rigidBody;
    private CameraManager camManager;
    private GameObject arrowCamera;
    private ProjectileManager projManager;
    private ShootingSessionManager shootManager;
    private Vector3 hitPoint;
    private float force;
    private bool hit;

    private void OnEnable() {
<<<<<<< HEAD
<<<<<<< HEAD
        this.hit = false;

        //enable the arrow's collider
        GetComponent<CapsuleCollider>().enabled = true;
        this.rigidBody = GetComponent<Rigidbody>();
        rigidBody.velocity = Vector3.zero;
        rigidBody.isKinematic = false;

=======
>>>>>>> parent of dcc4155... Fixed arrow shooting and targets
=======
>>>>>>> parent of dcc4155... Fixed arrow shooting and targets
        //switch to the arrow camera view
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        this.arrowCamera = ObjectFinder.GetChild(gameObject, "Arrow Camera");
        this.camManager = monitor.GetComponent<CameraManager>();
        this.projManager = monitor.GetComponent<ProjectileManager>();

<<<<<<< HEAD
<<<<<<< HEAD
        //rotate the arrow at the direction of the sight
        this.hitPoint = RotateTowardsTarget();

        //shorten hit distance by the length of the arrow
        float arrowLength = GetComponent<MeshRenderer>().bounds.size.z * (1 - thrustDepthPercent);
        Vector3 directionVector = (hitPoint - transform.position).normalized;
        hitPoint -= directionVector * arrowLength;

        //disable mouse look
        this.shootManager = monitor.GetComponent<ShootingSessionManager>();
        shootManager.EnterCamAnimation(true);
    }

    private void Update() {
        if (!hit) {
            transform.position = Vector3.MoveTowards(transform.position, hitPoint, Time.deltaTime * defaultForce);
            //transform.rotation = Quaternion.LookRotation(rigidbody.velocity); //pitch
        }
        else {
            escortTime -= Time.deltaTime;
            if (escortTime <= 0) Finish();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (!hit && CollisionIsAllowed(collision)) {
            hit = true;

            //stick to the target
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            rigidBody.isKinematic = true;
=======
        //disable mouse look
        this.shootManager = monitor.GetComponent<ShootingSessionManager>();
        shootManager.EnterCamAnimation(true);

        //enable the arrow's collider
        GetComponent<CapsuleCollider>().enabled = true;
        this.rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.isKinematic = false;

        //launch the arrow at the direction of the sight
        RotateTowardsTarget(out force, out hitPoint);
    }

    private void Update() {
=======
        //disable mouse look
        this.shootManager = monitor.GetComponent<ShootingSessionManager>();
        shootManager.EnterCamAnimation(true);

        //enable the arrow's collider
        GetComponent<CapsuleCollider>().enabled = true;
        this.rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.isKinematic = false;

        //launch the arrow at the direction of the sight
        RotateTowardsTarget(out force, out hitPoint);
    }

    private void Update() {
>>>>>>> parent of dcc4155... Fixed arrow shooting and targets
        transform.position = Vector3.MoveTowards(transform.position, hitPoint, Time.deltaTime * defaultForce);
        //transform.rotation = Quaternion.LookRotation(rigidbody.velocity); //pitch

        if (hit) {
            noHitEscortTime -= Time.deltaTime;
            if (noHitEscortTime <= 0) Finish();
<<<<<<< HEAD
>>>>>>> parent of dcc4155... Fixed arrow shooting and targets
=======
>>>>>>> parent of dcc4155... Fixed arrow shooting and targets
        }
    }

    private void RotateTowardsTarget(out float force, out Vector3 hitPoint) {
        Transform camTransform = Camera.main.transform;
        print("cam: " + Camera.main.name);
        Ray ray = new Ray(camTransform.position, camTransform.forward);
        Vector3 hitP = Vector3.zero;

        //find direction
<<<<<<< HEAD
        if (Physics.Raycast(ray, out RaycastHit rayHit) && rayHit.collider != null) {
            hitP = rayHit.point;
=======
        if (Physics.Raycast(ray, out RaycastHit rayHit, Mathf.Infinity, collisionsOnLayer) && rayHit.collider != null) {
            hitPoint = rayHit.point;
            distance = rayHit.distance;
            print("hit " + rayHit.collider.gameObject.name);
        }
        else {
            print("not hit");
            hitPoint = ray.GetPoint(distance);
            hit = true;
>>>>>>> parent of dcc4155... Fixed arrow shooting and targets
        }

        force = distance;
        print("distance " + distance);

        //rotate
        Vector3 direction = (hitP - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }

<<<<<<< HEAD
<<<<<<< HEAD
        return hitP;
=======
=======
>>>>>>> parent of dcc4155... Fixed arrow shooting and targets
    private void OnCollisionEnter(Collision collision) {
        print("colliding with game object " + collision.gameObject.name + " whose name is " + LayerMask.LayerToName(collision.gameObject.layer));

        if (!hit && CollisionIsAllowed(collision)) {
            hit = true;
            print("Arrow hit " + collision.gameObject.name + " (tagged as \"" + collision.gameObject.tag + "\")");

            //stick to the target
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            rigidbody.isKinematic = true;
        }
<<<<<<< HEAD
>>>>>>> parent of dcc4155... Fixed arrow shooting and targets
=======
>>>>>>> parent of dcc4155... Fixed arrow shooting and targets
    }

    private void Finish() {
        //enable mouse look and switch to first person camera
        shootManager.LoadArrow();
        shootManager.ExitCamAnimation(true, CameraEnabler.Tag.FirstPerson, false);
        camManager.DestroyCam(arrowCamera);
        projManager.DestroyLastSpawned();

        enabled = false;
    }

    private bool CollisionIsAllowed(Collision collision) {
        return (1 << collision.gameObject.layer) == collisionsOnLayer.value;
    }
}