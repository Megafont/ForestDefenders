using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

using UnityObject = UnityEngine.Object;
using Random = UnityEngine.Random;


public static class Utils_AI
{
    private static ResourceManager _ResourceManager;



    public static GameObject FindNearestObjectOfType(GameObject caller, Type typeToFind)
    {
        UnityObject[] objects = GameObject.FindObjectsOfType(typeToFind, false);

        // Debug.Log("Objs Found: " + objects.Length);


        float shortestDistance = float.MaxValue;
        GameObject closestObject = null;
        foreach (UnityObject obj in objects)
        {
            GameObject gameObject = obj.GameObject();

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

        // Debug.Log("Objs Found: " + objects.Length);


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

    public static List<ResourceNode> FindAllResourceNodesAccessableFromArea(LevelAreas area)
    {
        // Find all areas that are accessable from the starting area.
        List<LevelAreas> accessableAreas = new List<LevelAreas>();
        accessableAreas.AddRange(FindAllAccessableLevelAreasGoingForward(area));
        accessableAreas.AddRange(FindAllAccessableLevelAreasGoingBackard(area));


        if (_ResourceManager == null)
            _ResourceManager = GameManager.Instance.ResourceManager;


        // Find all active (non-depleted) resource nodes that are in the accessable areas we just found.
        List<ResourceNode> accessableResourceNodes = new List<ResourceNode>();
        for (int i = 0; i < _ResourceManager.ActiveResourceNodesCount; i++)
        {
            ResourceNode node = _ResourceManager.GetActiveResourceNode(i);
            Utils_Math.DetectAreaNumberFromGroundPosition(node.transform.position.x, node.transform.position.z, LayerMask.GetMask(new string[] { "Ground" }), out LevelAreas nodeArea);

            if (accessableAreas.Contains(nodeArea))
                accessableResourceNodes.Add(node);

        }


        return accessableResourceNodes;
    }

    private static List<LevelAreas> FindAllAccessableLevelAreasGoingForward(LevelAreas startArea) 
    {
        List<LevelAreas> accessableAreas = new List<LevelAreas>();


        LevelAreas lastArea = (LevelAreas)GameManager.Instance.BridgeConstructionZoneCount;
        LevelAreas curAreaToCheck = startArea;


        // Find all available resource nodes going forward.
        while (true)
        {
            if (curAreaToCheck != LevelAreas.Unknown)
            {
                if (!accessableAreas.Contains(curAreaToCheck))
                {
                    //Debug.Log($"Accessable Area: {curAreaToCheck}");

                    accessableAreas.Add(curAreaToCheck);
                }


                // Check if there is a bridge to the next area. If not, break out of this loop.
                BridgeConstructionZone zoneToCheck = GameManager.Instance.GetBridgeZoneAfterArea(curAreaToCheck);
                if (zoneToCheck != null && zoneToCheck.IsBridged)
                    curAreaToCheck = curAreaToCheck < lastArea ? curAreaToCheck + 1 : LevelAreas.Area1;
                else
                    break;
            }

            // If we've looped back to the starting area, then break out of this loop.
            if (curAreaToCheck == startArea)
                break;
        }


        return accessableAreas;
    }

    private static List<LevelAreas> FindAllAccessableLevelAreasGoingBackard(LevelAreas startArea)
    {
        List<LevelAreas> accessableAreas = new List<LevelAreas>();


        LevelAreas lastArea = (LevelAreas)GameManager.Instance.BridgeConstructionZoneCount;
        LevelAreas curAreaToCheck = startArea;


        // Find all available resource nodes going forward.
        while (true)
        {
            if (curAreaToCheck != LevelAreas.Unknown)
            {
                if (!accessableAreas.Contains(curAreaToCheck))
                {
                    //Debug.Log($"Accessable Area: {curAreaToCheck}");

                    accessableAreas.Add(curAreaToCheck);
                }


                // Check if there is a bridge to the previous area. If not, break out of this loop.
                BridgeConstructionZone zoneToCheck = GameManager.Instance.GetBridgeZoneBeforeArea(curAreaToCheck);
                if (zoneToCheck != null && zoneToCheck.IsBridged)
                    curAreaToCheck = curAreaToCheck > LevelAreas.Area1 ? curAreaToCheck - 1 : lastArea;
                else
                    break;
            }

            // If we've looped back to the starting area, then break out of this loop.
            if (curAreaToCheck == startArea)
                break;
        }


        return accessableAreas;
    }

    public static GameObject FindNearestBuildingAtOrBelowTier(GameObject caller, int callerTier)
    {
        UnityObject[] objects = GameObject.FindObjectsOfType(typeof(Building_Base), false);

        // Debug.Log("Objs Found: " + objects.Length);


        float shortestDistance = float.MaxValue;
        GameObject result = null;
        foreach (UnityObject uObj in objects)
        {
            GameObject buildingGameObject = uObj.GameObject();
            IBuilding buildingComponent = buildingGameObject.GetComponent<IBuilding>();
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
