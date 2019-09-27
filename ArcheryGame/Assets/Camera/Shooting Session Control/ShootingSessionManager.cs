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

    private Animator animator;
    private RigidbodyFirstPersonController playerController;
    private CameraManager camManager;
    private GameObject player, pulledArrowObject, pulledArrow;
    private GameObject arrowInstanceObj, arrowInstance, projectileCam;
    private bool isShooting, isPulling;
    private ProjectileManager projManager;
    
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
        Animate();
        AlignArrowInstance();
    }

    private void Animate() {
        if (isShooting && !IsAnimating() && !camManager.FollowingArrow()) {
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
                    LoadArrow();
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

    private void AlignArrowInstance() {
        if (arrowInstanceObj != null) {
            Transform arrowTransform = pulledArrowObject.transform;
            arrowInstanceObj.transform.position = arrowTransform.position;
            arrowInstanceObj.transform.rotation = arrowTransform.rotation;
        }
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
    /// Exit an animation of the camera and disable the animator,
    /// in order to asset the player with full mouse and movement control.
    /// </summary>
    /// <param name="disableAnimatorOnExit">True to disable the animator and apply full game control</param>
    /// <param name="exitToCamera">The camera to change to</param>
    /// <param name="zoomOnExit">True to zoom in with the first person camera</param>
    public void ExitCamAnimation(bool disableAnimatorOnExit, CameraEnabler.Tag exitToCamera, bool zoomOnExit) {
        camManager.ChangeCam(exitToCamera);
        animator.enabled = !disableAnimatorOnExit;
        playerController.EnableMouseRotation(true);
        playerController.EnableMovement(!isShooting);
        camManager.SetZoom(zoomOnExit);
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
        animator.SetBool(SHOOT_PARAM, false);
        animator.SetBool(DRAW_PARAM, false);
        ExitCamAnimation(false, CameraEnabler.Tag.Arrow, false);
        arrowInstance.SetActive(true);
        pulledArrowObject.SetActive(false);
        arrowInstance.GetComponent<MeshRenderer>().enabled = true; //display the arrow
        arrowInstance.GetComponent<ProjectileArrow>().enabled = true; //release the arrow

        //dispose arrow instance
        arrowInstanceObj = null;
        arrowInstance = null;
        projectileCam = null;
    }

    private void InstantiateArrow() {
        this.arrowInstanceObj = Instantiate(pulledArrowObject);
        arrowInstanceObj.transform.localScale *= 1.8f;
        AlignArrowInstance();

        this.arrowInstance = ObjectFinder.GetChild(arrowInstanceObj, "Stick");
        this.projectileCam = ObjectFinder.GetChild(arrowInstance, "Arrow Camera");
        arrowInstance.GetComponent<MeshRenderer>().enabled = false;
        projManager.Spawn(arrowInstanceObj);
        camManager.AddCam(projectileCam);
        projectileCam.SetActive(true);
        projectileCam.GetComponent<ArrowCameraAligner>().enabled = true;
    }

    public void LoadArrow() {
        pulledArrowObject.SetActive(true);
    }

    /// <returns>True if the player is at shooting stance at the moment.</returns>
    public bool AtShootingPos() { return isShooting; }

    /// <returns>True if the camera is animating at the moment.</returns>
    public bool IsAnimating() { return animator.enabled; }
}