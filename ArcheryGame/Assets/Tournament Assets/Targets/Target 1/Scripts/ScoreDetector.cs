using System;
using UnityEngine;

public class ScoreDetector : MonoBehaviour
{
    [Tooltip("Non-negative value of the ring.")]
    [SerializeField] private uint scoreValue;

    private readonly int OVERLAP_OBJECTS_ALLOC = 100;

    private ScoreManager scoreManager;
    private Collider[] overlapResults;
    private float detectionPeriod, distance, radius;
    private GameObject sensedProjectile;
    private Vector3 center;

    private void Start() {
        GameObject rings = transform.parent.gameObject;
        GameObject cloth = rings.transform.parent.gameObject;
        GameObject target = cloth.transform.parent.gameObject;
        this.scoreManager = target.GetComponent<ScoreManager>();
        this.overlapResults = new Collider[OVERLAP_OBJECTS_ALLOC];

        //define a circle collider that covers all ring area
        GameObject color = transform.Find("Color").gameObject;
        GameObject line;
        try { line = transform.Find("Line").gameObject; }
        catch (NullReferenceException) { line = null;  }
        GameObject anchor = (line != null) ? line : color; //not all rings have outer lines
        this.radius = anchor.GetComponent<MeshRenderer>().bounds.size.x / 2;
        this.center = anchor.transform.position + new Vector3(-radius, 0, 0);
    }

    private void Update() {
        if (detectionPeriod > 0) {
            Detect();

            detectionPeriod -= Time.deltaTime;
            if (detectionPeriod <= 0) Report();
        }
    }

    /// <summary>
    /// Let the ring detect arrow collisions for a fixed period of time.
    /// </summary>
    /// <param name="time">Fixed period of time during which the ring check for new collisions</param>
    public void EnableDetection(float time) {
        if (time > 0) detectionPeriod = time;
    }

    /// <summary>
    /// Check if an arrow collided this ring during the detection period.
    /// </summary>
    private void Detect() {
        int numFound = Physics.OverlapSphereNonAlloc(center, radius, overlapResults);
        sensedProjectile = null;

        for (int i = numFound - 1; i >= 0; i--) {
            Collider collider = overlapResults[i];
            GameObject obj = collider.gameObject;

            //found the arrow object
            if (obj.tag.Equals(scoreManager.projectileTag)) {
                //calculate distance from the center
                Vector3 closestPoint = collider.ClosestPoint(center);
                distance = Mathf.Abs(center.x - closestPoint.x) * 1000; //in milimeters

                sensedProjectile = obj;
                return;
            }
        }
    }

    /// <summary>
    /// Report ScoreManager about a projectile collision.
    /// </summary>
    private void Report() {
        if (sensedProjectile != null) scoreManager.Report(sensedProjectile, scoreValue, distance);
        sensedProjectile = null;
    }
}