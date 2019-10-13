// 2017 devotid Assets ^^^^^^^^^^^^^^ Simple Rifle Controller Script ^^^^^^^^^^^^^^^^^^
// This script is not intended to be used as a complete "bow controller" it is only meant to show the model is easily animated and ready for your specific application. :)

using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class SimpleBowController : MonoBehaviour {


	Animator m_Animator;  //Fetches the Animator attached to this game object in the start function

	void Start () {
		m_Animator = gameObject.GetComponent<Animator>();  //This gets the Animator, which should be attached to the GameObject you are intending to animate.
	}

	void Update () {
				
		if (Input.GetMouseButtonDown (1)) //Right Click Draws the Bow
			DrawBow();

		if (Input.GetMouseButtonDown (0))  //Left Click Fires the Bow
			FireBow();
		} 

	void DrawBow(){
		m_Animator.SetTrigger ("Draw Bow");
	}

	void FireBow(){
		m_Animator.SetTrigger ("Fire Bow");
	}
}

