using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class CamBehaviour : MonoBehaviour
{
    public static readonly Vector3 DEFAULT_POSITION = new Vector3(-0.389986f, 3.31f, 5.709991f);
    public static readonly Vector3 DEFAULT_ROTATION = new Vector3(13.376f, -183.345f, -1.4051f);

    private readonly string SHOOTING_PARAM = "shoot";

    private Animator animator;
    private RigidbodyFirstPersonController playerController;

    void Start() {
        this.animator = gameObject.GetComponent<Animator>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        this.playerController = player.GetComponent<RigidbodyFirstPersonController>();
    }

    private void AlignToDefault() {
        transform.localPosition = DEFAULT_POSITION;
        transform.eulerAngles = DEFAULT_ROTATION;
    }

    public void ExitToGameplay() {
        animator.enabled = false;
        playerController.EnableMouseRotation(true);
        print("1 local pos and rot is " + transform.localPosition + ", " + transform.eulerAngles);
        AlignToDefault();
        print("2 local pos and rot is " + transform.localPosition + ", " + transform.eulerAngles);
    }

    public void EnterCamMovement() {
        AlignToDefault();
        playerController.EnableMouseRotation(false);
    }

    public void SwitchShootingPos() {
        bool on = AtShootingPos();
        animator.SetBool(SHOOTING_PARAM, !on);
        if (!on) animator.enabled = true;
    }

    public bool AtShootingPos() {
        return animator.GetBool(SHOOTING_PARAM);
    }
}