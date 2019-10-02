using UnityEngine;

public class ArrowCameraAligner : MonoBehaviour
{
    private GameObject arrow;

    private void OnEnable() {}

    private void Start() {
        this.arrow = transform.parent.gameObject;
    }

    private void Update() {
        transform.rotation = Quaternion.LookRotation(arrow.transform.forward);
    }
}
