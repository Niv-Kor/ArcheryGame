using System;
using System.Runtime.CompilerServices;
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

    [SerializeField] [Range(0.1f, 5f)] private float maxAcceleration = 1f;
    [SerializeField] [Range(0f, 5f)] private float accelerationRate = 0.1f;

    private Animator animator;
    private State currentState;
    private RigidbodyFirstPersonController rigidbody;
    private bool isJumping, leaveJumpKey;
    private CrowdManager[] crowds;
    private float defJump;

    void Start() {
        this.animator = GetComponent<Animator>();
        this.currentState = State.IDLE;
        this.rigidbody = GetComponentInParent<RigidbodyFirstPersonController>();
        rigidbody.movementSettings.RunMultiplier = 0;
        this.defJump = rigidbody.movementSettings.JumpForce;
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
            isJumping = false; //avoid multiple jumps

            if (!uninterruptedState) {
                if (hasInput) {
                    if (pick && !movement) SetState(State.PICK, false);
                    else {
                        if (movement) Move();
                        if (jump) Jump(false);
                    }
                }
                else SetState(State.IDLE, true);
            }
        }
        else if (!isJumping) Jump(true); //force jump while in mid-air
    }

    private void Move() {
        if (Input.GetKey(KeyCode.LeftShift)) Run();
        else SetState(State.WALK, true);
    }

    private void Run() {
        SetState(State.RUN, true);
        Accelerate(true);
    }

    private void Accelerate(bool flag) {
        //slowly accelerate
        float acceleration = rigidbody.movementSettings.RunMultiplier;

        if (flag) {
            if (acceleration + accelerationRate <= maxAcceleration)
                acceleration += accelerationRate;
        }
        else acceleration = 0;

        rigidbody.movementSettings.RunMultiplier = acceleration;

        //increase jump height
        float jumpMultiplier = acceleration / maxAcceleration * 0.5f;
        rigidbody.movementSettings.JumpForce = defJump + defJump * jumpMultiplier;
    }

    private void Jump(bool fall) {

        print("state is fun? " + StateIs(State.RUN));
        print("fall is " + fall);

        animator.SetBool("high_jump", StateIs(State.RUN) && !fall); //use high jump only if current state is Run
        SetState(State.JUMP, true);
        isJumping = true;
    }

    private void SetState(State state, bool interruptable) {
        //cancel acceleration
        if (state != State.RUN) Accelerate(false);

        rigidbody.enabled = interruptable;
        animator.SetInteger("state", state.GetValue());
        currentState = state;
    }

    private bool StateIs(State state) { return state.Is(animator.GetInteger("state")); }
}
