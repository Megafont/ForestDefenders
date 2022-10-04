using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class BuildingConstructionGhost : MonoBehaviour
{
    [Header("General Settings")]
    
    [Tooltip("The minimum distance in front of the player that the construction ghost must be.")]
    public Vector2 MaxMovementDistances = new Vector2(8f, 5f);


    [Header("Grid Snap Settings")]

    [Tooltip("The size of the grid used when grid snap is on.")]
    [Range(0.1f, 2.0f)]
    public float GridSnapIncrement = 1.0f;
    [Tooltip("The rotation increment used when grid snap is on (in degrees).")]
    [Range(1f, 90f)]
    public float GridSnapRotationIncrement = 15f;

    [Tooltip("How much to move the construction ghost forward/back by per frame when grid snap is off.")]
    [Range(0.01f, 1.0f)]
    public float GridSnapOffIncrement = 0.03f;
    [Tooltip("How much to rotate the construction ghost by per frame when grid snap is off (in degrees).")]
    [Range(0.1f, 1.0f)]
    public float GridSnapOffRotationIncrement = 0.5f;


    [Header("Color Settings")]
    
    [Tooltip("The color the building ghost will appear when it is in an unobstructed area.")]
    public Color CanBuildColor = new Color32(0, 255, 0, 128);
    [Tooltip("The color the building ghost will appear when it is in an unobstructed area and grid snap is enabled.")]
    public Color CanBuildWithGridSnapColor = new Color32(0, 128, 255, 128);
    [Tooltip("The color the building ghost will appear when it is in an obstructed area.")]
    public Color ObstructedColor = new Color32(255, 0, 0, 128);
    [Tooltip("The color the building ghost will appear when the player can't afford to construct the selected building.")]
    public Color CantAffordColor = new Color32(100, 0, 0, 128);



    private InputManager _InputManager;

    private GameObject _Player;

    // The MeshCollider can't be set as Trigger if it isn't convex, so we're just using simple colliders instead since it will work fine anyway.
    private BoxCollider _BoxCollider;
    private CapsuleCollider _CapsuleCollider; // We switch to this collider for round buildings.

    private MeshFilter _MeshFilter;
    private MeshRenderer _Renderer;


    private BuildModeManager _BuildModeManager;
    private BuildingDefinition _BuildingDefinition;

    private List<Collider> _OverlappingObjects = new List<Collider>();


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



    /// <summary>
    /// The building ghost will be positioned this far ahead of the player. This value should be half the radius of the largest building or larger.
    /// Buildings that are longer on one side than the other may get too close to the player when rotated if this value is too small.
    /// 
    /// NOTE: The building construction ghost adds another offset on top of this, which is from the player moving the building position.
    /// </summary>
    private const float _CONSTRUCTION_OFFSET_FROM_PLAYER = 2.0f;

    private const float _GROUND_CHECK_RAYCAST_START_HEIGHT = 200f;
    private const float _GROUND_CHECK_RAYCAST_MAX_DISTANCE = 512f;


    private void Update()
    {
        CheckGhostMovementInputs();
        UpdateGhostPositionAndRotation();
        UpdateGhostColor();
    }

    private void OnEnable()
    {
        // Whenever the player enters build mode, this object gets enabled to show the player where their structure will be built.
        // We need to clear the overlapping objects list to ensure there is no erroneous items in it from the last time the player
        // was in build mode. Otherwise, this can prevent the player from being able to build since this script will think there
        // are still collisions in this case.
        _OverlappingObjects.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckCollider(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _OverlappingObjects.Remove(other);

        if (_OverlappingObjects.Count == 0)
            UpdateGhostColor();
    }


    public void Init()
    {
        _InputManager = GameManager.Instance.InputManager;

        _Player = GameManager.Instance.Player;

        _BuildModeManager = GameManager.Instance.BuildModeManager;

        _MeshFilter = GetComponent<MeshFilter>();
        _Renderer = GetComponent<MeshRenderer>();

        _BoxCollider = GetComponent<BoxCollider>();
        _CapsuleCollider = GetComponent<CapsuleCollider>();

        GameManager.Instance.BuildModeManager.SelectBuilding("Defense", "Barricade");
        _Renderer.material.color = CanBuildColor;
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
        _GhostOffsetRelativeToPlayer.x += _CurMovePositionState.x * GridSnapOffIncrement;
        _GhostOffsetRelativeToPlayer.y += _CurMovePositionState.y * GridSnapOffIncrement;

        // Next, check for rotation inputs.
        if (_CurRotateLeftState)
            _GhostRotationInDegrees -= GridSnapOffRotationIncrement;

        if (_CurRotateRightState)
            _GhostRotationInDegrees += GridSnapOffRotationIncrement;


        // Clamp the construction position within range.
        _GhostOffsetRelativeToPlayer.x = Mathf.Clamp(_GhostOffsetRelativeToPlayer.x, -MaxMovementDistances.x, MaxMovementDistances.x);
        _GhostOffsetRelativeToPlayer.y = Mathf.Clamp(_GhostOffsetRelativeToPlayer.y, -MaxMovementDistances.y, MaxMovementDistances.y);


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
            newPos = Utils.SnapPositionToGrid(newPos, GridSnapIncrement);

        transform.position = newPos;


        // Update the rotation of the build ghost.        

        Quaternion q = transform.rotation;
        Vector3 eulerAngles = q.eulerAngles;
        eulerAngles.y = _GhostRotationInDegrees;

        if (_CurGridSnapState)
            eulerAngles.y = Utils.RoundToNearestMultiple(eulerAngles.y, GridSnapRotationIncrement);

        q.eulerAngles = eulerAngles;
        transform.rotation = q;
    }

    /// <summary>
    /// Gets the ground height at the construction ghost's updated position.
    /// If no ground is present, then the current y value is returned.
    /// </summary>
    private float GetGhostYPos(Vector3 newPos)
    {
        Vector3 ghostPos = transform.position;

        float groundHeight = ghostPos.y;


        // Detect ground height.
        if (Physics.Raycast(new Vector3(ghostPos.x, _GROUND_CHECK_RAYCAST_START_HEIGHT, ghostPos.z),
                            Vector3.down,
                            out RaycastHit hitInfo,
                            _GROUND_CHECK_RAYCAST_MAX_DISTANCE,
                            LayerMask.GetMask(new string[] { "Ground" })))
        {
            groundHeight = _GROUND_CHECK_RAYCAST_START_HEIGHT - hitInfo.distance;
            groundHeight += _VerticalOffsetFromGround;
        }


        return groundHeight;
    }

    private void UpdateGhostColor()
    {
        if (!_BuildModeManager.CanAffordBuilding(_BuildingDefinition.ConstructionCosts))
        {
            _Renderer.material.color = CantAffordColor;
        }
        else if (IsObstructed)
        {
            _Renderer.material.color = ObstructedColor;
        }
        else
        {
            _Renderer.material.color = _InputManager.BuildMode.GridSnap ? CanBuildWithGridSnapColor : CanBuildColor;
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
        Bounds newMeshBounds = _MeshFilter.mesh.bounds;


        // These two lines are here intentionally rather than in the if statement below, since the BoxCollider is used in both cases.        
        _BoxCollider.size = newMeshBounds.size;
        float halfHeight = _BoxCollider.size.y / 2;

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
            _CapsuleCollider.radius = _BuildingDefinition.Radius;
        }

    }

    private void CheckCollider(Collider collider)
    {

        //Debug.Log($"Build ghost object collided with GameObject {other.name}!");


        if (collider.tag != "Ground" && 
            collider.tag != "EnemyTargetDetector" && collider.tag != "VillagerTargetDetector")
        {
            //Debug.Log($"Can't build here! Build ghost object collided with GameObject {other.name}!");

            // Make sure we don't end up with duplicate entries in the list, as this can cause the player to be unable to build things
            // since only one entry gets cleared in OnTriggerExit().
            if (!_OverlappingObjects.Contains(collider))
                _OverlappingObjects.Add(collider);
        }


        UpdateGhostColor();

    }



    public bool CanBuild
    {
        get
        {
            // To be able to build, the construction ghost must not be overlapping any obstacles, and the player must have the required resources for construction.
            return _OverlappingObjects.Count == 0 &&
                   _BuildModeManager.CanAffordBuilding(_BuildingDefinition.ConstructionCosts);
        }
    }

    public bool IsObstructed
    {
        get { return _OverlappingObjects.Count > 0; }
    }
    
}
