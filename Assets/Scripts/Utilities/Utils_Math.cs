using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder;

public static class Utils_Math
{
    // The Y-position at which the ground check ray will start.
    public const float GROUND_CHECK_RAYCAST_START_HEIGHT = 200f;
    // The maximum distance the ray will travel straight downwards in an attempt to find the ground.
    public const float GROUND_CHECK_RAYCAST_MAX_DISTANCE = 512f;


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

    /// <summary>
    /// Unlike the Vector2.Angle and Vector3.Angle functions, this one returns the angle as a number from -180 to +180 degrees.
    /// The previously mentioned Angle functions return the smallest angle between the two vectors as a value from 0-180 degrees.
    /// </summary>
    /// <param name="from">The starting vector.</param>
    /// <param name="to">The vector to measure to.</param>
    /// <param name="referenceAxis">The axis the rotation is around.</param>
    /// <returns>The angle between the passed in vectors (0 to 360 degrees).</returns>
    public static float CalculateSignedAngle(Vector3 from, Vector3 to, Vector3 referenceAxis)
    {
        // Get the smallest angle between the two vectors (will be in the range 0 to 180 degrees);
        float angle = Vector3.Angle(from, to);

        // Change it to be in the range -180 to +180 degrees relative to the from vector.
        float sign = Mathf.Sign(Vector3.Dot(referenceAxis, Vector3.Cross(from, to)));
        return angle * sign;
    }

    /// <summary>
    /// Fires a raycast to determine the y-coordinate of the ground at the specified coordinates.
    /// </summary>
    /// <param name="xPos">The X-coordinate of the point to find the ground height at.</param>
    /// <param name="zPos">The Z-coordinate of the point to find the ground height at.</param>
    /// <param name="groundLayersMask">The ground layer(s).</param>
    /// <param name="groundHeight">Outputs the ground height.</param>
    /// <returns>True if the ground height was found, or false if no ground was detected at the specified point.</returns>
    public static bool DetectGroundHeightAtPos(float xPos, float zPos, LayerMask groundLayersMask, out float groundHeight)
    {
        groundHeight = 0f;


        // Detect the ground height at the current ground sample point.
        if (Physics.Raycast(new Vector3(xPos, GROUND_CHECK_RAYCAST_START_HEIGHT, zPos),
                            Vector3.down,
                            out RaycastHit hitInfo,
                            GROUND_CHECK_RAYCAST_MAX_DISTANCE,
                            groundLayersMask))
        {
            groundHeight = GROUND_CHECK_RAYCAST_START_HEIGHT - hitInfo.distance;
            return true;
        }


        // No ground was detected.
        return false;
    }

    /// <summary>
    /// Fires a raycast to query the ground at the given position to determine what area it is in.
    /// </summary>
    /// <param name="xPos">The X-coordinate of the point to find the area at.</param>
    /// <param name="zPos">The Z-coordinate of the point to find the area at.</param>
    /// <param name="groundLayersMask">The ground layer(s).</param>
    /// <param name="parentArea">Outputs the area the specified position is in.</param>
    /// <returns>True if the area was determined, or false if no ground was detected at the specified point or the area could not be determined.
    ///          In the last case, the parentArea out parameter will be set to Unknown.</returns>
    public static bool DetectAreaNumberFromGroundPosition(float xPos, float zPos, LayerMask groundLayersMask, out LevelAreas parentArea)
    {
        parentArea = LevelAreas.Unknown;


        // Detect the ground height at the current ground sample point.
        if (Physics.Raycast(new Vector3(xPos, GROUND_CHECK_RAYCAST_START_HEIGHT, zPos),
                            Vector3.down,
                            out RaycastHit hitInfo,
                            GROUND_CHECK_RAYCAST_MAX_DISTANCE,
                            groundLayersMask))
        {
            string objName = hitInfo.collider.gameObject.name;
            if (objName.Length >= 7)
            {
                parentArea = (LevelAreas) int.Parse(objName.Substring(5, 2));
                return true;
            }
        }


        // No ground was detected.
        return false;
    }

    public static Vector3 CalculateAdjustedTargetPosition(GameObject target)
    {
        // Shift the target position up appropriately so the projectile doesn't go for the character's feet if the target is a monster, the player, or a villager.
        if (target.CompareTag("Player"))
            return target.transform.position + Vector3.up * 0.75f;
        else if (target.CompareTag("Monster") || target.CompareTag("Villager"))
            return target.transform.position + Vector3.up * (target.GetComponent<NavMeshAgent>().height / 2);
        else
            return target.transform.position;
    }

