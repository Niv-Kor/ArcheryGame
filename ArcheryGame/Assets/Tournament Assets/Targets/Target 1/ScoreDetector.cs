using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreDetector : MonoBehaviour
{
    [SerializeField] [Range(0, 10)] private int scoreValue;

    private ScoreManager scoreManager;

    void Start() {
        GameObject target = transform.parent.parent.gameObject;
        this.scoreManager = target.GetComponent<ScoreManager>();
    }

    private void OnCollisionEnter(Collision collision) {
        print("collided the " + scoreValue);
        scoreManager.Report(scoreValue);
    }
}
