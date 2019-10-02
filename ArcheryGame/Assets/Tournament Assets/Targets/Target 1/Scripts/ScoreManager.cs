using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private readonly float MAX_REPORT_PERIOD = .3f;

    private GameObject monitor;
    private float reportTimer;
    private int finalScore, lastScore;
    private bool reported;

    private void Start() {
        this.monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        Init();
    }

    /// <summary>
    /// Initialize the final score and wait for a new hit.
    /// </summary>
    private void Init() {
        this.reportTimer = MAX_REPORT_PERIOD;
        this.finalScore = 0;
        this.reported = false;
    }

    private void Update() {
        if (reported) {
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
    /// Report the target about a collision with one of the rings.
    /// The highest value ring is taken as the rightful final score.
    /// </summary>
    /// <param name="score">The value of the reporting ring</param>
    public void Report(int score) {
        reported = true;
        if (score > finalScore) finalScore = score;
    }

    /// <summary>
    /// Report the last hit's score.
    /// </summary>
    /// <param name="score">The score of the last arrow that hit the target</param>
    private void ReportFinalScore(int score) {
        print("reported " + score);
        //TODO
    }

    /// <returns>The score of the last arrow that hit the target.</returns>
    public int GetLastHitScore() { return lastScore; }
}
