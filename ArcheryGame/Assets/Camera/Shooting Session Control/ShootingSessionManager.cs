using System.Collections;
using System.Collections.Generic;
using Assets.Script_Tools;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;
using Input = UnityEngine.Input;

public class ShootingSessionManager : MonoBehaviour
{
    private readonly string STANCE_PARAM = "stance";
    private readonly string DRAW_PARAM = "draw";
    private readonly string SHOOT_PARAM = "shoot";

    private readonly Vector3 ARROW_CAM_ROTATION = new Vector3(-183.119f, -181.006f, 120.117f);

    private Animator animator;
    private RigidbodyFirstPersonController playerController;
    private CameraManager camManager;
    private GameObject player, pulledArrowObject, pulledArrow;
    private bool isShooting, isPulling;
    private ProjectileManager projManager;
    private GameObject arrowInstance, projectileCam;

    private void Start() {
        this.animator = gameObject.GetComponent<Animator>();
        this.player = GameObject.FindGameObjectWithTag("Player");
        this.playerController = player.GetComponent<RigidbodyFirstPersonController>();

        GameObject firstPersonObject = ObjectFinder.GetChild(gameObject, "First Person Camera");
        GameObject firstPersonCamera = ObjectFinder.GetChild(firstPersonObject, "Camera");
        GameObject equipment = ObjectFinder.GetChild(firstPersonCamera, "Equipment");
        GameObject bow = ObjectFinder.GetChild(equipment, "Bow");
        this.pulledArrowObject = ObjectFinder.GetChild(bow, "Arrow");
        this.pulledArrow = ObjectFinder.GetChild(pulledArrowObject, "Stick");

        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        this.projManager = monitor.GetComponent<ProjectileManager>();
        this.camManager = monitor.GetComponent<CameraManager>();
        this.isShooting = false;
        this.isPulling = false;
    }

    private void Update() {
        if (isShooting && !IsAnimating()) {
            if (isPulling) {
                //withdraw
                if (Input.GetMouseButtonDown(1)) {
                    //destroy the newly created arrow
                    projManager.DestroyLastSpawned();
                    camManager.DestroyCam(projectileCam);

                    //enable animation
                    EnterCamAnimation(true);
                    animator.SetBool(DRAW_PARAM, false);
                    isPulling = false;
                }
                //shoot
                else if (Input.GetMouseButtonUp(0)) {
                    EnterCamAnimation(true);
                    animator.SetBool(SHOOT_PARAM, true);
                    isPulling = false;
                }
            }
            else {
                //draw
                if (Input.GetMouseButtonDown(0)) {
                    InstantiateArrow();

                    //enable animation
                    EnterCamAnimation(true);
                    pulledArrow.SetActive(true);
                    animator.SetBool(DRAW_PARAM, true);
                    isPulling = true;
                }
            }
        }
    }

    /// <summary>
    /// Exit an animation of the camera and disable the animator,
    /// in order to asset the player with full mouse and movement control.
    /// </summary>
    /// <param name="disableAnimatorOnExit">True to disable the animator and apply full game control</param>
    /// <param name="exitToCamera">The camera to change to</param>
    /// <param name="zoomOnExit">True to zoom in with the first person camera</param>
    public void ExitCamAnimation(bool disableAnimatorOnExit, CameraTagSystem.Tag exitToCamera, bool zoomOnExit) {
        camManager.ChangeCam(exitToCamera);
        animator.enabled = !disableAnimatorOnExit;
        playerController.EnableMouseRotation(true);
        playerController.EnableMovement(!isShooting);
        camManager.SetZoom(zoomOnExit);
    }

    /// <summary>
    /// Enter an animation of the camera and take control from the player.
    /// </summary>
    /// /// <param name="enableAnimator">True to enable the animator or false to disable it</param>
    public void EnterCamAnimation(bool enableAnimator) {
        playerController.EnableMouseRotation(false);
        playerController.EnableMovement(false);
        animator.enabled = enableAnimator;
    }

    /// <summary>
    /// Enter or exit shooting stance while standing on one of the map's shooting spots.
    /// </summary>
    public void ToggleShootingStance() {
        EnterCamAnimation(true);
        animator.SetBool(STANCE_PARAM, !isShooting);
        isShooting = !isShooting; //toggle shooting mode
    }

    public void Shoot() {
        arrowInstance.SetActive(true);
        pulledArrowObject.SetActive(false);
        arrowInstance.GetComponent<MeshRenderer>().enabled = true; //display the arrow
        arrowInstance.GetComponent<ProjectileArrow>().enabled = true; //release the arrow
    }

    private void InstantiateArrow() {
        Transform arrowTransform = pulledArrowObject.transform;
        GameObject arrowInstanceObj = Instantiate(pulledArrowObject);
        arrowInstanceObj.transform.localScale *= 1.8f;
        arrowInstanceObj.transform.position = arrowTransform.position;
        arrowInstanceObj.transform.rotation = arrowTransform.rotation;

        this.arrowInstance = ObjectFinder.GetChild(arrowInstanceObj, "Stick");
        this.projectileCam = ObjectFinder.GetChild(arrowInstance, "Arrow Camera");
        projectileCam.transform.Rotate(0f, -180f, 0f);

        projManager.Spawn(arrowInstanceObj);
        camManager.AddCam(projectileCam);
        arrowInstance.GetComponent<MeshRenderer>().enabled = false;
    }

    /// <returns>True if the player is at shooting stance at the moment.</returns>
    public bool AtShootingPos() { return isShooting; }

    /// <returns>True if the camera is animating at the moment.</returns>
    public bool IsAnimating() { return animator.enabled; }
}