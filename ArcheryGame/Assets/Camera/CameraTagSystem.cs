using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTagSystem : MonoBehaviour
{
    public enum Tag {
        None,
        Generic,
        FirstPerson,
        ThirdPerson,
        Arrow
    }

    [SerializeField] public Tag tag = Tag.Generic;
}