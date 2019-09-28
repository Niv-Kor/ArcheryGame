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
    private GameObject player;
    private GameObject drawnArrowObject, drawnArrow;
    private GameObject arrowInstanceObj, arrowInstance, ArrowInstanceCam;
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
        this.drawnArrowObject = ObjectFinder.GetChild(bow, "Arrow");
        this.drawnArrow = ObjectFinder.GetChild(drawnArrowObject, "Stick");

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

    /// <summary>
    /// Animate the bow states.
    /// </summary>
    private void Animate() {
        if (isShooting && !IsAnimating() && !camManager.FollowingArrow()) {
            if (isPulling) {
                //withdraw
                if (Input.GetMouseButtonDown(1)) {
                    //destroy the newly created arrow
                    projManager.DestroyLastSpawned();
                    camManager.DestroyCam(ArrowInstanceCam);

                    //enable animation
                    EnterCamAnimation(true);
                    animator.SetBool(DRAW_PARAM, false);
                    isPulling = false;
                }
                //shoot
                else if (Input.GetMouseButtonUp(0)) {
                    print("shoot");
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
                    drawnArrow.SetActive(true);
                    animator.SetBool(DRAW_PARAM, true);
                    isPulling = true;
                }
            }
        }
    }

    /// <summary>
    /// Align the instantiated arrow with the actual drawn arrow.
    /// </summary>
    private void AlignArrowInstance() {
        if (arrowInstanceObj != null) {
            Transform arrowTransform = drawnArrowObject.transform;
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

    /// <summary>
    /// Create a new arrow to launch.
    /// The fired arrow should not be the one that's drawn, but rather duplicated one in its place.
    /// </summary>
    private void InstantiateArrow() {
        this.arrowInstanceObj = Instantiate(drawnArrowObject);
        arrowInstanceObj.transform.localScale *= 1.8f;
        AlignArrowInstance();

        this.arrowInstance = ObjectFinder.GetChild(arrowInstanceObj, "Stick");
        this.ArrowInstanceCam = ObjectFinder.GetChild(arrowInstance, "Arrow Camera");
        arrowInstance.GetComponent<MeshRenderer>().enabled = false;
        projManager.Spawn(arrowInstanceObj);
        camManager.AddCam(ArrowInstanceCam);
        ArrowInstanceCam.SetActive(true);
        ArrowInstanceCam.GetComponent<ArrowCameraAligner>().enabled = true;
    }

    /// <summary>
    /// Launch the loaded arrow after a draw.
    /// </summary>
    public void Shoot() {
        animator.SetBool(SHOOT_PARAM, false);
        animator.SetBool(DRAW_PARAM, false);
        arrowInstance.GetComponent<MeshRenderer>().enabled = true; //display the arrow
        arrowInstance.GetComponent<ProjectileArrow>().enabled = true; //release the arrow
        ExitCamAnimation(false, CameraEnabler.Tag.Arrow, false);
        arrowInstance.SetActive(true);
        drawnArrowObject.SetActive(false);
        
        //dispose arrow instance
        arrowInstanceObj = null;
        arrowInstance = null;
        ArrowInstanceCam = null;
    }

    /// <summary>
    /// Load a new arrow into the bow after a launch.
    /// </summary>
    public void LoadArrow() {
        drawnArrowObject.SetActive(true);
    }

    /// <returns>True if the player is at shooting stance at the moment.</returns>
    public bool AtShootingPos() { return isShooting; }

    /// <returns>True if the camera is animating at the moment.</returns>
    public bool IsAnimating() { return animator.enabled; }
}