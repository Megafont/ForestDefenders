using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;


public enum Directions
{
    North  = 0,
    East,
    South,
    West,
}

public static class Utils_Misc
{
    /// <summary>
    /// Shuffles a list of any type of object.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list.</typeparam>
    /// <param name="list">The list to shuffle.</param>
    public static void ShuffleList<T>(List<T> list)
    {
        TimeSpan span = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        int secondsSinceMidnight = (int)span.TotalSeconds;

        // Set the random number generator seed based on time so the list order is always different.
        Random.InitState(secondsSinceMidnight);

        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(0, i + 1);

            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        } // en for i

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

    /// <summary>
    /// Checks if the specified LayerMask contains the specified layer.
    /// </summary>
    /// <param name="layerMask">The LayerMask to check.</param>
    /// <param name="layerToCheck">The layer to eheck for.</param>
    /// <returns>True if the LayerMask contains the specified layer, or false otherwise.</returns>
    public static bool LayerMaskContains(LayerMask layerMask, int layerToCheck)
    {
        return layerMask == (layerMask | (1 << layerToCheck));
    }


    private static AnimationCurve _ShrinkCurve;
    public static IEnumerator ShrinkObjectToNothing(Transform transform, float shrinkDuration)
    {
        if (_ShrinkCurve == null)
            InitShrinkAnimCurve();


        float startTime = Time.time;


        while (Time.time - startTime < shrinkDuration)
        {
            float scale = (Time.time - startTime) / shrinkDuration;
            scale = _ShrinkCurve.Evaluate(scale);
            transform.localScale = new Vector3(scale, scale, scale);

            transform.position = transform.position + Vector3.down * 0.01f;

            yield return null; // Wait one frame.
        }

    }

    private static void InitShrinkAnimCurve()
    {
        _ShrinkCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0.75f), new Keyframe(1, 0, -2.5f, 0f));
        _ShrinkCurve.preWrapMode = WrapMode.ClampForever;
        _ShrinkCurve.postWrapMode = WrapMode.ClampForever;
        _ShrinkCurve.SmoothTangents(1, .5f);
    }

}