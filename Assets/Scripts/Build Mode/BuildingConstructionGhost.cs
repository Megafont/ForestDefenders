using System;
using System.Collections.Generic;

using UnityEngine;


public class BuildingConstructionGhost : MonoBehaviour
{
    [Header("General Settings")]
    
    [Tooltip("The minimum distance in front of the player that the construction ghost must be.")]
    [SerializeField] private Vector2 _MaxMovementDistances = new Vector2(8f, 5f);


    [Header("Grid Snap Settings")]

    [Tooltip("The size of the grid used when grid snap is on.")]
    [Range(0.1f, 2.0f)]
    [SerializeField] private float _GridSnapIncrement = 1.0f;

    [Tooltip("The rotation increment used when grid snap is on (in degrees).")]
    [Range(1f, 90f)]
    [SerializeField] private float _GridSnapRotationIncrement = 15f;

    [Tooltip("How much to move the construction ghost forward/back by per frame when grid snap is off.")]
    [Range(0.01f, 5.0f)]
    [SerializeField] private float _GridSnapOffIncrement = 0.1f;

    [Tooltip("How much to rotate the construction ghost by per frame when grid snap is off (in degrees).")]
    [Range(0.1f, 5.0f)]
    [SerializeField] private float _GridSnapOffRotationIncrement = 1f;


    [Header("Color Settings")]
    
    [Tooltip("The color the building ghost will appear when it is in an unobstructed area.")]
    [SerializeField] private Color _CanBuildColor = new Color32(0, 255, 0, 128);

    [Tooltip("The color the building ghost will appear when it is in an unobstructed area and grid snap is enabled.")]
    [SerializeField] private Color _CanBuildWithGridSnapColor = new Color32(0, 128, 255, 128);

    [Tooltip("The color the building ghost will appear when it is in an obstructed area.")]
    [SerializeField] private Color _ObstructedColor = new Color32(255, 0, 0, 128);

    [Tooltip("The color the building ghost will appear when the player can't afford to construct the selected building.")]
    [SerializeField] private Color _CantAffordColor = new Color32(100, 0, 0, 128);



    private GameManager _GameManager;
    private InputManager _InputManager;

    private GameObject _Player;

    // The MeshCollider can't be set as Trigger if it isn't convex, so we're just using simple colliders instead since it will work fine anyway.
    private BoxCollider _BoxCollider;
    private CapsuleCollider _CapsuleCollider; // We switch to this collider for round buildings.

    private MeshFilter _MeshFilter;
    private MeshRenderer _Renderer;


    private BuildModeManager _BuildModeManager;
    private BuildingDefinition _BuildingDefinition;

    private List<Collider> _OverlappingObjects;


    private Vector2 _CurMovePositionState;
    private Vector2 _PrevMovePositionState;

    private bool _CurRotateLeftState;
    private bool _PrevRotateLeftState;

    private bool _CurRotateRightState;
    private bool _PrevRotateRightState;

    private bool _CurGridSnapState;
    private bool _PrevGridSnapState; // The grid snap state of the previous frame.

    private Vector3 _GhostBasePosition; // The base position of the ghost based on the player (before the player's custom Z-shift is applied).

    private float _GhostRotationInDegrees; // The rotation of the building ghost in degrees.
    private Vector2 _GhostOffsetRelativeToPlayer; // How much the player has shifted the building position.
    private float _VerticalOffsetFromGround = 0.02f; // For displacing the construction ghost up a little to prevent it from colliding with the ground when it is on flat surfaces.

    private List<Vector3> _GroundSamplePoints;

    private bool _IsBridgeCompletelyInsideBridgeZone; // Whether or not the construction ghost is completely inside the bounds of _CurrentBridgeZone.



    /// <summary>
    /// The building ghost will be positioned this far ahead of the player. This value should be half the radius of the largest building or larger.
    /// Buildings that are longer on one side than the other may get too close to the player when rotated if this value is too small.
    /// 
    /// NOTE: The building construction ghost adds another offset on top of this, which is from the player moving the building position.
    /// </summary>
    private const float _CONSTRUCTION_OFFSET_FROM_PLAYER = 2.0f;



    private void Awake()
    {
        _GroundSamplePoints = new List<Vector3>();
        _OverlappingObjects = new List<Collider>();
    }

