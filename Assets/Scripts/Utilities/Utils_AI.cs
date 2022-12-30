using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


public static class Utils_AI
{
    public static GameObject FindNearestObjectOfType(GameObject caller, Type typeToFind)
    {
        Object[] objects = GameObject.FindObjectsOfType(typeToFind, false);

        // Debug.Log("Objs Found: " + objects.Length);


        float shortestDistance = float.MaxValue;
        GameObject closestObject = null;
        foreach (Object obj in objects)
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

    public static GameObject FindNearestBuildingAtOrBelowTier(GameObject caller, int callerTier)
    {

        Object[] objects = GameObject.FindObjectsOfType(typeof(Building_Base), false);

        // Debug.Log("Objs Found: " + objects.Length);


        float shortestDistance = float.MaxValue;
        GameObject result = null;
        foreach (Object obj in objects)
        {
            GameObject buildingGameObject = obj.GameObject();
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
