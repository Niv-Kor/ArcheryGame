using System.Collections;
using System.Collections.Generic;
using Assets.Script_Tools;
using UnityEngine;

public class LockedCameraState : StateMachineBehaviour
{
    [SerializeField] private CameraChanger.PlayerCamera exitCamera;
    [SerializeField] private bool disableAnimatorOnExit;
    [SerializeField] private bool zoomOnExit;

    private ShootingSessionManager shootSession;

    private void OnEnable() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject camera = ObjectFinder.GetChild(player, "Camera Monitor");
        this.shootSession = camera.GetComponent<ShootingSessionManager>();
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        shootSession.EnterCamAnimation();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        shootSession.ExitCamAnimation(disableAnimatorOnExit, exitCamera, zoomOnExit);
    }
}
