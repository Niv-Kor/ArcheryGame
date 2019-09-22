using System.Collections;
using System.Collections.Generic;
using Assets.Script_Tools;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;
using Input = UnityEngine.Input;

public class ShootingSessionManager : MonoBehaviour
{
    private readonly string SHOOTING_PARAM = "shoot";
    private readonly string DRAW_PARAM = "draw";

    private Animator animator;
    private RigidbodyFirstPersonController playerController;
    private CameraChanger cameraChanger;
    private GameObject player, firstPersonCamera, pulledArrow;
    private bool isShooting, isPulling;

    private void Start() {
        this.animator = gameObject.GetComponent<Animator>();
        this.player = GameObject.FindGameObjectWithTag("Player");
        this.playerController = player.GetComponent<RigidbodyFirstPersonController>();

        GameObject firstPersonObject = ObjectFinder.GetChild(gameObject, "First Person Camera");
        this.firstPersonCamera = ObjectFinder.GetChild(firstPersonObject, "Camera");
        GameObject equipment = ObjectFinder.GetChild(firstPersonCamera, "Equipment");
        GameObject bow = ObjectFinder.GetChild(equipment, "Bow");
        this.pulledArrow = ObjectFinder.GetChild(bow, "Arrow");

        GameObject camMonitor = GameObject.FindGameObjectWithTag("Camera Monitor");
        this.cameraChanger = camMonitor.GetComponent<CameraChanger>();
        this.isShooting = false;
        this.isPulling = false;
    }

    private void Update() {
        if (isShooting) {
            if (isPulling) {
                //withdraw
                if (Input.GetMouseButtonDown(1)) {
                    EnterCamAnimation();
                    animator.SetBool(DRAW_PARAM, false);
                    isPulling = false;
                }
                //shoot
                else if (Input.GetMouseButtonUp(0)) {
                    Shoot();
                    print("Release");
                    isPulling = false;
                }
            }
            else {
                //draw
                if (Input.GetMouseButtonDown(0)) {
                    EnterCamAnimation();
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
    public void ExitCamAnimation(bool disableAnimatorOnExit, CameraChanger.PlayerCamera exitToCamera, bool zoomOnExit) {
        cameraChanger.ChangeCam(exitToCamera);
        animator.enabled = !disableAnimatorOnExit;
        playerController.EnableMouseRotation(true);
        playerController.EnableMovement(!isShooting);
        cameraChanger.SetFirstPersonZoom(zoomOnExit);
    }

    /// <summary>
    /// Enter an animation of the camera and take control from the player.
    /// </summary>
    public void EnterCamAnimation() {
        playerController.EnableMouseRotation(false);
        playerController.EnableMovement(false);
        animator.enabled = true;
    }

    /// <summary>
    /// Enter or exit shooting stance while standing on one of the map's shooting spots.
    /// </summary>
    public void ToggleShootingStance() {
        EnterCamAnimation();
        animator.SetBool(SHOOTING_PARAM, !isShooting);
        isShooting = !isShooting; //toggle shooting mode
    }

    private void Shoot() {
        Transform arrowTransform = pulledArrow.transform;
        GameObject arrowInstance = Instantiate(pulledArrow, arrowTransform.position, arrowTransform.rotation);
        ProjectileArrow projArrow = arrowInstance.GetComponent<ProjectileArrow>();
        pulledArrow.SetActive(false);
        projArrow.enabled = true;
    }

    /// <returns>True if the player is at shooting stance at the moment.</returns>
    public bool AtShootingPos() { return isShooting; }

    /// <returns>True if the camera is animating at the moment.</returns>
    public bool isAnimating() { return animator.enabled; }
}