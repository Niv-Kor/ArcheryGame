using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hider_Manager : MonoBehaviour

{

    public GameObject Sign_1;

    void Update()

    {

        if (GetComponent<Animation>().IsPlaying("wave_1"))

        {

            Sign_1.SetActive(true);

        }

        else

        {

            Sign_1.SetActive(false);

        }

    }

}