    void Update()
    {
        if (!_BuildModeManager.IsSelectingBuilding)
        {
            CheckGhostMovementInputs();
            UpdateGhostPositionAndRotation();
        }

        UpdateGhostColor();
    }

    void OnEnable()
    {
        // Whenever the player enters build mode, this object gets enabled to show the player where their structure will be built.
        // We need to clear the overlapping objects list to ensure there is no erroneous items in it from the last time the player
        // was in build mode. Otherwise, this can prevent the player from being able to build since this script will think there
        // are still collisions in this case.
        _OverlappingObjects.Clear();
    }

    void OnTriggerStay(Collider other)
    {
        CheckCollider(other);
    }

    void OnTriggerExit(Collider other)
    {
        _OverlappingObjects.Remove(other);


        if (ParentBridgeConstructionZone != null && other.gameObject == ParentBridgeConstructionZone.gameObject)
        {
            ParentBridgeConstructionZone = null;
            _IsBridgeCompletelyInsideBridgeZone = false;
        }


        if (_OverlappingObjects.Count == 0)
            UpdateGhostColor();
    }



    public void Init()
    {
        _GameManager = GameManager.Instance;
        _InputManager = _GameManager.InputManager;

        _Player = _GameManager.Player;

        _BuildModeManager = _GameManager.BuildModeManager;

        _MeshFilter = GetComponent<MeshFilter>();
        _Renderer = GetComponent<MeshRenderer>();

        _BoxCollider = GetComponent<BoxCollider>();
        _CapsuleCollider = GetComponent<CapsuleCollider>();

        _BuildModeManager.SelectBuilding("Walls", "Simple Fence");

        _Renderer.material.color = _CanBuildColor;
    }

    private void CheckGhostMovementInputs()
    {
        _PrevMovePositionState = _CurMovePositionState;
        _CurMovePositionState = _InputManager.BuildMode.MoveBuildPosition;

        _PrevRotateLeftState = _CurRotateLeftState;
        _CurRotateLeftState = _InputManager.BuildMode.RotateBuildLeft;

        _PrevRotateRightState = _CurRotateRightState;
        _CurRotateRightState = _InputManager.BuildMode.RotateBuildRight;

        _PrevGridSnapState = _CurGridSnapState;
        _CurGridSnapState = _InputManager.BuildMode.GridSnap;


        // First check for movement inputs.
        _GhostOffsetRelativeToPlayer.x += _CurMovePositionState.x * _GridSnapOffIncrement;
        _GhostOffsetRelativeToPlayer.y += _CurMovePositionState.y * _GridSnapOffIncrement;

        // Next, check for rotation inputs.
        if (_CurRotateLeftState)
            _GhostRotationInDegrees -= _GridSnapOffRotationIncrement;

        if (_CurRotateRightState)
            _GhostRotationInDegrees += _GridSnapOffRotationIncrement;


        // Clamp the construction position within range.
        _GhostOffsetRelativeToPlayer.x = Mathf.Clamp(_GhostOffsetRelativeToPlayer.x, -_MaxMovementDistances.x, _MaxMovementDistances.x);
        _GhostOffsetRelativeToPlayer.y = Mathf.Clamp(_GhostOffsetRelativeToPlayer.y, -_MaxMovementDistances.y, _MaxMovementDistances.y);


        _GhostRotationInDegrees = _GhostRotationInDegrees % 360;
    }

    private void UpdateGhostPositionAndRotation()
    {                                      
        // Update the position of the build ghost.

        Vector3 newPos = _GhostBasePosition;
        newPos += new Vector3(_GhostOffsetRelativeToPlayer.x,
                              _Player.transform.position.y,
                              _GhostOffsetRelativeToPlayer.y);
        newPos.y = GetGhostYPos(newPos);

        if (_CurGridSnapState)
            newPos = Utils_Math.SnapPositionToGrid(newPos, _GridSnapIncrement);

        transform.position = newPos;


        // Update the rotation of the build ghost.        

        Quaternion q = transform.rotation;
        Vector3 eulerAngles = q.eulerAngles;
        eulerAngles.y = _GhostRotationInDegrees;

        if (_CurGridSnapState)
            eulerAngles.y = Utils_Math.RoundToNearestMultiple(eulerAngles.y, _GridSnapRotationIncrement);

        q.eulerAngles = eulerAngles;
        transform.rotation = q;


        if (_IsBridgeCompletelyInsideBridgeZone)
            ParentBridgeConstructionZone.ApplyConstraints(transform);
    }

