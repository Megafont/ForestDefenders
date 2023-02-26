using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class BridgeConstructionZone : MonoBehaviour
{
    // How far the player must move the construction ghost to cause it to unlock from the bridge constuction zone.
    const float GHOST_UNLOCK_DISTANCE = 1f;


    [Header("Zone Settings")]

    [Tooltip("The group of resource nodes that is accessed by building a bridge across this bridge construction zone.")]
    [SerializeField] private GameObject _NextResourceNodeRegion;
    [Tooltip("The group of resource nodes that is just before this bridge construction zone.")]
    [SerializeField] private GameObject _PreviousResourceNodeRegion;

    [Tooltip("The list of bridges that have been constructed in this bridge construction zone. You don't need to manually add anything to this list except for bridges you build in the Unity Editor.")]
    [SerializeField] private List<GameObject> _BridgesInThisZone;


    [Header("Bridge Construction Settings")]

    [Tooltip("The Y-axis rotation (in degrees) that bridges will always be locked to when built in this construction zone.")]
    [Range(0f, 359f)]
    [SerializeField] private float _LockRotation = 0f;



    private ResourceManager _ResourceManager;



    private void Awake()
    {
        _ResourceManager = GameManager.Instance.ResourceManager;
    }


    public void ApplyConstraints(Transform gameObject)
    {
        Vector3 position = gameObject.transform.position;
        Vector3 rotation = Vector3.zero;


        if (_LockRotation == 0f || _LockRotation == 180f)
        {
            position.x = transform.position.x;
            rotation.y = _LockRotation;
        }
        else if (_LockRotation == 90f || _LockRotation == 270f)
        {
            position.z = transform.position.z;
            rotation.y = _LockRotation;
        }


        // If the player has tried to move the construction ghost by more than UnlockDistance, then allow
        // it to unlock from the bridge construction zone.
        if (Vector3.Distance(gameObject.position, position) > GHOST_UNLOCK_DISTANCE)
            return;


        // Apply the bridge constraints of this bridge construction zone to the transform of the building
        // construction ghost to lock it in this zone.
        gameObject.transform.position = position;
        gameObject.transform.rotation = Quaternion.Euler(rotation);
    }

    public void AddBridge(IBuilding bridge)
    {
        if (bridge == null)
            throw new Exception("The passed in bridge is null!");

        if (bridge is not Building_WoodBridge)
            throw new Exception($"The passed in building \"{bridge.gameObject.name}\" is not a bridge!");


        _BridgesInThisZone.Add(bridge.gameObject);
        bridge.gameObject.GetComponent<Health>().OnDeath += OnBridgeDestroyed;
    }

    private void OnBridgeDestroyed(GameObject sender, GameObject attacker)
    {
        _BridgesInThisZone.Remove(sender.gameObject);
        sender.GetComponent<Health>().OnDeath -= OnBridgeDestroyed;
    }
    


    /// <summary>
    /// Returns true if there is at least one bridge crossing this zone, or false otherwise.
    /// </summary>
    public bool IsBridged
    {
        get
        {
            return _BridgesInThisZone.Count > 0;
        }
    }


}
