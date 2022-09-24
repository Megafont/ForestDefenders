using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class ResourceManager : MonoBehaviour
{
    public GameObject ResourcesParent;


    private Dictionary<string, GameObject> _ResourceTypeParents;
    private Dictionary<ResourceTypes, int> _ResourceStockpiles;


    public Dictionary<ResourceTypes, int> Stockpiles
    {
        get { return _ResourceStockpiles; }
    }



    void Awake()
    {
        _ResourceTypeParents = new Dictionary<string, GameObject>();

        InitResourceTypeParentObjects();
        InitResourceStockpiles();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Utils.DestroyAllChildGameObjects(ResourcesParent);
    }



    private void InitResourceTypeParentObjects()
    {
        Utils.DestroyAllChildGameObjects(ResourcesParent);


        List<GameObject> resourceNodes;
        List<string> resourceTypes;

        resourceNodes = new List<GameObject>();
        resourceTypes = new List<string>();


        object[] objects = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (object obj in objects)
        {
            GameObject gObj = (GameObject)obj;

            if (gObj.layer == LayerMask.NameToLayer("Resources"))
            {
                resourceNodes.Add(gObj);
                if (!resourceTypes.Contains(gObj.tag))
                    resourceTypes.Add(gObj.tag);
            }

        } // end foreach obj


        // Create the parent object for each type of resource node.
        resourceTypes.Sort();
        foreach (string category in resourceTypes)
        {
            GameObject categoryParent = new GameObject(category);
            categoryParent.transform.parent = ResourcesParent.transform;

            _ResourceTypeParents.Add(category, categoryParent);
        }


        // Parent each resource node to the proper parent.
        foreach (GameObject resourceNode in resourceNodes)
        {
            resourceNode.transform.parent = _ResourceTypeParents[resourceNode.tag].transform;
        }
    }

    private void InitResourceStockpiles()
    {
        _ResourceStockpiles = new Dictionary<ResourceTypes, int>();


        foreach (int i in Enum.GetValues(typeof(ResourceTypes)))
            _ResourceStockpiles.Add((ResourceTypes) i, 0);

    }


}
