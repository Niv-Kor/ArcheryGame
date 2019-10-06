using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Tooltip("Array of the 10 rings")]
    [SerializeField] private GameObject[] rings = new GameObject[10];

    [Tooltip("Tag of the arrow that's suppose to hit the target")]
    [SerializeField] public string arrowTag;

    private readonly float REPORT_PERIOD = .5f;
    private readonly string CLEARED_ARROW_TAG = "Untagged";

    private GameObject reportedArrow, monitor;
    private float reportTimer;
    private int finalScore, lastScore;
    private bool waitingForReports;

    private void Start() {
        this.monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        Init();
    }

    /// <summary>
    /// Initialize the final score and wait for a new hit.
    /// </summary>
    private void Init() {
        this.reportTimer = 0;
        this.finalScore = 0;
        this.waitingForReports = false;
        this.reportedArrow = null;
    }

    private void Update() {
        if (waitingForReports) {
            reportTimer -= Time.deltaTime;

            //check final results
            if (reportTimer <= 0) {
                ReportFinalScore(finalScore);
                lastScore = finalScore;
                Init();
            }
        }
    }

    /// <summary>
    /// Tell the rings to start detecting collisions with an arrow.
    /// The rings will try to detect that collision for a fixed period of time and then
    /// report back their results.
    /// </summary>
    public void CheckScore() {
        foreach (GameObject ring in rings) {
            ScoreDetector detector = ring.GetComponent<ScoreDetector>();
            detector.EnableDetection(REPORT_PERIOD);
        }

        //activate timer
        reportTimer = REPORT_PERIOD * 1.1f;
        waitingForReports = true;
    }

    /// <summary>
    /// Report the target about a collision with one of the rings.
    /// The highest value ring is taken as the rightful final score.
    /// </summary>
    /// <param name="score">The value of the reporting ring</param>
    public void Report(GameObject arrow, int score) {
        if (score > finalScore) finalScore = score;
        reportedArrow = arrow;
    }

    /// <summary>
    /// Report the last hit's final score.
    /// </summary>
    /// <param name="score">The score of the last arrow that hit the target</param>
    private void ReportFinalScore(int score) {
        if (reportedArrow != null) {
            print("Reported " + score);
            reportedArrow.tag = CLEARED_ARROW_TAG;
        }
    }

    /// <returns>The score of the last arrow that hit the target.</returns>
    public int GetLastHitScore() { return lastScore; }
}
