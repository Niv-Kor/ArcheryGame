using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class ArrowNavigator : MonoBehaviour
{
    [SerializeField] private float pitchAngle;

    [SerializeField] private float minForce = 30f;
    [SerializeField] private float maxForce = 250f;

    [Tooltip("The percent of the arrow that will stuck in the target.")]
    [SerializeField] [Range(0, 1f)] private float thrustDepthPercent = .15f;

    private readonly string PARENT_OBJECT_NAME = "Projectile Container";
    private readonly string SPHERE_NAVIGATOR_NAME = "Navigator";
    private readonly long DISTANCE_OF_MAX_FORCE = 1000;
    private readonly float MAX_FORCE_DECAY_PERCENT = 30;
    private readonly float ANCHOR_SCALE = .03f;

    private GameObject carrier, navigator, anchor;
    private float radius, arrowLength, force;
    private int forwardDirection;

    public void GenerateSphere(Vector3 arrowPos, Vector3 hitPoint, float arrowLength) {
        this.arrowLength = arrowLength;
        this.radius = Vector3.Distance(arrowPos, hitPoint);
        this.force = CalculateForce(radius);

        this.carrier = new GameObject(PARENT_OBJECT_NAME);
        carrier.transform.position = transform.position;
        carrier.transform.LookAt(hitPoint);
        transform.SetParent(carrier.transform);

        this.navigator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        navigator.name = SPHERE_NAVIGATOR_NAME;
        navigator.GetComponent<SphereCollider>().enabled = false;
        navigator.GetComponent<MeshRenderer>().enabled = false;
        navigator.transform.localScale *= radius * 2;
        navigator.transform.position = transform.position;
        navigator.transform.LookAt(hitPoint);
        navigator.transform.SetParent(carrier.transform);

        this.anchor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        anchor.GetComponent<SphereCollider>().enabled = false;
        anchor.GetComponent<MeshRenderer>().enabled = false;
        anchor.transform.localScale *= ANCHOR_SCALE;
        anchor.transform.position = hitPoint;
        anchor.transform.SetParent(navigator.transform);

        //check the sign of arrow flight's direction
        float anchorZ = anchor.transform.position.z;
        float carrierZ = carrier.transform.position.z;
        this.forwardDirection = (anchorZ > carrierZ) ? 1 : -1;

        pitchAngle = 1 / radius;
    }

    public bool Navigate(float delta) {
        //pitch the navigator so the anchor point will also get relatively lowered
        navigator.transform.Rotate(pitchAngle, 0, 0);

        //align the navigator sphere with the arrow
        Vector3 navigatorPos = navigator.transform.localPosition;
        navigator.transform.localPosition = new Vector3(navigatorPos.x, 0, navigatorPos.z);

        //rotate the arrow towards the anchor point
        Vector3 anchorPoint = anchor.transform.position;
        //transform.rotation = Quaternion.LookRotation(anchorPoint);

        //move the arrow towards the anchor and save the distance it made
        Vector3 lastPosition = carrier.transform.position;
        float decayedforce = DecayForce(force, Vector3.Distance(lastPosition, anchorPoint));
        carrier.transform.position = Vector3.MoveTowards(lastPosition, anchorPoint, delta * decayedforce);

        //check if arrow has yet to reach the anchor point
        if (!ReachPoint(carrier.transform.position, anchorPoint, 'z')) {
            float diff = Vector3.Distance(carrier.transform.position, lastPosition) * 2;
            Vector3 diffVector = new Vector3(diff, diff, diff);

            //relatively scale down the navigator sphere
            float thrustedArrowLength = arrowLength * (2 - (1 - thrustDepthPercent));
            if (navigator.transform.localScale.x > thrustedArrowLength)
                navigator.transform.localScale -= diffVector;

            return false;
        }
        else return true; //reach anchor point
    }

    public void Align() {
        navigator.transform.localPosition = transform.localPosition;
    }

    public void DefuseSpin(float angle) {
        //parentObj.transform.Rotate(0, 0, -angle);
    }

    private void OnDrawGizmos() {
        if (navigator != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(anchor.transform.position, .03f);
        }
    }

    private bool ReachPoint(Vector3 pointA, Vector3 pointB, char axis) {
        switch (axis.ToString().ToLower().ToCharArray()[0]) {
            case 'x': return (forwardDirection == 1) ? pointA.x >= pointB.x : pointA.x <= pointB.x;
            case 'y': return (forwardDirection == 1) ? pointA.y >= pointB.y : pointA.y <= pointB.y;
            case 'z': return (forwardDirection == 1) ? pointA.z >= pointB.z : pointA.z <= pointB.z;
            default: return false;
        };
    }

    private float DecayForce(float force, float currentDistance) {
        float passedDistance = radius - currentDistance;
        float percentPassed = passedDistance / radius * 100;
        float remainForce = 100 - (percentPassed * MAX_FORCE_DECAY_PERCENT / 100);
        print("decayed: " + force + " * " + remainForce + " = " + (force * remainForce / 100));
        return force * remainForce / 100;
    }

    private float CalculateForce(float distance) {
        float distancePercent = Mathf.Clamp(distance / DISTANCE_OF_MAX_FORCE * 100, 0, 250);
        return (distancePercent * (maxForce - minForce) / 100) + minForce;
    }
}