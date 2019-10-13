using Assets.Script_Tools;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class CameraManager : MonoBehaviour
{
    [SerializeField] public Camera firstPersonCam;
    [SerializeField] private List<GameObject> cameraObjects;
    [SerializeField] [Range(0, 100)] private float zoomPercent;

    private static readonly float DEFAULT_FIELD_OF_VIEW = 60;

    private RigidbodyFirstPersonController playerController;
    private CameraEnabler.Tag currentCameraTag;
    private Camera currentCamera;
    private ProjectileManager projManager;

    private void Start() {
        GameObject player = GameObject.FindWithTag("Player");
        GameObject monitor = GameObject.FindWithTag("Player Monitor");
        this.playerController = player.GetComponent<RigidbodyFirstPersonController>();
        this.projManager = monitor.GetComponent<ProjectileManager>();
        this.currentCameraTag = CameraEnabler.Tag.None;
        ChangeCam(CameraEnabler.Tag.ThirdPerson);
    }

    /// <summary>
    /// Change the main camera of the game.
    /// </summary>
    /// <param name="cam">The camera to change into</param>
    public void ChangeCam(CameraEnabler.Tag camTag, GameObject cam=null) {
        if (camTag == CameraEnabler.Tag.None || camTag == currentCameraTag) return;

        //looking for the last spawned arrow's camera
        if (camTag == CameraEnabler.Tag.Arrow && cam == null) {
            GameObject lastSpawnedArrow = projManager.GetLastSpawned();

            if (lastSpawnedArrow != null) {
                GameObject arrowCam = ObjectFinder.GetChild(lastSpawnedArrow, "Camera");
                ChangeCam(camTag, arrowCam);
            }
            else ChangeCam(CameraEnabler.Tag.None);

            return;
        }
        //add new camera to the list
        else if (cam != null && !cameraObjects.Contains(cam)) AddCam(cam);

        //iterate over all cameras
        foreach (GameObject obj in cameraObjects) {
            CameraEnabler.Tag ofTag = obj.GetComponent<CameraEnabler>().cameraTag;
            bool correctCam = ofTag == camTag || obj == cam;
            obj.GetComponent<CameraEnabler>().Activate(correctCam);

            if (correctCam) {
                currentCameraTag = camTag;
                currentCamera = obj.GetComponent<Camera>();
                if (camTag != CameraEnabler.Tag.Arrow) playerController.cam = currentCamera;
            }
        }
    }

    /// <param name="camTag">The tag of the requested camera</param>
    /// <returns>
    /// A camera that is tagged the same as the argument.
    /// If multiple cameras are found, the first one is returned.
    /// </returns>
    public GameObject GetCam(CameraEnabler.Tag camTag) {
        if (camTag == CameraEnabler.Tag.Arrow) return projManager.GetLastSpawned();
        else {
            foreach (GameObject obj in cameraObjects) {
                CameraEnabler.Tag ofTag = obj.GetComponent<CameraEnabler>().cameraTag;
                if (ofTag == camTag) return obj;
            }
        }

        return null;
    }

    /// <summary>
    /// Add a camera to the cameras list.
    /// </summary>
    /// <param name="cam">The camera to add</param>
    public void AddCam(GameObject cam) {
        cameraObjects.Add(cam);
    }

    /// <summary>
    /// Remove a camera from the cameras list.
    /// </summary>
    /// <param name="cam">The camera to remove</param>
    public void DestroyCam(GameObject cam) {
        cameraObjects.Remove(cam);
    }

    /// <summary>
    /// Zoom with the first person camera.
    /// </summary>
    /// <param name="flag">True to zoom in or false to zoom out back to default</param>
    public void SetZoom(bool flag) {
        if (currentCamera == null) return;
        currentCamera.fieldOfView = flag ? GetPhysicalZoom() : DEFAULT_FIELD_OF_VIEW;
    }

    /// <summary>
    /// Zoom with the first person camera.
    /// </summary>
    /// <param name="flag">True to zoom in or false to zoom out back to default</param>
    /// <param name="cam,">The camera to zoom in or out</param>
    public void SetZoom(bool flag, Camera cam=null) {
        Camera zoomedCamera = (cam != null) ? cam : currentCamera;
        if (currentCamera == null) return;

        zoomedCamera.fieldOfView = flag ? GetPhysicalZoom() : DEFAULT_FIELD_OF_VIEW;
    }

    /// <summary>
    /// Get the maximal zoom of a camera (based on the "Zoom Percent" field).
    /// </summary>
    public float GetPhysicalZoom() {
        float subtraction = (DEFAULT_FIELD_OF_VIEW / 100) * zoomPercent;
        return DEFAULT_FIELD_OF_VIEW - subtraction;
    }

    /// <returns>The current main camera object.</returns>
    public Camera GetMainCamera() { return currentCamera; }

    /// <returns>The current main camera's tag.</returns>
    public CameraEnabler.Tag GetMainCameraTag() { return currentCameraTag; }
}