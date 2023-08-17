using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

using UnityObject = UnityEngine.Object;
using Random = UnityEngine.Random;
using Unity.VisualScripting; // This IS needed as UnityObject.GameObject() is from this namespace.


public static class Utils_World
{
    private static GameManager _GameManager;
    private static ResourceManager _ResourceManager;


    public static Vector2Int GetCoordinateInAreasMapSpace(Vector3 worldPos, Bounds terrainBounds, Vector2 areaMapSize)
    {
        // Calculate worldPos relative to the terrain bounds.
        Vector3 relativeCoords = worldPos - terrainBounds.min;

        // Calculate the position as percentages of the way from bounds.min to bounds.max on each axis.        
        Vector2 percentages = new Vector2(relativeCoords.x / terrainBounds.size.x,
                                          relativeCoords.z / terrainBounds.size.z);
        
        //Debug.Log($"WORLD: {worldPos}    REL: {relativeCoords}    PERC: {percentages}        BOUNDS: {terrainBounds}        MIN: {terrainBounds.min}        MAX: {terrainBounds.max}        SIZE: {terrainBounds.size}");
        

        // Calculate the coordinates in the areas map texture. The subraction operation is just there to invert the coordinates, since they are
        // backwards.
        Vector2Int texCoords = new Vector2Int(Mathf.RoundToInt(/*areaMapSize.x - */percentages.x * areaMapSize.x),
                                              Mathf.RoundToInt(/*areaMapSize.y - */percentages.y * areaMapSize.y));

        return texCoords;
    }

    /// <summary>
    /// Uses the areas map and the passed in world position coordinates to determine which area that location is in.
    /// </summary>
    /// <param name="worldPos">The world position to get the parent area of.</param>
    /// <returns>An integer from 1-5 indicating which area the specified point is in. Returns -1 if that point is not within any of the areas.</returns>
    public static LevelAreas DetectAreaNumberFromPosition(Vector3 worldPos)
    {
        if (_GameManager == null)
            _GameManager = GameManager.Instance;


        Vector2Int texCoords = GetCoordinateInAreasMapSpace(worldPos, _GameManager.TerrainBounds, new Vector2(_GameManager.AreasMap.width, _GameManager.AreasMap.height));

        
        Color32 mapColor = _GameManager.AreasMap.GetPixel(texCoords.x, texCoords.y);

        //Debug.Log($"MAP COLOR: {mapColor}");

        if (mapColor.g == 50)
            return LevelAreas.Area1;
        else if (mapColor.g == 100)
            return LevelAreas.Area2;
        else if (mapColor.g == 150)
            return LevelAreas.Area3;
        else if (mapColor.g == 200)
            return LevelAreas.Area4;
        else if (mapColor.g == 250)
            return LevelAreas.Area5;
        else
            return LevelAreas.Unknown;
    }

    public static List<ResourceNode> FindActiveResourceNodesAccessableFromArea(LevelAreas area)
    {
        // Find all areas that are accessable from the starting area.
        List<LevelAreas> accessableAreas = FindAllAccessableLevelAreasFrom(area);

        if (_ResourceManager == null)
            _ResourceManager = GameManager.Instance.ResourceManager;


        // Find all active (non-depleted) resource nodes that are in the accessable areas we just found.
        List<ResourceNode> accessableActiveResourceNodes = new List<ResourceNode>();
        for (int i = 0; i < _ResourceManager.ActiveResourceNodesCount; i++)
        {
            ResourceNode node = _ResourceManager.GetActiveResourceNode(i);
            LevelAreas nodeArea = DetectAreaNumberFromPosition(node.transform.position);

            if (accessableAreas.Contains(nodeArea))
                accessableActiveResourceNodes.Add(node);

        }

        return accessableActiveResourceNodes;
    }

    public static List<ResourceNode> FindAllResourceNodesAccessableFromArea(LevelAreas area)
    {
        // Find all areas that are accessable from the starting area.
        List<LevelAreas> accessableAreas = FindAllAccessableLevelAreasFrom(area);

        if (_ResourceManager == null)
            _ResourceManager = GameManager.Instance.ResourceManager;


        // Find all active (non-depleted) resource nodes that are in the accessable areas we just found.
        List<ResourceNode> accessableResourceNodes = new List<ResourceNode>();
        for (int i = 0; i < _ResourceManager.AllResourceNodesCount; i++)
        {
            ResourceNode node = _ResourceManager.GetResourceNode(i);
            LevelAreas nodeArea = DetectAreaNumberFromPosition(node.transform.position);

            if (accessableAreas.Contains(nodeArea))
                accessableResourceNodes.Add(node);

        }

        return accessableResourceNodes;
    }


