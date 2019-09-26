using Assets.Script_Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowAligner : MonoBehaviour
{
    private GameObject originalPrefab;

    private void OnEnable() {}

    private void Start() {
        GameObject bow = GameObject.FindGameObjectWithTag("Bow");
        this.originalPrefab = ObjectFinder.GetChild(bow, "Arrow");
        print("start");
    }

    private void Update() {
        print("align");
        transform.position = originalPrefab.transform.position;
        transform.rotation = originalPrefab.transform.rotation;
    }
}