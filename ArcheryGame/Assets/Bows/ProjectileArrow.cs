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

    private Rigidbody rigidbody;
    private CameraManager camManager;
    private GameObject arrowCamera;
    private ProjectileManager projManager;
    private ShootingSessionManager shootManager;
    private Vector3 hitPoint;
    private bool hit, flyForever;

    private void OnEnable() {
        //switch to the arrow camera view
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        this.arrowCamera = ObjectFinder.GetChild(gameObject, "Arrow Camera");
        this.camManager = monitor.GetComponent<CameraManager>();
        this.projManager = monitor.GetComponent<ProjectileManager>();

        //disable mouse look
        this.shootManager = monitor.GetComponent<ShootingSessionManager>();
        shootManager.EnterCamAnimation(true);

        //enable the arrow's collider
        GetComponent<CapsuleCollider>().enabled = true;
        this.rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.isKinematic = false;

        //launch the arrow at the direction of the sight

        //Vector3 archersParadox = new Vector3(.3f, -.4f, 0f);
        //rigidbody.AddRelativeForce((-transform.forward + archersParadox) * force);

        float force;
        RotateTowardsTarget(out force, out hitPoint);
        hitPoint.z -= transform.lossyScale.z * .75f; //shorten the distance by 70% of the arrow length

        //rigidbody.AddRelativeForce(-transform.forward * defaultForce);

        //clean y velocity so the arrow won't yaw
        //rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
    }

    private void Update() {
        transform.position = Vector3.MoveTowards(transform.position, hitPoint, Time.deltaTime * defaultForce);
        //transform.rotation = Quaternion.LookRotation(rigidbody.velocity); //pitch

        if (hit) {
            noHitEscortTime -= Time.deltaTime;
            if (noHitEscortTime <= 0) Finish();
        }
    }

    private void RotateTowardsTarget(out float force, out Vector3 hitPoint) {
        Transform camTransform = Camera.main.transform;
        Ray ray = new Ray(camTransform.position, Camera.main.transform.forward);
        float distance = defaultForce;

        //find direction
        if (Physics.Raycast(ray, out RaycastHit rayHit, Mathf.Infinity, collisionsOnLayer) && rayHit.collider != null) {
            hitPoint = rayHit.point;
            distance = rayHit.distance;
            print("hit " + rayHit.collider.gameObject.name);
        }
        else {
            print("not hit");
            hitPoint = ray.GetPoint(distance);
            hit = true;
        }

        force = distance;
        print("distance " + distance);

        //rotate
        Vector3 direction = (hitPoint - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnCollisionEnter(Collision collision) {
        print("colliding with game object " + collision.gameObject.name + " of layer " + collision.gameObject.layer + " whose name is " + LayerMask.LayerToName(collision.gameObject.layer));

        if (!hit && CollisionIsAllowed(collision)) {
            hit = true;
            print("Arrow hit " + collision.gameObject.name + " (tagged as \"" + collision.gameObject.tag + "\")");

            //stick to the target
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            rigidbody.isKinematic = true;
        }
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
        print("comparing " + (1 << collision.gameObject.layer) + " and " + collisionsOnLayer.value);
        print("which is " + ((1 << collision.gameObject.layer) == collisionsOnLayer.value));
        return (1 << collision.gameObject.layer) == collisionsOnLayer.value;
    }
}