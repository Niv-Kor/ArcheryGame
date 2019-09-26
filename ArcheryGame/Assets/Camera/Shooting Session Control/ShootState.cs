using System.Collections;
using System.Collections.Generic;
using Assets.Script_Tools;
using UnityEngine;

public class ShootState : StateMachineBehaviour
{
    [SerializeField] [Range(0, 100)] private float exitAfterPercentage;

    private ShootingSessionManager shootSession;
    private float time, changeTime;
    private bool shot;

    private void OnEnable() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject monitor = ObjectFinder.GetChild(player, "Player Monitor");
        this.shootSession = monitor.GetComponent<ShootingSessionManager>();
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        this.time = 0;
        this.changeTime = stateInfo.length * exitAfterPercentage / 100;
        shot = false;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (shot) return;

        time += Time.deltaTime;
        if (time >= changeTime) {
            shot = true;
            time = 0; //init for next time
            shootSession.Shoot();
        }
    }
}
