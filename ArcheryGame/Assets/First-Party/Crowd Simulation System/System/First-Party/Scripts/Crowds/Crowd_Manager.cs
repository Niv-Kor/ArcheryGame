using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crowd_Manager : MonoBehaviour

{

    public GameObject[] Crowds;

    public AnimationClip[] Animations;

    void Start()

    {

        Crowds = GameObject.FindGameObjectsWithTag("Crowd");

        Crowd_Random();

    }

    public void Crowd_Idle()

    {

        foreach (GameObject CrowdPerson in Crowds)

        {

            CrowdPerson.GetComponent<Animation>().Play("idle_1");

        }

    }

    public void Crowd_Clap()

    {

        foreach (GameObject CrowdPerson in Crowds)

        {

            CrowdPerson.GetComponent<Animation>().Play("clapping_1");

        }

    }

    public void Crowd_Cheer()

    {

        foreach (GameObject CrowdPerson in Crowds)

        {

            CrowdPerson.GetComponent<Animation>().Play("cheering_1");

        }

    }

    public void Crowd_Wave()

    {

        foreach (GameObject CrowdPerson in Crowds)

        {

            CrowdPerson.GetComponent<Animation>().Play("wave_1");

        }

    }

    public void Crowd_Random()

    {

        foreach (GameObject CrowdPerson in Crowds)

        {

            CrowdPerson.GetComponent<Animation>().clip = Animations[Random.Range(0, Animations.Length)];
            CrowdPerson.GetComponent<Animation>().Play();

        }

    }

}