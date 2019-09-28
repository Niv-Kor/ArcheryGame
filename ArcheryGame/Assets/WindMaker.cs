using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindMaker : MonoBehaviour
{
    [SerializeField] [Range(0, 5)] private float wildMovement;

    private Cloth cloth;
    private float wildTime;

    void Start() {
        this.cloth = GetComponent<Cloth>();
    }

    void Update() {
        wildTime -= Time.deltaTime;

        if (wildTime <= 0) {
            wildTime = 5 - wildMovement;
            cloth.useGravity = !cloth.useGravity;
        }      
    }
}
