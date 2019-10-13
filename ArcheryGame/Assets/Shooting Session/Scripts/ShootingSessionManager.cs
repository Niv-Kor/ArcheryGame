using Assets.Script_Tools;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using Input = UnityEngine.Input;

public class ShootingSessionManager : MonoBehaviour
{
    [SerializeField] private GameObject drawnArrow;

    private readonly string STANCE_PARAM = "stance";
    private readonly string DRAW_PARAM = "draw";
    private readonly string SHOOT_PARAM = "shoot";

    private Animator animator;
    private RigidbodyFirstPersonController playerController;
    private CameraManager camManager;
    private GameObject player;
    private GameObject arrowInstance, arrowInstanceCam;
    private bool isShooting, isPulling;
    private ProjectileManager projManager;
    
    private void Start() {
        this.animator = gameObject.GetComponent<Animator>();
        this.player = GameObject.FindGameObjectWithTag("Player");
        this.playerController = player.GetComponent<RigidbodyFirstPersonController>();

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
        if (isShooting && !IsAnimating() && camManager.GetMainCameraTag() != CameraEnabler.Tag.Arrow) {
            if (isPulling) {
                //withdraw
                if (Input.GetMouseButtonDown(1)) {
                    //destroy the newly created arrow
                    projManager.DestroyLastSpawned();
                    camManager.DestroyCam(arrowInstanceCam);

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
        if (arrowInstance != null) {
            Transform arrowTransform = drawnArrow.transform;
            arrowInstance.transform.position = arrowTransform.position;
            arrowInstance.transform.eulerAngles = Vector3.zero;
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
        //duplicate arrow instance
        arrowInstance = Instantiate(drawnArrow);
        AlignArrowInstance();

        arrowInstanceCam = ObjectFinder.GetChild(arrowInstance, "Camera");
        arrowInstance.SetActive(false);
        projManager.Spawn(arrowInstance);
        camManager.AddCam(arrowInstanceCam);
        arrowInstanceCam.SetActive(true);
        arrowInstanceCam.GetComponent<ArrowCameraAligner>().enabled = true;
    }

    /// <summary>
    /// Launch the loaded arrow after a draw.
    /// </summary>
    public void Shoot(Vector3 arrowPos, Quaternion arrowRot) {
        animator.SetBool(SHOOT_PARAM, false);
        animator.SetBool(DRAW_PARAM, false);
        ExitCamAnimation(false, CameraEnabler.Tag.Arrow, false);
        arrowInstance.transform.position = arrowPos;
        arrowInstance.transform.rotation = arrowRot;
        arrowInstance.SetActive(true);
        drawnArrow.SetActive(false);
        arrowInstance.GetComponent<ProjectileArrow>().enabled = true; //release the arrow

        //dispose arrow instance variables
        arrowInstance = null;
        arrowInstanceCam = null;
    }

    /// <summary>
    /// Load a new arrow into the bow after a launch.
    /// </summary>
    public void LoadArrow() {
        drawnArrow.SetActive(true);
    }

    /// <returns>True if the player is at shooting stance at the moment.</returns>
    public bool AtShootingPos() { return isShooting; }

    /// <returns>True if the camera is animating at the moment.</returns>
    public bool IsAnimating() { return animator.enabled; }
}