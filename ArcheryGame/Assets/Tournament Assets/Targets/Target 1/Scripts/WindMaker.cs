using UnityEngine;

public class WindMaker : MonoBehaviour
{
    [Tooltip("How wild is the flag's movement.")]
    [SerializeField] [Range(0, 5)] private float wildness;

    private Cloth cloth;
    private float wildTime;

    void Start() {
        this.cloth = GetComponent<Cloth>();
    }

    void Update() {
        wildTime -= Time.deltaTime;

        if (wildTime <= 0) {
            wildTime = 5 - wildness;
            cloth.useGravity = !cloth.useGravity;
        }      
    }
}
