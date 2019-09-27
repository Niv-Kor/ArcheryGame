using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindMaker : MonoBehaviour
{
    [SerializeField] [Range(0, 5)] private float wildMovement;

    private Cloth cloth;
    private float wildTime;
    private int stiffnessMultiplier;

    void Start() {
        this.cloth = GetComponent<Cloth>();
        this.stiffnessMultiplier = 1;
        cloth.stretchingStiffness = .5f;
    }

    void Update() {
        wildTime -= Time.deltaTime;

        if (wildTime <= 0) {
            wildTime = 5 - wildMovement;
            cloth.stretchingStiffness += .05f * stiffnessMultiplier;

            if (cloth.stretchingStiffness >= .7f || cloth.stretchingStiffness <= .5f)
                stiffnessMultiplier *= -1;
        }
    }
}
