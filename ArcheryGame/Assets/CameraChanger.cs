using System.Collections;
using System.Collections.Generic;
using Assets.Script_Tools;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class CameraChanger : MonoBehaviour
{
    public enum PlayerCamera {
        FIRST_PERSON, THIRD_PERSON
    }

    [SerializeField] private Camera firstPersonCam;
    [SerializeField] private Camera thirdPersonCam;
    [SerializeField] [Range(0, 100)] private float zoomPercent;

    private static readonly float DEFAULT_FIELD_OF_VIEW = 60;

    private RigidbodyFirstPersonController playerController;

    private void Start() {
        GameObject player = GameObject.FindWithTag("Player");
        this.playerController = player.GetComponent<RigidbodyFirstPersonController>();

        ChangeCam(PlayerCamera.THIRD_PERSON);
    }

    /// <summary>
    /// Change the main camera of the game.
    /// </summary>
    /// <param name="cam">The camera to change into</param>
    public void ChangeCam(PlayerCamera cam) {
        bool isFirst = cam == PlayerCamera.FIRST_PERSON;
        TagMainCamera(firstPersonCam, isFirst);
        firstPersonCam.enabled = isFirst;
        firstPersonCam.gameObject.SetActive(isFirst);

        bool isThird = cam == PlayerCamera.THIRD_PERSON;
        TagMainCamera(thirdPersonCam, isThird);
        thirdPersonCam.enabled = isThird;
        thirdPersonCam.gameObject.SetActive(isThird);

        Camera currentCamera = isFirst ? firstPersonCam : thirdPersonCam;
        playerController.cam = currentCamera;
    }

    /// <summary>
    /// Tag a camera as "MainCamera" or "untagged".
    /// </summary>
    /// <param name="camera">The camera to tag</param>
    /// <param name="flag">True for "MainCamera", or false for "Untagged"</param>
    private void TagMainCamera(Camera camera, bool flag) {
        if (flag) camera.gameObject.tag = "MainCamera";
        else camera.gameObject.tag = "Untagged";
    }

    /// <summary>
    /// Zoom with the first person camera.
    /// </summary>
    /// <param name="flag">True to zoom in or false to zoom out back to default</param>
    public void SetFirstPersonZoom(bool flag) {
        float multiplier = flag ? zoomPercent : 0;
        float subtraction = (DEFAULT_FIELD_OF_VIEW / 100) * multiplier;
        firstPersonCam.fieldOfView = DEFAULT_FIELD_OF_VIEW - subtraction;
    }
}