using System.Collections;
using System.Collections.Generic;
using Assets.Script_Tools;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class LockedCameraState : StateMachineBehaviour
{
    [SerializeField] private CameraEnabler.Tag exitCamera;
    [SerializeField] private bool disableAnimatorOnExit;
    [SerializeField] private bool zoomOnExit;

    protected ShootingSessionManager shootSession;

    private void OnEnable() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject monitor = ObjectFinder.GetChild(player, "Player Monitor");
        this.shootSession = monitor.GetComponent<ShootingSessionManager>();
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        shootSession.EnterCamAnimation(true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        shootSession.ExitCamAnimation(disableAnimatorOnExit, exitCamera, zoomOnExit);
    }
}