    /// <summary>
    /// Determines if an object is completely inside of a box collider.
    /// </summary>
    /// <param name="collider">The bounds data of the bounding box.</param>
    /// <param name="transformOfCollider">The transform of the bounding box's parent object.</param>
    /// <param name="boundingBoxOfObject">The bounds data of the object we want to know is completely inside the bounding box or not.</param>
    /// <param name="transformOfObject">The transform of the object.</param>
    /// <returns>True if the object is completely inside the bounding box, or false otherwise.</returns>
    public static bool ObjectIsCompletelyInsideBoxCollider(BoxCollider collider, 
                                                           Bounds boundingBoxOfObject, Transform transformOfObject)
    {
        Vector3 objectMinExtent;
        Vector3 objectMaxExtent;

        
        // NOTE: We only convert the object's bounds to world space.
        //       This is because the collider's bounds are already in world space.
        GetExtentsOfBoundingBoxInWorldSpace(boundingBoxOfObject, transformOfObject, out objectMinExtent, out objectMaxExtent);

        //UnityEngine.Debug.Log($"Bounding Box Min: {collider.bounds.min}    Bounding Box Max: {collider.bounds.max}");
        //UnityEngine.Debug.Log($"Object Min:       {objectMinExtent}         Object Max:       {objectMaxExtent}");


        if (collider.bounds.Contains(objectMinExtent) &&
            collider.bounds.Contains(objectMaxExtent))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the minimum and maximum extents of a bounding box.
    /// 
    /// NOTE: It is only necessary to use this function when working with Bounds data that is NOT already in world space.
    ///       If it is, such as in the case of a box collider, then you can just use Bounds.Min/Bounds.Max to get the same information.
    /// </summary>
    /// <param name="bounds">The bounds data of the bounding box to get the min/max extents for.</param>
    /// <param name="transform">The transform of the object owning the bounding box.</param>
    /// <param name="minExtent">A Vector3 containing the lowest x, y, and z values of all corners of the bounding box (aka the bottom, back, left corner).</param>
    /// <param name="maxExtent">A Vector3 containing the highest x, y, and z values of all corners of the bounding box (aka the top, front, right corner).</param>
    public static void GetExtentsOfBoundingBoxInWorldSpace(Bounds bounds, Transform transform, out Vector3 minExtent, out Vector3 maxExtent)
    {
        Vector3[] corners = GetBoundingBoxCornersInWorldSpace(bounds, transform);

        minExtent = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        maxExtent = new Vector3(float.MinValue, float.MinValue, float.MinValue);


        foreach (Vector3 corner in corners)
        {
            minExtent.x = Mathf.Min(corner.x, minExtent.x);
            minExtent.y = Mathf.Min(corner.y, minExtent.y);
            minExtent.z = Mathf.Min(corner.z, minExtent.z);

            maxExtent.x = Mathf.Max(corner.x, maxExtent.x);
            maxExtent.y = Mathf.Max(corner.y, maxExtent.y);
            maxExtent.z = Mathf.Max(corner.z, maxExtent.z);
        }
    }

    /// <summary>
    /// Gets the world space positions of all eight corners of a bounding box.
    /// </summary>
    /// <param name="bounds">The bounding box data.</param>
    /// <param name="transform">The transform of the object owning the bounding box.</param>
    /// <returns>A Vector3 array containing all eight corners of the bounding box in world space.</returns>
    private static Vector3[] GetBoundingBoxCornersInWorldSpace(Bounds bounds, Transform transform)
    {
        Vector3[] corners = new Vector3[8];

        // Transform bounds local points to world space.
        corners[0] = transform.TransformPoint(bounds.min.x, bounds.min.y, bounds.min.z);
        corners[1] = transform.TransformPoint(bounds.min.x, bounds.min.y, bounds.max.z);
        corners[2] = transform.TransformPoint(bounds.min.x, bounds.max.y, bounds.min.z);
        corners[3] = transform.TransformPoint(bounds.min.x, bounds.max.y, bounds.max.z);
        corners[4] = transform.TransformPoint(bounds.max.x, bounds.min.y, bounds.min.z);
        corners[5] = transform.TransformPoint(bounds.max.x, bounds.min.y, bounds.max.z);
        corners[6] = transform.TransformPoint(bounds.max.x, bounds.max.y, bounds.min.z);
        corners[7] = transform.TransformPoint(bounds.max.x, bounds.max.y, bounds.max.z);


        return corners;
    }

    /// <summary>
    /// Gets the raw positions of all eight corners of a bounding box.
    /// </summary>
    /// <param name="bounds">The bounding box data.</param>
    /// <returns>A Vector3 array containing the raw positions of all eight corners of the bounding box.</returns>
    private static Vector3[] GetBoundingBoxCornersUnmodified(Bounds bounds)
    {
        Vector3[] corners = new Vector3[8];

        // Transform bounds local points to world space.
        corners[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
        corners[1] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        corners[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
        corners[3] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        corners[4] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
        corners[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        corners[6] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
        corners[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);


        return corners;
    }    
}
