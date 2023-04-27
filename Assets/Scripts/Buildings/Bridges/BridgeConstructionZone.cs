using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class BridgeConstructionZone : MonoBehaviour
{
    // How far the player must move the construction ghost to cause it to unlock from the bridge constuction zone.
    const float GHOST_UNLOCK_DISTANCE = 1f;


    [Header("Zone Settings")]

    [Tooltip("The type of bridge that can be built in this bridge zone.")]
    [SerializeField] private BridgeTypes _AllowedBridgeType;
    [Tooltip("The area that is accessed by building a bridge across this bridge construction zone.")]
    [SerializeField] private LevelAreas _NextArea;
    [Tooltip("The area that is just before this bridge construction zone.")]
    [SerializeField] private LevelAreas _PreviousArea;

    [Tooltip("The list of bridges that have been constructed in this bridge construction zone. You don't need to manually add anything to this list except for bridges you build in the Unity Editor.")]
    [SerializeField] private List<GameObject> _BridgesInThisZone;


    [Header("Bridge Construction Settings")]

    [Tooltip("The Y-axis rotation (in degrees) that bridges will always be locked to when built in this construction zone.")]
    [Range(0f, 359f)]
    [SerializeField] private float _LockRotation = 0f;


    private GameManager _GameManager;



    private void Awake()
    {
        _GameManager = GameManager.Instance;

        _GameManager.VillageManager_Buildings.OnBuildingDestroyed += OnBridgeDestroyed;
    }

    private void Start()
    {
        if (_GameManager.StartWithAllZonesBridged)
        {
           
            string bridgeType = "";
            if (_AllowedBridgeType == BridgeTypes.WoodBridge_10m)
                bridgeType = "Wood Bridge (10m)";
            else if (_AllowedBridgeType == BridgeTypes.WoodBridge_20m)
                bridgeType = "Wood Bridge (20m)";


            GameObject prefab = BuildModeDefinitions.GetBuildingPrefab("Bridges", bridgeType);


            GameObject bridge = Instantiate(prefab, transform);
            if (gameObject == null)
            {
                Debug.LogError($"GameManager.StartWithAllZonesBridged is on, but failed to generate bridge of type \"{bridgeType}\" for bridge zone \"{gameObject.name}\"!");
            }
        }
    }

    public void ApplyConstraints(Transform gameObject)
    {
        Vector3 position = gameObject.transform.position;
        position.y = transform.position.y; // Force the bridge to snap to the y-position of the bridge construction zone.

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

        if (bridge.Category != "Bridges")
            throw new Exception($"The passed in building \"{bridge.gameObject.name}\" is not a bridge!");


        _BridgesInThisZone.Add(bridge.gameObject);
    }

    private void OnBridgeDestroyed(IBuilding building, bool wasDeconstructedByPlayer)
    {
        _BridgesInThisZone.Remove(building.gameObject);
    }
    


    /// <summary>
    /// Returns the type of bridge that is allowed in this bridge construction zone.
    /// </summary>
    public BridgeTypes AllowedBridgeType { get { return _AllowedBridgeType; } }

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


    public int BridgeCount {  get { return _BridgesInThisZone.Count; } }

    public LevelAreas PrevArea { get { return _PreviousArea; } }
    public LevelAreas NextArea { get { return _NextArea; } }

}
