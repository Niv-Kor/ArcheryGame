using Assets.Script_Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightAdjuster : MonoBehaviour
{
    [Tooltip("Fire a raycast at the object the arrow is aiming at")]
    [SerializeField] private bool castRay;

    [Tooltip("Print the aimed object's name and distance")]
    [SerializeField] private bool debug;

    private readonly string DEBUGGER_PREFIX = "[SightAdjuster] ";

    private GameObject arrow;
    private ProjectileArrow arrowBehaviour;

    private void Start() {
        this.arrow = GameObject.FindGameObjectWithTag("Arrow");
        this.arrowBehaviour = arrow.GetComponent<ProjectileArrow>();
    }

    private void Update() {
        if (castRay || debug) Aim();
    }

    /// <summary>
    /// Fire a raycast that determines where the arrow should hit, and analyze the results.
    /// </summary>
    private void Aim() {
        Transform camTransform = Camera.main.transform;
        Ray ray = new Ray(camTransform.position, camTransform.forward);
        GameObject aimedObject = null;
        float distance = Mathf.Infinity;

        //find direction
        if (Physics.Raycast(ray, out RaycastHit rayHit) && rayHit.collider != null) {
            aimedObject = rayHit.collider.gameObject;
            distance = rayHit.distance;
        }

        arrowBehaviour.sightHit = rayHit.point;

        if (castRay) Debug.DrawRay(camTransform.position, camTransform.forward, Color.cyan);
        if (debug) DebugAiming(aimedObject, distance);
    }

    /// <summary>
    /// Print the aimed object's name and distance.
    /// </summary>
    /// <param name="aimedObject">The object the arrow is aiming at</param>
    /// <param name="distance">The distance from the object</param>
    private void DebugAiming(GameObject aimedObject, float distance) {
        if (aimedObject != null) print(DEBUGGER_PREFIX + "Sight is aiming at " + aimedObject.name + ". Distance: " + distance);
        else print(DEBUGGER_PREFIX + "Arrow is not aiming at any object.");
    }
}