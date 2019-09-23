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
    [SerializeField] private float straightTime = 2f;
    [SerializeField] private float force = 8000f;

    private readonly string[] FORBIDDEN_COLLISIONS = {"Player",
                                                      "Avatar",
                                                      "Arrow",
                                                      "Camera",
                                                      "Main Camera"};

    private Rigidbody rigidbody;
    private CameraManager camManager;
    private GameObject arrowCamera;
    private ShootingSessionManager shootManager;

    private void OnEnable() {
        //switch to the arrow camera view
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        this.arrowCamera = ObjectFinder.GetChild(gameObject, "Arrow Camera");
        this.camManager = monitor.GetComponent<CameraManager>();
        //camManager.ChangeCam(CameraTagSystem.Tag.Arrow, arrowCamera);

        //disable mouse look
        this.shootManager = monitor.GetComponent<ShootingSessionManager>();
        shootManager.EnterCamAnimation(false);

        //enable the arrow's collider
        GetComponent<CapsuleCollider>().enabled = true;
        this.rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.isKinematic = false;

        //launch the arrow
        rigidbody.AddRelativeForce(Vector3.forward * force);
    }

    private void Update() {
        straightTime -= Time.deltaTime;
        //if (straightTime < 0)
        Pitch();
    }

    private void OnCollisionEnter(Collision collision) {
        if (CollisionIsAllowed(collision)) {
            enabled = false;
            rigidbody.constraints = RigidbodyConstraints.FreezeAll; //stick to the target

            //enable mouse look and switch to first person camera
            shootManager.ExitCamAnimation(true, CameraTagSystem.Tag.FirstPerson, false);
            camManager.DestroyCam(arrowCamera);
            print("Arrow hit " + collision.gameObject.name + " (tagged as \"" + collision.gameObject.tag + "\")");
        }
    }

    /// <summary>
    /// Rotate the arrow around the x axis while in mid-air.
    /// </summary>
    private void Pitch() {
        transform.rotation = Quaternion.LookRotation(rigidbody.velocity);
    }

    private bool CollisionIsAllowed(Collision collision) {
        foreach (string tag in FORBIDDEN_COLLISIONS)
            if (collision.gameObject.tag.Equals(tag)) return false;

        return true;
    }

    private bool InRange(float value, float from, float to) {
        return value >= from && value <= to;
    }
}