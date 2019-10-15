using UnityEngine;

public class LockedCameraState : StateMachineBehaviour
{
    [Tooltip("The camera to swith to after this animation ends.")]
    [SerializeField] private CameraEnabler.Tag exitCamera;

    [Tooltip("Disable the animator when this animation ends.")]
    [SerializeField] private bool disableAnimatorOnExit;

    private ShootingSessionManager shootSession;

    private void OnEnable() {
        GameObject monitor = GameObject.FindGameObjectWithTag("Monitor");
        this.shootSession = monitor.GetComponent<ShootingSessionManager>();
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        shootSession.EnterCamAnimation(true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        shootSession.ExitCamAnimation(disableAnimatorOnExit, exitCamera);
    }
}
