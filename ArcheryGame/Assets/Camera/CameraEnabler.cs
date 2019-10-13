using System.Collections.Generic;
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

    [Tooltip("The correct type of the camera.")]
    [SerializeField] public Tag cameraTag = Tag.Generic;

    [Tooltip("A list of objects to enable upon activation of this camera\n" +
             "(and disable again upon deactivation)")]
    [SerializeField] private List<GameObject> enableOnActivation;

    [Tooltip("A list of objects to disable upon activation of this camera\n" +
             "(and enable again upon deactivation)")]
    [SerializeField] private List<GameObject> disableOnActivation;

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

    /// <summary>
    /// Activate a camera and enable all its critical components.
    /// </summary>
    /// <param name="flag">True to activate or false to deactivate</param>
    public void Activate(bool flag) {
        if (!init) Start();

        camComponent.enabled = flag;
        audioListener.enabled = flag;
        flare.enabled = flag;
        TagAsMain(flag);

        //toggle the activation of the dependent objects
        foreach (GameObject child in enableOnActivation) child.SetActive(flag);
        foreach (GameObject child in disableOnActivation) child.SetActive(!flag);
    }

    /// <summary>
    /// Tag a camera as the main camera in order to assure its activation in the game.
    /// </summary>
    /// <param name="flag">True to tag as "MainCamera" or false to tag as a regular "Camera"</param>
    private void TagAsMain(bool flag) {
        gameObject.tag = flag ? "MainCamera" : "Camera";
    }
}