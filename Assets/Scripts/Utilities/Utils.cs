using System.Collections;
using System.Collections.Generic;

using UnityEngine;



public enum Directions
{
    North  = 0,
    East,
    South,
    West,
}

public static class Utils
{
    /// <summary>
    /// This array is used by the GetClosestCardinalDirectionToVector() function.
    /// </summary>
    private static Vector3[] _CompareDirections = new Vector3[] { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };


    /// <summary>
    /// Finds the closest cardinal direction to the passed in vector.
    /// </summary>
    /// <remarks>
    /// I learned how to make this function from the following video:
    /// https://www.youtube.com/watch?v=ByTEFq8Til4
    /// </remarks>
    /// <param name="direction">The vector we want to find the closest cardinal direction to.</param>
    public static Directions GetClosestCardinalDirectionToVector(Vector3 direction)
    {
        direction = direction.normalized;

        float closestDistance = -Mathf.Infinity;
        Directions result = Directions.North;

        float dotProduct = 0f;


        for (int i = 0; i < 4; i++)
        {
            dotProduct = Vector3.Dot(direction, _CompareDirections[i]);
            if (dotProduct > closestDistance)
            {
                closestDistance = dotProduct;
                result = (Directions)i;
            }

        } // end for i


        //Debug.Log($"Direction: {direction}    Result: {result}");

        return result;
    }

    public static float RoundToNearestMultiple(float value, float multiple)
    {
        float quotient = (int)(value / multiple); // Get the quotient with no decimal part.
        float remainder = value - (quotient * multiple);

        /*
        if (_CurAdjustPositionState != Vector2.zero && _CurAdjustPositionState != _PrevAdjustPositionState)
            Debug.Log($"Q: {quotient}    R: {remainder}    V: {value}    M: {multiple}");
        */

        if (remainder < multiple / 2)
            return quotient * multiple;
        else
            return (quotient + 1) * multiple;
    }

    public static Vector3 SnapPositionToGrid(Vector3 position, float increment)
    {
        position.x = RoundToNearestMultiple(position.x, increment);
        // We don't snap the y-axis, as this might cause problems such as the object getting stuck in the floor.
        position.z = RoundToNearestMultiple(position.z, increment);

        return position;
    }

    public static void DestroyAllChildGameObjects(GameObject parent)
    {
        // I couldn't get it to delete all child game objects myself, as there were always
        // a couple not getting deleted. The code in this function is a solution I found here:
        // https://stackoverflow.com/questions/46358717/how-to-loop-through-and-destroy-all-children-of-a-game-object-in-unity


        int i = 0;

        //Array to hold all child obj
        GameObject[] allChildren = new GameObject[parent.transform.childCount];

        //Find all child obj and store to that array
        foreach (Transform child in parent.transform)
        {
            allChildren[i] = child.gameObject;
            i += 1;
        }

        //Now destroy them
        foreach (GameObject child in allChildren)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }

    }
}
