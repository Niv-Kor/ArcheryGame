using UnityEngine;

public class CrowdManager : MonoBehaviour
{
    public GameObject[] crowds;
    public AnimationClip[] animations;

    void Start() {
        crowds = GameObject.FindGameObjectsWithTag("Crowd");

        if (crowds.Length > 0) {
            Animation crowdAnimation = crowds[0].GetComponent<Animation>();
            animations = new AnimationClip[crowdAnimation.GetClipCount()];
            animations[0] = crowdAnimation.GetClip("idle_1");
            animations[1] = crowdAnimation.GetClip("clapping_1");
            animations[2] = crowdAnimation.GetClip("cheering_1");
            animations[3] = crowdAnimation.GetClip("wave_1");
            SetRandom();
        }
    }

    public void Idle() {
        foreach (GameObject CrowdPerson in crowds)
            CrowdPerson.GetComponent<Animation>().Play("idle_1");
    }

    public void Clap() {
        foreach (GameObject CrowdPerson in crowds)
            CrowdPerson.GetComponent<Animation>().Play("clapping_1");
    }

    public void Cheer() {
        foreach (GameObject CrowdPerson in crowds)
            CrowdPerson.GetComponent<Animation>().Play("cheering_1");
    }

    public void Wave() {
        foreach (GameObject CrowdPerson in crowds)
            CrowdPerson.GetComponent<Animation>().Play("wave_1");
    }

    public void SetRandom() {
        foreach (GameObject CrowdPerson in crowds) {
            AnimationClip clip = animations[Random.Range(0, animations.Length)];
            CrowdPerson.GetComponent<Animation>().clip = clip;
            CrowdPerson.GetComponent<Animation>().Play();
        }
    }
}