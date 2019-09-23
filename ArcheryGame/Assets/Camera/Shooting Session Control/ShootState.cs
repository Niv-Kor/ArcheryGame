using System.Collections;
using System.Collections.Generic;
using Assets.Script_Tools;
using UnityEngine;

public class ShootState : StateMachineBehaviour
{
    private ShootingSessionManager shootSession;

    private void OnEnable() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject monitor = ObjectFinder.GetChild(player, "Player Monitor");
        this.shootSession = monitor.GetComponent<ShootingSessionManager>();
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Debug.Log("enter Enter");
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Debug.Log("enter Exit");
        shootSession.Shoot();
    }
}
