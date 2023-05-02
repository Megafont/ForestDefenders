using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// Represents a zone in which the player can construct bridges.
/// </summary>
/// <remarks>
/// NOTE: Bridge construction zones should ALWAYS be setup so that the long side runs along the local Z axis.
///       The local X axis should always be the width of the bridge zone. This is because the code in this class
///       assumes the length of bridges in the zone run along the local Z axis.
/// </remarks>
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



    [Header("Trigger Settings")]
    [SerializeField] private List<TriggerableObjectRef> _TriggeredObjects;



    private GameManager _GameManager;
    private BoxCollider _BridgeZoneCollider;

    private bool _IsBridged;

    private BuildModeManager _BuildModeManager;



    public delegate void BridgeConstructionZone_OnBridgedStateChangedHandler(GameObject sender, bool newState);

    public event BridgeConstructionZone_OnBridgedStateChangedHandler OnBridgedStateChanged;



    private void Awake()
    {
        _GameManager = GameManager.Instance;
        _BuildModeManager = _GameManager.BuildModeManager;

        _GameManager.VillageManager_Buildings.OnBuildingDestroyed += OnBridgeDestroyed;

        _BridgeZoneCollider = GetComponent<BoxCollider>();
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

    public void ApplyConstraints(Transform targetGameObject)
    {
        Vector3 bridgePos = _GhostBridgePosition;
        Vector3 bridgePosRelativeToBridgeZoneCenter = transform.InverseTransformPoint(bridgePos); // Convert position to the bridge zone's local space.

        // Snap bridge to the altitude and z-axis of the bridge construction zone. This means it can slide along the z-axis of the bridge zone to constrain it along the length of the zone.
        bridgePosRelativeToBridgeZoneCenter = new Vector3(0, bridgePosRelativeToBridgeZoneCenter.y, bridgePosRelativeToBridgeZoneCenter.z);


        // Remember, we're working with the bridge construction zone's local coordinate system here.
        bridgePos.y = 0; // Force the bridge to snap to the altitude of the bridge construction zone.
        bridgePos.x = 0; // Force the bridge to stay snapped to the local Z axis of the bridge construction zone by locking the X axis.
        bridgePos.z = bridgePosRelativeToBridgeZoneCenter.z;

        Vector3 rotation = transform.rotation.eulerAngles;


        // If the player has tried to move the construction ghost by more than GHOST_UNLOCK_DISTANCE units from the center of
        // the length of the bridge construction zone, then allow it to unlock from the bridge construction zone.
        // We simply return so that the constraints don't get applied.
        if (!_PrevIsCompletelyInsideResult)
        {
            return;
        }
        

        bridgePos = transform.TransformPoint(bridgePos); // Convert back to world space.

        // Apply the bridge constraints of this bridge construction zone to the transform of the building
        // construction ghost to lock it in this zone.
        targetGameObject.transform.position = bridgePos;
        targetGameObject.transform.rotation = transform.rotation; // Copy the rotation of the bridge zone.

        //Debug.Log($"Applied constraints:    Position: {targetGameObject.position}    Rotation: {targetGameObject.rotation.eulerAngles}");
    }

    public void AddBridge(IBuilding bridge)
    {
        if (bridge == null)
            throw new Exception("The passed in bridge is null!");

        if (bridge.Category != "Bridges")
            throw new Exception($"The passed in building \"{bridge.gameObject.name}\" is not a bridge!");


        _BridgesInThisZone.Add(bridge.gameObject);

        BridgedStateChanged();
    }

    private void OnBridgeDestroyed(IBuilding building, bool wasDeconstructedByPlayer)
    {
        _BridgesInThisZone.Remove(building.gameObject);

        BridgedStateChanged();
    }

    private bool _PrevIsCompletelyInsideResult;
    public bool IsCompletelyInsideBridgeZone(GameObject bridgeObject, BuildingDefinition bridgeDefinition)
    {        
        Vector3 position = bridgeObject.transform.position;
        
        // If the position hasn't changed, simply return the previous result.
        if (position == _GhostBridgePrevPosition)
            return _PrevIsCompletelyInsideResult;

        Vector3 positionRelativeToBridgeZoneCenter = transform.InverseTransformPoint(position); // Convert position to the bridge zone's local space.


        // NOTE: This line originally used _BridgeZoneCollider.bounds.size, but for some reason this returns a bounding box
        // that is larger than that of the actual collider (roughly 2x wider for some reason).
        Vector3 bridgeZoneSize = _BridgeZoneCollider.size;


        // Calculate the distance of the bridge's center from the bridge zone's center point, taking into account half of the bridge width to prevent the player
        // from being able to slide the bridge too far along the bridge zone in either direction.
        float halfBridgeWidth = bridgeDefinition.Size.z / 2;
        //float halfBridgeLength = bridgeDefinition.Size.x / 2;
        float threshHoldZ = (bridgeZoneSize.z / 2) - halfBridgeWidth;


        bool isWithinWidthOfZone = Mathf.Abs(positionRelativeToBridgeZoneCenter.x) <= GHOST_UNLOCK_DISTANCE;
        bool isWithinLengthOfZone = Mathf.Abs(positionRelativeToBridgeZoneCenter.z) <= threshHoldZ;


        if (isWithinWidthOfZone && isWithinLengthOfZone)
        {
            _GhostBridgeDefinition = bridgeDefinition;
            _GhostBridgePrevPosition = _GhostBridgePosition;
            _GhostBridgePosition = position;
            _GhostBridgeXDelta = position.x - transform.position.x;
        }
        else
        {
            _GhostBridgeDefinition = null;
            _GhostBridgePosition = Vector3.zero;
            _GhostBridgePrevPosition = Vector3.zero;
            _GhostBridgeXDelta = 0;
        }
        

        //Debug.Log($"Inside Zone Thresholds:  (X: {threshHoldX}    Z: {threshHoldZ})    X Delta: {_GhostBridgeXDelta}    GhostBridgePos: {_GhostBridgePosition}    Prev Pos: {_GhostBridgePrevPosition}");
        //Debug.Log($"Bridge \"{bridgeObject.name}\" is inside bridge zone:  (Width-wise: {isWithinWidthOfZone}  |  Length-wise: {isWithinLengthOfZone})    bridgeZonePos: {transform.position}    bridgePosRelativeToBridgeZonePos: {positionRelativeToBridgeZoneCenter}    bridgeZoneSize: {_BridgeZoneCollider.bounds.size}");


        _PrevIsCompletelyInsideResult = isWithinWidthOfZone && isWithinLengthOfZone;
        return _PrevIsCompletelyInsideResult;
    }

    private BuildingDefinition _GhostBridgeDefinition;
    private Vector3 _GhostBridgePosition;
    private Vector3 _GhostBridgePrevPosition;
    private float _GhostBridgeXDelta;
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;


        float gizmosHeight = transform.position.y;


        // Draw bridge zone bounding box
        // ====================================================================================================

        Vector3 bridgeZoneSize;
        if (_BridgeZoneCollider == null)
            return;
        else
            bridgeZoneSize = _BridgeZoneCollider.size;


        // Shift bridge's size to be centered around the origin.
        Vector3 min = -(bridgeZoneSize / 2);
        Vector3 max = bridgeZoneSize / 2;

        Vector3 offset2 = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3[] box = new Vector3[4];
        box[0] = transform.rotation * new Vector3(min.x, gizmosHeight, min.z) + offset2;
        box[1] = transform.rotation * new Vector3(max.x, gizmosHeight, min.z) + offset2;
        box[2] = transform.rotation * new Vector3(max.x, gizmosHeight, max.z) + offset2;
        box[3] = transform.rotation * new Vector3(min.x, gizmosHeight, max.z) + offset2;



        // Draw bridge zone bounds
        // ----------------------------------------------------------------------------------------------------
        Gizmos.color = Color.green;
        Gizmos.DrawLine(box[0], box[1]);
        Gizmos.DrawLine(box[1], box[2]);
        Gizmos.DrawLine(box[2], box[3]);
        Gizmos.DrawLine(box[3], box[0]);

        // Draw center point.
        Gizmos.DrawSphere(transform.position, 0.5f);


        // Draw bridge bounding box
        // ====================================================================================================

        Vector3 bridgeSize;
        if (_GhostBridgeDefinition == null)
            return;
        else
            bridgeSize = _GhostBridgeDefinition.Size;

        // Shift bridge's size to be centered around the origin.
        min = -(bridgeSize / 2);
        max = bridgeSize / 2;

        Vector3 offset = new Vector3(_GhostBridgePosition.x, 0, _GhostBridgePosition.z);
        box[0] = transform.rotation * new Vector3(min.x, gizmosHeight, min.z) + offset;
        box[1] = transform.rotation * new Vector3(max.x, gizmosHeight, min.z) + offset;
        box[2] = transform.rotation * new Vector3(max.x, gizmosHeight, max.z) + offset;
        box[3] = transform.rotation * new Vector3(min.x, gizmosHeight, max.z) + offset;


        // Draw bridge bounds.
        // ----------------------------------------------------------------------------------------------------
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(box[0], box[1]);
        Gizmos.DrawLine(box[1], box[2]);
        Gizmos.DrawLine(box[2], box[3]);
        Gizmos.DrawLine(box[3], box[0]);

        // Draw center point.
        Gizmos.DrawSphere(_GhostBridgePosition, 0.5f);
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
            return _IsBridged;
        }
        private set
        {
            _IsBridged = value;
        }
    }



    private void BridgedStateChanged()
    {
        _IsBridged = BridgeCount > 0;

        OnBridgedStateChanged?.Invoke(this.gameObject, IsBridged);

        //Debug.Log("Bridged state changed!");

        // Trigger any triggerable objects as appropriate.
        for (int i = 0; i < _TriggeredObjects.Count; i++)
        {
            TriggerableObjectRef objRef = _TriggeredObjects[i];

            if (objRef.Object != null)
            {
                bool state = !objRef.InvertState ? _IsBridged : !_IsBridged;

                //Debug.Log($"Setting triggerable object \"{objRef.Object.name}\"'s state to {state}.");

                objRef.Object.SetActive(state);
            }
            else
            {
                Debug.LogError($"Triggerable Object at index {i} in bridge construction zone \"{gameObject.name}\" is null! Skipping it.");
            }
        }
    }


    public int BridgeCount {  get { return _BridgesInThisZone.Count; } }

    public LevelAreas PrevArea { get { return _PreviousArea; } }
    public LevelAreas NextArea { get { return _NextArea; } }

}