    private static List<LevelAreas> FindAllAccessableLevelAreasFrom(LevelAreas startArea)
    {
        // A list of the areas found to be accessable to the specified area.
        List<LevelAreas> accessableAreas = new List<LevelAreas>();
        // A list of all areas that have been traversed so far.
        List<LevelAreas> alreadyTraversedAreas = new List<LevelAreas>();
        // A list of areas whose bridge construction zones still need to be checked.
        List<LevelAreas> areasToCheck = new List<LevelAreas>();


        accessableAreas.Add(startArea);
        alreadyTraversedAreas.Add(startArea);
        areasToCheck.Add(startArea);


        // Find all available resource nodes going forward.
        while (areasToCheck.Count > 0)
        {
            LevelAreas curAreaToCheck = areasToCheck[0];


            if (curAreaToCheck != LevelAreas.Unknown)
            {
                List<BridgeConstructionZone> connectedBridgeZones = FindAllBridgeZonesConnectedToArea(curAreaToCheck);
                foreach (BridgeConstructionZone zone in connectedBridgeZones)
                {
                    // Find the area this bridge construction zone is connected to on the opposite side as curAreaToCheck.
                    LevelAreas possibleNewAreaToCheck = zone.PrevArea != curAreaToCheck ? zone.PrevArea : zone.NextArea;

                    // If this bridge construction zone has at least one bridge, and the area it connects to has
                    // not been traversed yet, then added it to both lists.
                    if (zone.IsBridged && !alreadyTraversedAreas.Contains(possibleNewAreaToCheck))
                    {
                        if (!accessableAreas.Contains(possibleNewAreaToCheck))
                        {
                            accessableAreas.Add(possibleNewAreaToCheck);
                            //Debug.Log($"Found accessable Area: {possibleNewAreaToCheck}");
                        }

                        if (!areasToCheck.Contains(possibleNewAreaToCheck))
                        {
                            areasToCheck.Add(possibleNewAreaToCheck);
                            //Debug.Log($"Found Area to Check: {possibleNewAreaToCheck}");
                        }
                    }

                } // end foreach


                if (!alreadyTraversedAreas.Contains(curAreaToCheck))
                    alreadyTraversedAreas.Add(curAreaToCheck);
            }


            areasToCheck.Remove(curAreaToCheck);
            //Debug.Log($"Areas to Check: {areasToCheck.Count}");

        } // end while


        return accessableAreas;
    }

    private static List<BridgeConstructionZone> FindAllBridgeZonesConnectedToArea(LevelAreas area)
    {
        List<BridgeConstructionZone> connectedBridgeZones = new List<BridgeConstructionZone>();


        foreach (BridgeConstructionZone zone in GameManager.Instance.BridgeConstructionZonesList)
        {
            if (zone.PrevArea == area || zone.NextArea == area)
            {
                //Debug.Log($"Connected Area: {zone.name}");
                connectedBridgeZones.Add(zone);
            }
        }


        return connectedBridgeZones;
    }

    public static GameObject FindNearestObjectOfType(GameObject caller, Type typeToFind)
    {
        UnityObject[] objects = GameObject.FindObjectsOfType(typeToFind, false);

        // Debug.Log("Objects Found: " + objects.Length);


        float shortestDistance = float.MaxValue;
        GameObject closestObject = null;
        foreach (UnityObject uObj in objects)
        {
            GameObject gameObject = uObj.GameObject();

            float distance = Vector3.Distance(caller.transform.position, gameObject.transform.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestObject = gameObject;
            }

        } // end foreach obj


        // Debug.Log($"Closest: \"{closestObject.name}\"");

        return closestObject;
    }

    public static GameObject FindNearestObjectFromList(GameObject caller, Type typeToFind)
    {
        UnityObject[] objects = GameObject.FindObjectsOfType(typeToFind, false);

        // Debug.Log("Objects Found: " + objects.Length);


        float shortestDistance = float.MaxValue;
        GameObject closestObject = null;
        foreach (UnityObject uObj in objects)
        {
            GameObject gameObject = uObj.GameObject();

            float distance = Vector3.Distance(caller.transform.position, gameObject.transform.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestObject = gameObject;
            }

        } // end foreach obj


        // Debug.Log($"Closest: \"{closestObject.name}\"");

        return closestObject;
    }

    public static GameObject FindNearestBuildingAtOrBelowTier(GameObject caller, int callerTier, bool ignoreBridges = false)
    {
        UnityObject[] objects = GameObject.FindObjectsOfType(typeof(Building_Base), false);

        // Debug.Log("Objects Found: " + objects.Length);


        float shortestDistance = float.MaxValue;
        GameObject result = null;
        foreach (UnityObject uObj in objects)
        {
            GameObject buildingGameObject = uObj.GameObject();
            IBuilding buildingComponent = buildingGameObject.GetComponent<IBuilding>();


            if (buildingComponent.Category == "Bridges" && ignoreBridges)
                continue;


            int buildingTier = buildingComponent.GetBuildingDefinition().Tier;

            float distance = Vector3.Distance(caller.transform.position, buildingGameObject.transform.position);

            if (distance < shortestDistance &&
                buildingTier <= callerTier)
            {
                shortestDistance = distance;
                result = buildingGameObject;
            }

        } // end foreach obj


        // Debug.Log($"Closest at or below tier: \"{closestObject.name}\"");

        return result;
    }

    public static Vector3 GetRandomPointAroundTarget(Transform target)
    {
        Vector3 start = Vector3.right * 1.0f;

        Quaternion q = Quaternion.Euler(new Vector3(0, Random.Range(0, 359), 0));

        Vector3 randomPoint = (q * start) + target.position;
        //UnityEngine.Debug.Log($"Random Point: {randomPoint}");

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPoint, out hit, 5.0f, NavMesh.AllAreas);

        return hit.position;
    }

}
