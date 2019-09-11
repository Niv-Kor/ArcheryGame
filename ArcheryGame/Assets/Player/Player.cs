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
        public static readonly State PICK = new State(4);

        private int value;

        private State(int value) {
            this.value = value;
        }

        public int GetValue() { return value; }

        public bool Is(int val) { return value == val; }
    }

    private Animator animator;
    private State currentState;
    private RigidbodyFirstPersonController rigidbody;
    private bool isJumping, leaveJumpKey;
    private CrowdManager[] crowds;

    void Start() {
        this.animator = GetComponent<Animator>();
        this.currentState = State.IDLE;
        this.rigidbody = GetComponentInParent<RigidbodyFirstPersonController>();
        this.isJumping = false;
        this.leaveJumpKey = true;

        
    }

    void Update() {
        bool uninterruptedState = animator.GetBool("uninterrupted");
        bool xMovement = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        bool yMovement = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S);
        bool movement = xMovement || yMovement;
        bool jump = Input.GetKey(KeyCode.Space) && !isJumping && leaveJumpKey;
        bool pick = Input.GetKey(KeyCode.Q) && !uninterruptedState;
        bool hasInput = movement || jump || pick;
        bool onGround = rigidbody.Grounded;

        leaveJumpKey = Input.GetKeyUp(KeyCode.Space); //check if player doesn't hold jump key
        animator.SetBool("on_ground", onGround); //check if player hit ground

        if (onGround) {
            isJumping = false; //prevent multiple jumps

            if (!uninterruptedState) {
                if (hasInput) {
                    if (pick && !movement) SetState(State.PICK, false);
                    else {
                        if (movement) Move();
                        if (jump) Jump();
                    }
                }
                else SetState(State.IDLE, true);
            }
        }
        else Jump(); //force jump while in mid-air
    }

    private void Move() {
        if (Input.GetKey(KeyCode.LeftShift)) SetState(State.RUN, true);
        else SetState(State.WALK, true);
    }

    private void Jump() {
        SetState(State.JUMP, true);
        isJumping = true;
    }

    private void SetState(State state, bool interruptable) {
        rigidbody.enabled = interruptable;
        animator.SetInteger("state", state.GetValue());
        currentState = state;
    }
}
