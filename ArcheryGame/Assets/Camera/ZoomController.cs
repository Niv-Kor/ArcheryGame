using UnityEngine;

public class ZoomController : MonoBehaviour
{
    [Tooltip("The maximum zoom possible.")]
    [SerializeField] [Range(0, 100f)] private float maxZoomPercent;
    
    [Tooltip("The maximum amount of wheel rotations it takes to get from 0% to 100%.")]
    [SerializeField] private uint maxWheelRotations;

    private readonly int DEFAULT_VIEW = 60;
    private readonly int TRANSFORMATION_STEP = 1;

    private Camera camComponent;
    public float currentZoom, nextTransform, minZoom;
    private int transformMultiplier;
    private bool transforming;

    private void Start() {
        this.camComponent = GetComponent<Camera>();
        this.currentZoom = 0;
        this.minZoom = DEFAULT_VIEW - (maxZoomPercent * DEFAULT_VIEW / 100);
    }

    private void Update() {
        if (!transforming) {
            float wheelValue = Input.GetAxis("Mouse ScrollWheel");

            if (wheelValue != 0) {
                minZoom = DEFAULT_VIEW - (maxZoomPercent * DEFAULT_VIEW / 100);
                float rotationVolume = (DEFAULT_VIEW - minZoom) / (maxWheelRotations / 2);

                if (wheelValue < 0 && currentZoom > 0) currentZoom -= rotationVolume;
                else if (wheelValue > 0 && currentZoom < 100) currentZoom += rotationVolume;
                else return;

                currentZoom = Mathf.Clamp(currentZoom, 0, 100);
                Set((100 - currentZoom) * (DEFAULT_VIEW - minZoom) / 100 + minZoom);
            }
        }
        else Transform();
    }

    /// <summary>
    /// Set the field of view variable and transform it smoothly.
    /// </summary>
    /// <param name="fieldOfView">The new value for the field of view variable (Must be within range)</param>
    public void Set(float fieldOfView) {
        float minZoom = DEFAULT_VIEW - (maxZoomPercent * DEFAULT_VIEW / 100);
        if (fieldOfView < minZoom || fieldOfView > DEFAULT_VIEW) return;

        nextTransform = fieldOfView;
        transformMultiplier = (nextTransform > camComponent.fieldOfView) ? 1 : -1;
        transforming = true;
    }

    /// <summary>
    /// Smoothly change the field of view variable.
    /// </summary>
    private void Transform() {
        //finished transforiming because difference has the same sign as transformMultiplier
        if ((camComponent.fieldOfView - nextTransform) * transformMultiplier >= 0) {
            currentZoom = 100 - (camComponent.fieldOfView - minZoom) / (DEFAULT_VIEW - minZoom) * 100;
            transforming = false;
            return;
        }
        else camComponent.fieldOfView += TRANSFORMATION_STEP * transformMultiplier;
    }

    /// <summary>
    /// Reset zoom back to 0%.
    /// </summary>
    public void Reset() { Set(DEFAULT_VIEW); }
}
