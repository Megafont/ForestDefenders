using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


public class BuildingConstructionGhost : MonoBehaviour
{
    [Tooltip("How far in front of the player the building ghost should appear.")]
    public float DistanceInFrontOfPlayer = 2.0f;

    [Tooltip("The color the building ghost will appear when it is in an unobstructed area.")]
    public Color CanBuildColor = new Color32(0, 255, 0, 128);
    [Tooltip("The color the building ghost will appear when it is in an obstructed area.")]
    public Color ObstructedColor = new Color32(255, 0, 0, 128);
    [Tooltip("The color the building ghost will appear when the player can't afford to construct the selected building.")]
    public Color CantAffordColor = new Color32(100, 0, 0, 128);



    // The MeshCollider can't be set as Trigger if it isn't convex, so we're just using simple colliders instead since it will work fine anyway.
    private BoxCollider _BoxCollider;
    private CapsuleCollider _CapsuleCollider; // We switch to this collider for round buildings.

    private MeshFilter _MeshFilter;
    private MeshRenderer _Renderer;


    private BuildModeManager _BuildModeManager;
    private BuildingDefinition _BuildingDefinition;

    private List<Collider> _OverlappingObjects = new List<Collider>();



    void Start()
    {
        _BuildModeManager = GameManager.Instance.BuildModeManager;

        _MeshFilter = GetComponent<MeshFilter>();
        _Renderer = GetComponent<MeshRenderer>();

        _BoxCollider = GetComponent<BoxCollider>();
        _CapsuleCollider = GetComponent<CapsuleCollider>();

        GameManager.Instance.BuildModeManager.SelectBuilding("Defense", "Barricade");
        _Renderer.material.color = CanBuildColor;
    }

    private void Update()
    {
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

    /// <summary>
    /// I had to add this event method, because sometimes using just OnTriggerEnter() and OnTriggerExit() doesn't proved enough updates
    /// to keep the building ghost the correct color at al times.
    /// </summary>
    /// <param name="other">The collider that intersected the building ghost.</param>
    private void OnTriggerStay(Collider other)
    {
        //CheckCollider(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _OverlappingObjects.Remove(other);

        if (_OverlappingObjects.Count == 0)
            UpdateGhostColor();
    }

    public void ChangeMesh(Mesh newMesh, BuildingDefinition buildingDef)
    {
        _BuildingDefinition = buildingDef;

        _MeshFilter.mesh = newMesh; // We have to use .sharedMesh here to avoid a Unity error saying you can't access .mesh on a prefab.

        ChangeColliderMesh();
    }

    private void ChangeColliderMesh()
    {
        Bounds newMeshBounds = _MeshFilter.mesh.bounds;


        // These two lines are here intentionally rather than in the if statement below, since the BoxCollider is used in both cases.        
        _BoxCollider.size = newMeshBounds.size;
        float halfHeight = _BoxCollider.size.y / 2;

        // Shift the collider up by half the height of the model so it sits on the ground like the model.
        _BoxCollider.center = new Vector3(transform.position.x, 
                                          halfHeight, 
                                          transform.position.z);


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


        if (collider.tag != "Ground" && collider.tag != "Player" && 
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

    private void UpdateGhostColor()
    {
        if (!_BuildModeManager.CanAffordBuilding(_BuildingDefinition.ConstructionCosts))
            _Renderer.material.color = CantAffordColor;
        else
            _Renderer.material.color = IsObstructed ? ObstructedColor : CanBuildColor;
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
