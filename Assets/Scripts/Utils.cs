using System.Collections;
using System.Collections.Generic;

using UnityEngine;



public static class Utils
{
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