    /// <summary>
    /// Gets the ground height at the construction ghost's updated position.
    /// If no ground is present, then the current y value is returned.
    /// When ground is present, the returned height is the average of the appropriate ground sample points
    /// that are arranged in a 3x3 square the same size as the bounding box of the mesh.
    /// </summary>
    private float GetGhostYPos(Vector3 newPos)
    {
        float groundHeight;


        // Get the necessary ground sample points depending on which type of building is currently selected
        // and calculate the ground's Y position under the construction ghost.
        if (BuildModeDefinitions.BuildingIsBridge(_BuildingDefinition)) // Is the building a bridge?
        {
            GetLeftAndRightCenterGroundSamplePoints_XandZCoords();
            groundHeight = CalculateGroundHeight(CalculateGroundPositionOps.Max);
        }
        else
        {
            GetCenterGroundSamplePoint_XandZCoords();
            groundHeight = CalculateGroundHeight(CalculateGroundPositionOps.Average);
        }


        return groundHeight;
    }

    private enum CalculateGroundPositionOps { Average = 0, Min, Max };
    private float CalculateGroundHeight(CalculateGroundPositionOps operation)
    {
        float groundHeight = 0;
        float groundHeightsSum = 0;
        float min = float.MaxValue;
        float max = float.MinValue;

        Vector3 ghostPos = transform.position;


        for (int i = 0; i < _GroundSamplePoints.Count; i++)
        {
            // Detect the ground height at the current ground sample point.
            if (Utils_Math.DetectGroundHeightAtPos(_GroundSamplePoints[i].x,
                                                   _GroundSamplePoints[i].z,
                                                   LayerMask.GetMask(new string[] { "Ground" }),
                                                   out groundHeight))
            {
                groundHeight += _VerticalOffsetFromGround;
            }
            else // No ground was detected at the current ground sample point, so just use the construction ghost's own y coordinate.
            {
                groundHeight = ghostPos.y;
            }


            // Update the ground heights sum depending on the specified operation type.
            if (operation == CalculateGroundPositionOps.Average)
                groundHeightsSum += groundHeight;
            else if (operation == CalculateGroundPositionOps.Min)
                min = Mathf.Min(groundHeight, min);
            else if (operation == CalculateGroundPositionOps.Max)
                max = Mathf.Max(groundHeight, max);


            // Display vertical debug lines showing where the sample points are below the building.
            Debug.DrawLine(new Vector3(_GroundSamplePoints[i].x, Utils_Math.GROUND_CHECK_RAYCAST_START_HEIGHT, _GroundSamplePoints[i].z),
                           new Vector3(_GroundSamplePoints[i].x, groundHeight, _GroundSamplePoints[i].z),
                           new Color32(0, (byte)(75 * (i / 3) + 25), 0, 255)); // The green channel increments by 75 moving from lowest Z row to highest Z row of points (and 25 is added so the brightest lines are green = 255).
        
        } // end for _GroundSamplePoints



        if (operation == CalculateGroundPositionOps.Average)
            return groundHeightsSum / _GroundSamplePoints.Count;
        else if (operation == CalculateGroundPositionOps.Min)
            return min;
        else if (operation == CalculateGroundPositionOps.Max)
            return max;
        else // NOTE: This case should never actually be able run and is just here to make the compiler stop whining that not all code paths return a value.
            throw new Exception("An unexpected error occurred!");
    }

