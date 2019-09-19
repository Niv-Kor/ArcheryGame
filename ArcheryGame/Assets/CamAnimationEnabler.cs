using System.Collections;
using System.Collections.Generic;
using Assets.Script_Tools;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class CamAnimationEnabler : StateMachineBehaviour
{
    private CamBehaviour camBahaviour;
    private bool completeRound;

    void OnEnable() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform camera = ObjectFinder.GetChild(player.transform, "MainCamera");
        this.camBahaviour = camera.GetComponent<CamBehaviour>();
        this.completeRound = false;
    }

    public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (!animator.GetBool("shoot")) camBahaviour.ExitToGameplay();
    }

    public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (animator.GetBool("shoot")) camBahaviour.EnterCamMovement();
    }
}
