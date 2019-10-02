using UnityEngine;

public class ShootState : StateMachineBehaviour
{
    [SerializeField] [Range(0, 100)] private float exitAfterPercentage;

    private ShootingSessionManager shootSession;
    private GameObject arrow;
    private Vector3 arrowInitPos;
    private Quaternion arrowInitRot;
    private float time, changeTime;
    private bool shot;

    private void OnEnable() {
        GameObject monitor = GameObject.FindGameObjectWithTag("Player Monitor");
        this.arrow = GameObject.FindGameObjectWithTag("Arrow");
        this.shootSession = monitor.GetComponent<ShootingSessionManager>();
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        arrowInitPos = arrow.transform.position;
        arrowInitRot = arrow.transform.rotation;
        time = 0;
        changeTime = stateInfo.length * exitAfterPercentage / 100;
        shot = false;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (shot) return;

        time += Time.deltaTime;
        if (time >= changeTime) {
            shot = true;
            time = 0; //init for next time
            shootSession.Shoot(arrowInitPos, arrowInitRot);
        }
    }
}
