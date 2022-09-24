using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class ResourceManager : MonoBehaviour
{
    public GameObject ResourcesParent;


    private Dictionary<string, GameObject> _ResourceTypeParents;
    private Dictionary<ResourceTypes, int> _ResourceStockpilesByType;
    private Dictionary<ResourceTypes, List<ResourceNode>> _ResourceNodesByType;
    private List<ResourceNode> _AllResourceNodes;



    public Dictionary<ResourceTypes, int> Stockpiles
    {
        get { return _ResourceStockpilesByType; }
    }



    void Awake()
    {
        _ResourceTypeParents = new Dictionary<string, GameObject>();

        InitResourceTypeParentObjects();
        InitResourceStockpiles();
        InitResourceNodeLists();
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
        _ResourceStockpilesByType = new Dictionary<ResourceTypes, int>();


        foreach (int i in Enum.GetValues(typeof(ResourceTypes)))
            _ResourceStockpilesByType.Add((ResourceTypes) i, 0);

    }

    private void InitResourceNodeLists()
    {
        _ResourceNodesByType = new Dictionary<ResourceTypes, List<ResourceNode>>();
        _AllResourceNodes = new List<ResourceNode>();

        _ResourceNodesByType.Add(ResourceTypes.Wood, new List<ResourceNode>());
        _ResourceNodesByType.Add(ResourceTypes.Stone, new List<ResourceNode>());

        DetectResourceNodes();
    }

    private void DetectResourceNodes()
    {
        ResourceNode[] resourceNodes = FindObjectsOfType<ResourceNode>();


        foreach (ResourceNode node in resourceNodes)
        {
            _ResourceNodesByType[node.ResourceType].Add(node);
            _AllResourceNodes.Add(node);
        }
    }

    public ResourceTypes GetLowestResourceStockpileType()
    {
        ResourceTypes lowestResourceType = ResourceTypes.Wood;
        int minAmount = int.MaxValue;


        foreach (KeyValuePair<ResourceTypes, int> pair in _ResourceStockpilesByType)
        {
            if (pair.Value < minAmount)
            {
                lowestResourceType = pair.Key;
                minAmount = pair.Value;
            }

        } // end foreach pair


        return lowestResourceType;
    }

    public ResourceNode FindNearestResourceNodeOfType(Vector3 callerPosition, ResourceTypes type)
    {
        ResourceNode closestNode = null;
        float minDistance = float.MaxValue;


        foreach (ResourceNode node in _ResourceNodesByType[type])
        {
            float distance = Vector3.Distance(callerPosition, node.transform.position);
            if (node.AmountAvailable > 0 && distance < minDistance)
            {
                closestNode = node;
                minDistance = distance;
            }

        } // end foreach node


        return closestNode;
    }

    public ResourceNode FindNearestResourceNode(Vector3 callerPosition)
    {
        ResourceNode closestNode = null;
        float minDistance = float.MaxValue;


        foreach (ResourceNode node in _AllResourceNodes)
        {
            float distance = Vector3.Distance(callerPosition, node.transform.position);
            if (node.AmountAvailable > 0 && distance < minDistance)
            {
                closestNode = node;
                minDistance = distance;
            }

        } // end foreach node


        return closestNode;
    }


}
