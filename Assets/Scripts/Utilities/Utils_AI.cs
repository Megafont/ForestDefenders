using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;

using Object = UnityEngine.Object;


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
            int buildingTier = buildingComponent.GetBuildingDefinition().BuildingTier;

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

}
