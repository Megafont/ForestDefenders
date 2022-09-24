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


}
