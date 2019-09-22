using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileArrow : MonoBehaviour {
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float force = 2000f;
    [SerializeField] private float timer;

    private readonly Vector3 FORWARD_VECTOR = new Vector3(-0.44f, -0.037f, 1f);

    private Rigidbody rigidbody;
    private bool fly, hit;

    private void OnEnable() {
        this.rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.isKinematic = false;
        rigidbody.AddRelativeForce(Vector3.up * force);

        this.hit = false;
        this.fly = true;
    }

    private void Update() {
        if (fly && !hit) Spin();
    }

    private void OnCollisionEnter(Collision collision) {
        if (fly && !hit && !collision.collider.tag.Equals("Avatar")) {
            hit = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeAll; //stick to the object
            print("Arrow hit " + collision.gameObject.name + " (tagged as \"" + collision.gameObject.tag + "\"");
        }
    }

    private void Spin() {
        Vector3 velocity = rigidbody.velocity;
        float combinedVelocity = Mathf.Sqrt(Mathf.Pow(velocity.x, 2) + Mathf.Pow(velocity.y, 2));
        float fallAngle = -Mathf.Atan2(velocity.z, combinedVelocity);
        fallAngle *= 180 / Mathf.PI; //to degrees
        print("arrow angle is " + fallAngle);
        Vector3 sourceRotation = transform.eulerAngles;
        transform.eulerAngles = new Vector3(180 - fallAngle, sourceRotation.y, sourceRotation.z);
    }
}