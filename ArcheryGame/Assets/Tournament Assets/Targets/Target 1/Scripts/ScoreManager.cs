using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Tooltip("Array of the 10 rings")]
    [SerializeField] private GameObject[] rings = new GameObject[10];

    [Tooltip("Use auto-trigger to let the target detect an incoming projectile automatically.\n" +
             "Disabling this option forces you to use the CalculateScore() method manually,\n" +
             "in order to let the target know a projectile had hit it.\n\n" +
             "WARNING: Auto-trigger only works when disabling Project Settings->"+
             "Physics->Queries Hit Triggers.")]
    [SerializeField] private bool autoTrigger = true;

    [Tooltip("The tag of the projectile that's suppose to hit the target.")]
    [SerializeField] public string projectileTag;

    [Tooltip("How to tag the projectile that already hit the target" +
             " (must be different than the projectile tag).")]
    [SerializeField] public string clearedProjectileTag = "Untagged";

    [Tooltip("The amount of time (in seconds) given to calculate the final score of the projectile.\n" +
             "This period of time is heavily relying on performance, and selecting a one\n" + 
             "that's too short might result in an inaccuracy of the final score.")]
    [SerializeField] [Range(.1f, 1)] private float reportPeriod = .5f;

    private GameObject reportedProjectile, monitor;
    private float reportTimer, finalDistance;
    private uint finalScore, lastScore;
    private bool waitingForReports;

    private void Start() {
        if (!CheckTagException(projectileTag, clearedProjectileTag))
            throw new ProjectileTagException();

        this.monitor = GameObject.FindGameObjectWithTag("Monitor");
        Init();
    }

    /// <summary>
    /// Initialize the final score and wait for a new hit.
    /// </summary>
    private void Init() {
        this.reportTimer = 0;
        this.finalScore = 0;
        this.waitingForReports = false;
        this.reportedProjectile = null;
    }

    private void Update() {
        if (waitingForReports) {
            reportTimer -= Time.deltaTime;

            //check final results
            if (reportTimer <= 0) {
                ReportFinalScore(finalScore, finalDistance);
                lastScore = finalScore;
                Init();
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (autoTrigger && collision.collider.gameObject.tag.Equals(projectileTag)) CalculateScore();
    }

    private void OnTriggerEnter(Collider collider) {
        if (autoTrigger && collider.gameObject.tag.Equals(projectileTag)) CalculateScore();
    }

    /// <summary>
    /// Tell the rings to start detecting collisions with a projectile.
    /// The rings will try to detect that collision for a fixed period of time and then
    /// report back their results.
    /// </summary>
    public void CalculateScore() {
        foreach (GameObject ring in rings) {
            ScoreDetector detector = ring.GetComponent<ScoreDetector>();
            detector.EnableDetection(reportPeriod);
        }

        //activate timer
        reportTimer = reportPeriod * 1.1f;
        waitingForReports = true;
    }

    /// <summary>
    /// Report the target about a collision with one of the rings.
    /// The highest value ring is taken as the rightful final score.
    /// </summary>
    /// <param name="score">The value of the reporting ring</param>
    public void Report(GameObject arrow, uint score, float dist) {
        reportedProjectile = arrow;

        if (score > finalScore) {
            finalScore = score;
            finalDistance = dist;
        }
    }

    /// <summary>
    /// Report the last hit's final score.
    /// </summary>
    /// <param name="score">The score of the last projectile that hit the target</param>
    private void ReportFinalScore(uint score, float dist) {
        if (reportedProjectile != null) {
            print("Reported " + score + " with distance " + dist + ".");
            reportedProjectile.tag = clearedProjectileTag;
        }
        else print("Miss");
    }

    /// <summary>
    /// Check if the "Projectile Tag" and "Cleared Projectile Tag" parameters
    /// contain legal values.
    /// </summary>
    /// <param name="projTag">The inspector's "Projectile Tag"</param>
    /// <param name="clearedProjTag">The inspector's "Cleared Projectile Tag"</param>
    /// <returns>True if the parameters contain legal values</returns>
    private bool CheckTagException(string projTag, string clearedProjTag) {
        bool foundFirst = false, foundSecond = false;
        string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
        for (int i = 0; i < tags.Length; i++) {
            if (!foundFirst && projTag.Equals(tags[i])) foundFirst = true;
            else if (!foundSecond && clearedProjTag.Equals(tags[i])) foundSecond = true;
        }

        return foundFirst && foundSecond;
    }

    /// <returns>The score of the last projectile that hit the target.</returns>
    public int GetLastScore() { return (int) lastScore; }

    /// <returns>
    /// The distance from the center (in milimeters) of the last projectile that hit the target.
    /// </returns>
    public float GetLastDistanceFromCenter() { return finalDistance; }
}