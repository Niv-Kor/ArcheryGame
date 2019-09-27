using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private readonly float MAX_REPORT_TIME = .3f;

    private GameObject monitor;
    private float reportTimer;
    private int finalScore;
    private bool reported;

    private void Start() {
        this.monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        Init();
    }

    private void Init() {
        this.reportTimer = MAX_REPORT_TIME;
        this.finalScore = 0;
        this.reported = false;
    }

    private void Update() {
        if (reported) {
            reportTimer -= Time.deltaTime;

            //check final results
            if (reportTimer <= 0) {
                ReportFinalScore(finalScore);
                Init();
            }
        }
    }

    public void Report(int score) {
        reported = true;
        if (score > finalScore) finalScore = score;

        print("REPORTED " + score + " AND FINAL IS NOW " + finalScore);
    }

    private void ReportFinalScore(int score) {
        print("reported " + score);
        //TODO
    }
}
