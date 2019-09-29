using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreDetector : MonoBehaviour
{
    [Tooltip("The value of the ring")]
    [SerializeField] [Range(0, 10)] private int scoreValue;

    private ScoreManager scoreManager;

    private void Start() {
        GameObject ringNum = transform.parent.gameObject;
        GameObject rings = ringNum.transform.parent.gameObject;
        GameObject cloth = rings.transform.parent.gameObject;
        GameObject target = cloth.transform.parent.gameObject;
        this.scoreManager = target.GetComponent<ScoreManager>();
    }

    private void OnCollisionEnter(Collision collision) {
        scoreManager.Report(scoreValue);
    }
}
