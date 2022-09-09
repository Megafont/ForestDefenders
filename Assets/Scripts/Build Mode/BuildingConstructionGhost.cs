using System.Collections.Generic;
using UnityEngine;

public class BuildingConstructionGhost : MonoBehaviour
{
    public Color CanBuildColor = new Color32(0, 255, 0, 128);
    public Color BuildingBlockedColor = new Color32(255, 0, 0, 128);


    public bool CanBuild { get { return _OverlappingObjects.Count == 0; } }



    private List<Collider> _OverlappingObjects = new List<Collider>();

    // The MeshCollider can't be set as Trigger if it isn't convex, so we're just using simple colliders instead since it will work fine anyway.
    private BoxCollider _BoxCollider;
    private CapsuleCollider _CapsuleCollider; // We switch to this collider for round buildings.

    private MeshFilter _MeshFilter;
    private MeshRenderer _Renderer;



    private void Start()
    {
        _BoxCollider = GetComponent<BoxCollider>();
        _CapsuleCollider = GetComponent<CapsuleCollider>();

        _MeshFilter = GetComponent<MeshFilter>();
        _Renderer = GetComponent<MeshRenderer>();

        GameManager.Instance.BuildModeManager.SelectBuilding("Defense", "Barricade");
        _Renderer.material.color = CanBuildColor;
    }



    public void ChangeMesh(Mesh newMesh, BuildingDefinition buildingDef)
    {
        _MeshFilter.sharedMesh = newMesh; // We have to use .sharedMesh here to avoid a Unity error saying you can't access .mesh on a prefab.


        // These two lines are here intentionally rather than in the if statement below, since the BoxCollider is used in both cases.        
        _BoxCollider.size = newMesh.bounds.size;
        _BoxCollider.center = new Vector3(0, _BoxCollider.size.y / 2, 0); // Shift the collider up by half the height of the model so it sits on the ground like the model.


        if (!buildingDef.IsRound)
        {
            // This is not a round building, so use the box collider.
            _BoxCollider.enabled = true;
            _CapsuleCollider.enabled = false;
        }
        else
        {
            _CapsuleCollider.enabled = true;
            _BoxCollider.enabled = true; // We enable the box collider too, because we want the top and bottom to be flat (this more or less creates a cylinder collider).
            _CapsuleCollider.center = new Vector3(0, _BoxCollider.size.y / 2, 0); // Shift the collider up by half the height of the model so it sits on the ground like the model.
            _CapsuleCollider.height = newMesh.bounds.size.y;
            _CapsuleCollider.radius = buildingDef.Radius;
        }
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
        //Debug.Log($"Build ghost object collided with GameObject {other.name}!");

        if (other.tag != "Ground" && other.tag != "Player" && other.tag != "EnemyTargetDetector")
        {
            //Debug.Log($"Can't build here! Build ghost object collided with GameObject {other.name}!");

            // Make sure we don't end up with duplicate entries in the list, as this can cause the player to be unable to build things
            // since only one entry gets cleared in OnTriggerExit().
            if (!_OverlappingObjects.Contains(other))
            {
                _OverlappingObjects.Add(other);
                UpdateGhostColor();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _OverlappingObjects.Remove(other);

        if (_OverlappingObjects.Count == 0)
            UpdateGhostColor();
    }    

    private void UpdateGhostColor()
    {
        _Renderer.material.color = CanBuild ? CanBuildColor : BuildingBlockedColor;
    }

}