    private void GetAllGroundSamplePoints_XandZCoords()
    {
        _GroundSamplePoints.Clear();


        Bounds meshBounds = _MeshFilter.mesh.bounds;


        // Get nine sample points, one at each corner of the bounding box, one in the center of each side, and one in the center of the bounding box.
        // We go in rows of three from back to front on the Z-axis.
        // We also call TransformPoint() in order to convert each point into world space.
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.min.x,     0, meshBounds.min.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.center.x,  0, meshBounds.min.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.max.x,     0, meshBounds.min.z)));

        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.min.x,     0, meshBounds.center.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.center.x,  0, meshBounds.center.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.max.x,     0, meshBounds.center.z)));

        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.min.x,     0, meshBounds.max.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.center.x,  0, meshBounds.max.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.max.x,     0, meshBounds.max.z)));
    }

    private void GetForwardAndBackGroundSamplePoints_XandZCoords()
    {
        _GroundSamplePoints.Clear();


        Bounds meshBounds = _MeshFilter.mesh.bounds;


        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.min.x,     0, meshBounds.min.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.center.x,  0, meshBounds.min.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.max.x,     0, meshBounds.min.z)));

        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.min.x,     0, meshBounds.max.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.center.x,  0, meshBounds.max.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.max.x,     0, meshBounds.max.z)));
    }

    private void GetLeftAndRightGroundSamplePoints_XandZCoords()
    {
        _GroundSamplePoints.Clear();


        Bounds meshBounds = _MeshFilter.mesh.bounds;


        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.min.x,    0, meshBounds.min.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.min.x,    0, meshBounds.center.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.min.x,    0, meshBounds.max.z)));

        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.max.x,    0, meshBounds.min.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.max.x,    0, meshBounds.center.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.max.x,    0, meshBounds.max.z)));
    }

    private void GetCornerGroundSamplePoints_XandZCoords()
    {
        _GroundSamplePoints.Clear();


        Bounds meshBounds = _MeshFilter.mesh.bounds;


        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.min.x, 0, meshBounds.min.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.max.x, 0, meshBounds.min.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.min.x, 0, meshBounds.max.z)));
        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.max.x, 0, meshBounds.max.z)));
    }

    private void GetForwardAndBackCenterGroundSamplePoints_XandZCoords()
    {
        _GroundSamplePoints.Clear();


        Bounds meshBounds = _MeshFilter.mesh.bounds;


        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.center.x, 0, meshBounds.min.z)));

        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.center.x, 0, meshBounds.max.z)));
    }

    private void GetLeftAndRightCenterGroundSamplePoints_XandZCoords()
    {
        _GroundSamplePoints.Clear();


        Bounds meshBounds = _MeshFilter.mesh.bounds;


        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.min.x, 0, meshBounds.center.z)));

        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.max.x, 0, meshBounds.center.z)));
    }

    private void GetCenterGroundSamplePoint_XandZCoords()
    {
        _GroundSamplePoints.Clear();


        Bounds meshBounds = _MeshFilter.mesh.bounds;


        _GroundSamplePoints.Add(transform.TransformPoint(new Vector3(meshBounds.center.x, 0, meshBounds.center.z)));
    }

    private void UpdateGhostColor()
    {
        if (IsObstructed())
        {
            _Renderer.material.color = _ObstructedColor;
        }
        else if (!_BuildModeManager.CanAffordBuilding(_BuildingDefinition.ConstructionCosts) && 
                 !_GameManager.ConstructionIsFree)
        {
            _Renderer.material.color = _CantAffordColor;
        }
        else
        {
            _Renderer.material.color = _InputManager.BuildMode.GridSnap ? _CanBuildWithGridSnapColor : _CanBuildColor;
        }
    }

    public void ResetTransform()
    {
        ResetPosition();
        ResetRotation();
    }

    /// <summary>
    /// Places the construction ghost in front of the player's position and slightly above the ground.
    /// This way it won't collide with the ground as long as it is on a flat surface.
    /// </summary>
    /// <param name="basePosition"></param>
    private void ResetPosition()
    {       
        // Update the construction ghost's base position. The forward offset is left out here on purpose (see the next comments).
        _GhostBasePosition = _Player.transform.position;
        _GhostBasePosition.y += _VerticalOffsetFromGround;

        // Set the construction ghost offset a bit above the player. We didn't include this in the ghost base position above, because the player is supposed to be
        // the center of the construction ghost's movement range. Including it there would offset that center point to be a bit ahead of the player.
        _GhostOffsetRelativeToPlayer = new Vector2(0, _CONSTRUCTION_OFFSET_FROM_PLAYER);


        //Debug.Log($"Ghost base pos: {_GhostBasePosition}    Player pos: {_Player.transform.position}    Offset: {_GhostOffsetRelativeToPlayer}");
    }

    private void ResetRotation()
    {
        Quaternion q = new Quaternion();
        q.eulerAngles = Vector3.zero;

        transform.rotation = q;
    }

    public void ChangeMesh(Mesh newMesh, BuildingDefinition buildingDef)
    {
        _BuildingDefinition = buildingDef;

        _MeshFilter.mesh = newMesh; // We have to use .sharedMesh here to avoid a Unity error saying you can't access .mesh on a prefab.

        ChangeColliderMesh();
        ResetRotation();
    }

    private void ChangeColliderMesh()
    {
        Bounds newMeshBounds = new Bounds(Vector3.zero, _BuildingDefinition.Size);


        // These two lines are here intentionally rather than in the if statement below, since the BoxCollider is used in both cases.        
        _BoxCollider.size = newMeshBounds.size;
        float halfHeight = newMeshBounds.size.y / 2;

        // Shift the collider up by half the height of the model so it sits on the ground like the model.
        _BoxCollider.center = new Vector3(0,//transform.position.x, 
                                          halfHeight,
                                          0);//transform.position.z);

        
        if (!_BuildingDefinition.IsRound)
        {
            // This is not a round building, so use the box collider.
            _BoxCollider.enabled = true;
            _CapsuleCollider.enabled = false;
        }
        else
        {
            _CapsuleCollider.enabled = true;
            _BoxCollider.enabled = true; // We enable the box collider too, because we want the top and bottom to be flat (this more or less creates a cylinder collider).
            _CapsuleCollider.center = new Vector3(0, halfHeight, 0); // Shift the collider up by half the height of the model so it sits on the ground like the model.
            _CapsuleCollider.height = newMeshBounds.size.y;
            _CapsuleCollider.radius = newMeshBounds.size.x / 2;
        }

    }

    private void CheckCollider(Collider collider)
    {

        //Debug.Log($"Build ghost object collided with GameObject {collider.name}, {collider.tag}!");


        _IsBridgeCompletelyInsideBridgeZone = false;
        

        // Check if we are completely inside a bridge construction zone while trying to build a bridge.
        if (BuildModeDefinitions.BuildingIsBridge(_BuildingDefinition) &&
            collider.CompareTag("BridgeConstructionZone") &&
            Utils_Math.ObjectIsCompletelyInsideBoxCollider((BoxCollider) collider, _MeshFilter.mesh.bounds, transform))
        {
            //Debug.Log("Can build bridge!");

            ParentBridgeConstructionZone = collider.GetComponent<BridgeConstructionZone>();
            _IsBridgeCompletelyInsideBridgeZone = true;

        }
        // Add the collider we hit to the list of obstructions if it is not already in the list.
        else if (!_OverlappingObjects.Contains(collider))
        {
            // Make sure we don't end up with duplicate entries in the list, as this can cause the player to be unable to build things
            // since only one entry gets cleared in OnTriggerExit().
            _OverlappingObjects.Add(collider);
        }


        UpdateGhostColor();

    }

    public bool IsObstructed()
    {
        bool result = false;


        // Is the construction ghost free of collisions with any other object?
        if (_OverlappingObjects.Count < 1)
        {
            // The building is not overlapping anything else, and is not a bridge within a bridge construction zone.
            // In this case, it is not considered obstructed unless it is a bridge that is outside a bridge construction zone.
            if (BuildModeDefinitions.BuildingIsBridge(_BuildingDefinition))
                result = true;
            else
                result = false;
        }
        else if (_IsBridgeCompletelyInsideBridgeZone) // Is the construction ghost completely within a bridge construction zone?
        {
            if (_OverlappingObjects.Count == 1) // Is the construction ghost free of collisions with anything other than the bridge construction zone?
            {
                // The building is overlapping a bridge zone.
                // If IsBridgeCompletelyInsideBridgeZone is true, it means the building IS a bridge AND it is completely within this bridge construction zone.
                result = !_IsBridgeCompletelyInsideBridgeZone;
            }

        }
        else
        {
            // The building is obstructed since it is overlapping one or more other things.
            result = true;
        }


        return result;
    }


    public Vector3 BuildPosition
    {
        get
        {
            // Cancel out the vertical offset of the construction ghost so the building position is flush with the ground.
            return transform.position - new Vector3(0, _VerticalOffsetFromGround, 0);
        }
    }

    public bool CanBuild
    {
        get
        {
            // To be able to build, the construction ghost must not be overlapping any obstacles, and the player must have the required resources for construction.
            return !IsObstructed() &&
                   (_BuildModeManager.CanAffordBuilding(_BuildingDefinition.ConstructionCosts) || _GameManager.ConstructionIsFree);
        }
    }
    
    /// <summary>
    /// Returns the bridge construction zone the construction ghost is currently overlapping, or null
    /// if it is not overlapping one.
    /// </summary>
    public BridgeConstructionZone ParentBridgeConstructionZone { get; private set; }


}
