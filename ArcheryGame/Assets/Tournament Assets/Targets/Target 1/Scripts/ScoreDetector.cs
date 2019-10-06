using UnityEngine;

public class ScoreDetector : MonoBehaviour
{
    [Tooltip("The value of the ring")]
    [SerializeField] [Range(0, 10)] private int scoreValue;

    private readonly int MAX_OVERLAP_OBJECTS = 100;

    private ScoreManager scoreManager;
    private Collider[] overlapResults;
    private float detectionPeriod, radius;
    private GameObject sensedArrow;
    private Vector3 center;

    private void Start() {
        GameObject ringNum = transform.parent.gameObject;
        GameObject rings = ringNum.transform.parent.gameObject;
        GameObject cloth = rings.transform.parent.gameObject;
        GameObject target = cloth.transform.parent.gameObject;
        this.scoreManager = target.GetComponent<ScoreManager>();
        this.overlapResults = new Collider[MAX_OVERLAP_OBJECTS];
        this.radius = GetComponent<MeshRenderer>().bounds.size.x / 2;
        this.center = transform.position + new Vector3(-radius, 0, 0);
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
        sensedArrow = null;

        for (int i = numFound - 1; i >= 0; i--) {
            GameObject obj = overlapResults[i].gameObject;

            //found the arrow object
            if (obj.tag.Equals(scoreManager.arrowTag)) {
                sensedArrow = obj;
                return;
            }
        }
    }

    /// <summary>
    /// Report ScoreManager about an arrow collision.
    /// </summary>
    private void Report() {
        if (sensedArrow != null) scoreManager.Report(sensedArrow, scoreValue);
    }
}