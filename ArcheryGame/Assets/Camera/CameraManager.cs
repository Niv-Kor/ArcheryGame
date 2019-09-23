using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Assets.Script_Tools;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera firstPersonCam;
    [SerializeField] private List<GameObject> cameraObjects;
    [SerializeField] [Range(0, 100)] private float zoomPercent;

    private static readonly float DEFAULT_FIELD_OF_VIEW = 60;

    private RigidbodyFirstPersonController playerController;
    private CameraTagSystem.Tag currentCameraTag;
    private Camera currentCamera;
    private ProjectileManager projManager;

    private void Start() {
        GameObject player = GameObject.FindWithTag("Player");
        GameObject monitor = GameObject.FindWithTag("Player Monitor");
        this.playerController = player.GetComponent<RigidbodyFirstPersonController>();
        this.projManager = monitor.GetComponent<ProjectileManager>();
        this.currentCameraTag = CameraTagSystem.Tag.None;
        ChangeCam(CameraTagSystem.Tag.ThirdPerson);
    }

    /// <summary>
    /// Change the main camera of the game.
    /// </summary>
    /// <param name="cam">The camera to change into</param>
    public void ChangeCam(CameraTagSystem.Tag camTag, GameObject cam=null) {
        if (camTag == CameraTagSystem.Tag.None || camTag == currentCameraTag) return;

        //looking for the last spawned arrow's camera
        if (camTag == CameraTagSystem.Tag.Arrow && cam == null) {
            GameObject lastSpawnedArrow = projManager.GetLastSpawned();

            if (lastSpawnedArrow != null) {
                GameObject arrowStick = ObjectFinder.GetChild(lastSpawnedArrow, "Stick");
                GameObject arrowCam = ObjectFinder.GetChild(arrowStick, "Arrow Camera");
                ChangeCam(camTag, arrowCam);
            }
            else ChangeCam(CameraTagSystem.Tag.None);

            return;
        }
        //add new camera to the list
        else if (cam != null && !cameraObjects.Contains(cam)) AddCam(cam);

        //iterate over all cameras
        foreach (GameObject obj in cameraObjects) {
            CameraTagSystem.Tag ofTag = obj.GetComponent<CameraTagSystem>().tag;
            bool correctCam = ofTag == camTag || obj == cam;
            obj.SetActive(correctCam);
            TagAsMain(obj, correctCam);

            if (correctCam) {
                currentCameraTag = camTag;
                currentCamera = obj.GetComponent<Camera>();
                if (camTag != CameraTagSystem.Tag.Arrow) playerController.cam = currentCamera;
            }
        }
    }

    public void AddCam(GameObject cam) {
        cameraObjects.Add(cam);
    }

    public void DestroyCam(GameObject cam) {
        cameraObjects.Remove(cam);
    }

    private void TagAsMain(GameObject cam, bool flag) {
        string tag = flag ? "MainCamera" : "Camera";
        cam.tag = tag;
    }

    /// <summary>
    /// Zoom with the first person camera.
    /// </summary>
    /// <param name="flag">True to zoom in or false to zoom out back to default</param>
    public void SetZoom(bool flag) {
        if (currentCamera == null) return;

        float multiplier = flag ? zoomPercent : 0;
        float subtraction = (DEFAULT_FIELD_OF_VIEW / 100) * multiplier;
        currentCamera.fieldOfView = DEFAULT_FIELD_OF_VIEW - subtraction;
    }
}