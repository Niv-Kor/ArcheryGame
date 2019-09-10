using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UninterrupltedStateEvent : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool("uninterrupted", true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool("uninterrupted", false);
    }
}
