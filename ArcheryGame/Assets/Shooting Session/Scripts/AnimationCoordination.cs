using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCoordination : MonoBehaviour
{
    [SerializeField] private GameObject bow;

    private Animator bowAnimator;

    private void Start() {
        this.bowAnimator = bow.GetComponent<Animator>();
    }

    public void DrawBow() {
        bowAnimator.SetTrigger("draw");
    }

    public void FireBow() {
        bowAnimator.SetTrigger("fire");
    }
}