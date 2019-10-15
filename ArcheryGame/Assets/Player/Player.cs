using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour
{
    private class State
    {
        public static readonly State IDLE = new State(0);
        public static readonly State WALK = new State(1);
        public static readonly State RUN = new State(2);
        public static readonly State JUMP = new State(3);

        private int value;

        private State(int value) {
            this.value = value;
        }

        public int GetValue() { return value; }
        public bool Is(int val) { return value == val; }
    }

    [SerializeField] [Range(0.1f, 5f)] private float maxAcceleration = 1f;
    [SerializeField] [Range(0f, 5f)] private float accelerationRate = 0.1f;

    private Animator animator;
    private RigidbodyFirstPersonController rigidBody;
    private ShootingSessionManager shootSession;
    private bool leaveJumpKey, isJumping, onShootingSpot;
    private CrowdManager[] crowds;
    private float defJump;

    private void Start() {
        GameObject avatar = transform.Find("Avatar").gameObject;
        this.animator = avatar.GetComponent<Animator>();
        this.rigidBody = GetComponentInParent<RigidbodyFirstPersonController>();
        rigidBody.movementSettings.RunMultiplier = 0;

        GameObject cameraObj = GameObject.FindWithTag("Monitor");
        this.shootSession = cameraObj.GetComponent<ShootingSessionManager>();

        this.defJump = rigidBody.movementSettings.JumpForce;
        this.leaveJumpKey = true;
        this.isJumping = false;
    }

    private void Update() {
        Animate();
        EnterShootingStance();
    }

    private void OnCollisionEnter(Collision collision) {
        //enter or leave shooting position
        if (collision.gameObject.tag.Equals("Shooting Spot")) {
            onShootingSpot = true;
        }
    }

    private void OnCollisionExit(Collision collision) {
        if (collision.gameObject.tag.Equals("Shooting Spot")) {
            onShootingSpot = false;
        }
    }

    /// <summary>
    /// Animate the player as a response to the user's input.
    /// </summary>
    private void Animate() {
        bool xMovement = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        bool yMovement = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S);
        bool movement = xMovement || yMovement;
        bool jump = Input.GetKey(KeyCode.Space) && !isJumping && leaveJumpKey;
        bool onGround = rigidBody.Grounded;

        leaveJumpKey = Input.GetKeyUp(KeyCode.Space); //check if player doesn't hold jump key
        animator.SetBool("on_ground", onGround); //check if player hit ground

        if (onGround) {
            isJumping = false; //prevent multiple jumps

            if (jump || movement) {
                if (movement) Move();
                if (jump) Jump();
            }
            else SetState(State.IDLE);
        }
        else Jump(); //force jump while in mid-air
    }

    /// <summary>
    /// Apply the corrent animation for the current movement style.
    /// </summary>
    private void Move() {
        if (Input.GetKey(KeyCode.LeftShift)) Run();
        else SetState(State.WALK);
    }

    /// <summary>
    /// Apply a running animation with an acceleration system.
    /// </summary>
    private void Run() {
        SetState(State.RUN);
        Accelerate(true);
    }

    /// <summary>
    /// Accelerate slowly as the player runs.
    /// </summary>
    /// <param name="flag">True to enable or false to disable (drops immediately to 0 acceleration)</param>
    private void Accelerate(bool flag) {
        //slowly accelerate
        float acceleration = rigidBody.movementSettings.RunMultiplier;

        if (flag) {
            if (acceleration + accelerationRate <= maxAcceleration)
                acceleration += accelerationRate;
        }
        else acceleration = 0;

        rigidBody.movementSettings.RunMultiplier = acceleration;

        //increase jump height
        float jumpMultiplier = acceleration / maxAcceleration * 0.5f;
        rigidBody.movementSettings.JumpForce = defJump + defJump * jumpMultiplier;
    }

    /// <summary>
    /// Apply jump animation.
    /// </summary>
    private void Jump() {
        SetState(State.JUMP);
        isJumping = true;
    }

    /// <summary>
    /// Set the state of the player manually.
    /// </summary>
    /// <param name="state"></param>
    private void SetState(State state) {
        //cancel acceleration
        if (state != State.RUN) Accelerate(false);

        animator.SetInteger("state", state.GetValue());
    }

    /// <summary>
    /// Enter or exit shooting stance while standing on one of the map's shooting spots.
    /// </summary>
    private void EnterShootingStance() {
        if (Input.GetKeyDown(KeyCode.Tab) && onShootingSpot) shootSession.ToggleShootingStance();
    }
}