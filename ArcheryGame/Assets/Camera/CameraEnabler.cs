using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Xml.Serialization;
using UnityEngine;

public class CameraEnabler : MonoBehaviour
{
    public enum Tag {
        None,
        Generic,
        FirstPerson,
        ThirdPerson,
        Arrow
    }

    [SerializeField] public Tag tag = Tag.Generic;
    [SerializeField] private List<GameObject> enableOnActivation;

    private CameraManager camManager;
    private Camera camComponent;
    private AudioListener audioListener;
    private FlareLayer flare;
    private bool init;

    private void Start() {
        if (init) return;
        else init = true;

        this.camComponent = GetComponent<Camera>();
        this.audioListener = GetComponent<AudioListener>();
        this.flare = GetComponent<FlareLayer>();
    }

    public void Activate(bool flag) {
        if (!init) Start();

        camComponent.enabled = flag;
        audioListener.enabled = flag;
        flare.enabled = flag;
        TagAsMain(flag);

        //enable or disable the dependent children
        foreach (GameObject child in enableOnActivation) child.SetActive(flag);
    }

    private void TagAsMain(bool flag) {
        gameObject.tag = flag ? "MainCamera" : "Camera";
    }
}