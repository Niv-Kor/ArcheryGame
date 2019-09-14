using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour {
    private class State {
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
    private RigidbodyFirstPersonController rigidbody;
    private bool leaveJumpKey, isJumping;
    private CrowdManager[] crowds;
    private float defJump;

    void Start() {
        this.animator = GetComponent<Animator>();
        this.rigidbody = GetComponentInParent<RigidbodyFirstPersonController>();
        rigidbody.movementSettings.RunMultiplier = 0;
        this.defJump = rigidbody.movementSettings.JumpForce;
        this.leaveJumpKey = true;
        this.isJumping = false;
    }

    void Update() {
        Animate();
    }

    void Animate() {
        bool xMovement = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        bool yMovement = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S);
        bool movement = xMovement || yMovement;
        bool jump = Input.GetKey(KeyCode.Space) && !isJumping && leaveJumpKey;
        bool onGround = rigidbody.Grounded;

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

    private void Move() {
        if (Input.GetKey(KeyCode.LeftShift)) Run();
        else SetState(State.WALK);
    }

    private void Run() {
        SetState(State.RUN);
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

    private void Jump() {
        SetState(State.JUMP);
        isJumping = true;
    }

    private void SetState(State state) {
        //cancel acceleration
        if (state != State.RUN) Accelerate(false);

        animator.SetInteger("state", state.GetValue());
    }
}