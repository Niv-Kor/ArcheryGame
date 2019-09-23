using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOfMass : MonoBehaviour
{
    [SerializeField] private Vector3 center;
    [SerializeField] private bool visualizeSphere = true;

    private readonly Color TRANSPARENT = new Color(0f, 0f, 0f, 0f);

    private Rigidbody rigidbody;

    private void Start() {
        this.rigidbody = GetComponent<Rigidbody>();
    }

    private void Update() {
        rigidbody.centerOfMass = center;
        rigidbody.WakeUp();
    }

    private void OnDrawGizmos() {
        Color color = visualizeSphere ? Color.red : TRANSPARENT;
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position + transform.rotation * center, .2f);
    }
}