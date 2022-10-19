using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class ResourceManager : MonoBehaviour
{
    public GameObject ResourcesParent;


    private Dictionary<ResourceTypes, GameObject> _ResourceTypeParents;
    private Dictionary<ResourceTypes, int> _ResourceStockpilesByType;
    private Dictionary<ResourceTypes, List<ResourceNode>> _ResourceNodesByType;
    private List<ResourceNode> _AllResourceNodes;



    public Dictionary<ResourceTypes, int> Stockpiles
    {
        get { return _ResourceStockpilesByType; }
    }



    void Awake()
    {
        _ResourceTypeParents = new Dictionary<ResourceTypes, GameObject>();

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


        // Create the parent object for each type of resource node.
        foreach (int i in Enum.GetValues(typeof(ResourceTypes)))
        {
            GameObject categoryParent = new GameObject(Enum.GetName(typeof(ResourceTypes), i));
            categoryParent.transform.parent = ResourcesParent.transform;

            _ResourceTypeParents.Add((ResourceTypes) i, categoryParent);
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


        foreach (int i in Enum.GetValues(typeof(ResourceTypes)))
            _ResourceNodesByType.Add((ResourceTypes) i, new List<ResourceNode>());


        DetectResourceNodes();
    }

    private void DetectResourceNodes()
    {
        ResourceNode[] resourceNodes = FindObjectsOfType<ResourceNode>();


        foreach (ResourceNode node in resourceNodes)
        {
            _ResourceNodesByType[node.ResourceType].Add(node);
            _AllResourceNodes.Add(node);

            node.transform.parent = _ResourceTypeParents[node.ResourceType].transform;
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