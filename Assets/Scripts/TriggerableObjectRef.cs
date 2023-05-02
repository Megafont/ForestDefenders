using System;

using UnityEngine;


[Serializable]
public struct TriggerableObjectRef
{
    [Tooltip("The object to be activated or deactivated.")]
    public GameObject Object;
    [Tooltip("If enabled, the object will be deactivated when this bridge zone is bridged.")]
    public bool InvertState;
}
