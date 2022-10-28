using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

using Random = UnityEngine.Random;


public class ResourceManager : MonoBehaviour
{
    public GameObject ResourcesParent;


    private Dictionary<ResourceTypes, GameObject> _ResourceTypeParents;
    private Dictionary<ResourceTypes, int> _ResourceStockpilesByType;
    private Dictionary<ResourceTypes, List<ResourceNode>> _ResourceNodesByType;


    private List<ResourceNode> _AllResourceNodes;

    private List<ResourceNode> _ActiveResourceNodes;
    private List<ResourceNode> _DepletedResourceNodes;

    

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

    void OnDestroy()
    {
        Utils.DestroyAllChildGameObjects(ResourcesParent);
    }



    public void RestoreResourceNodes()
    {
        if (_AllResourceNodes == null)
            return;


        foreach (ResourceNode node in _AllResourceNodes)
            node.RestoreNode();


        _ActiveResourceNodes.Clear();
        _ActiveResourceNodes.AddRange(_AllResourceNodes);

        _DepletedResourceNodes.Clear();
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

    public ResourceNode FindNearestResourceNode(Vector3 callerPosition, ResourceTypes? type = null)
    {
        ResourceNode closestNode = null;
        float minDistance = float.MaxValue;


        foreach (ResourceNode node in _AllResourceNodes)
        {
            // A node can be null if it was destroyed and removed from the resource manager.
            if (node == null)
                continue;

            float distance = Vector3.Distance(callerPosition, node.transform.position);
            if (node.AmountAvailable > 0 && distance < minDistance)
            {
                // If no resource type was specified, or if the resource node has the same type as that specified,
                // then set it as the new closest node.
                if (type == null || type == node.ResourceType)
                {
                    closestNode = node;
                    minDistance = distance;
                }
            }

        } // end foreach node


        return closestNode;
    }

    /// <summary>
    /// Gets a random resource node that is not depleted.
    /// </summary>
    public ResourceNode GetRandomActiveResourceNode()
    {
        if (_ActiveResourceNodes.Count == 0)
            return null;


        int index = Random.Range(0, _ActiveResourceNodes.Count);
        return _ActiveResourceNodes[index];        
    }

    public void AddResourceNode(ResourceNode newNode)
    {
        _AllResourceNodes.Add(newNode);

        if (!newNode.IsDepleted)
            _ActiveResourceNodes.Add(newNode);
        else
            _DepletedResourceNodes.Add(newNode);

        _ResourceNodesByType[newNode.ResourceType].Add(newNode);

        newNode.OnNodeDepleted += OnResourceNodeDepleted;
    }

    public void RemoveResourceNode(ResourceNode node)
    {
        _AllResourceNodes.Remove(node);

        _ActiveResourceNodes.Remove(node);
        _DepletedResourceNodes.Remove(node);

        _ResourceNodesByType[node.ResourceType].Remove(node);

        node.OnNodeDepleted -= OnResourceNodeDepleted;
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
        _ActiveResourceNodes = new List<ResourceNode>();
        _DepletedResourceNodes = new List<ResourceNode>();


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

            if (!node.IsDepleted)
                _ActiveResourceNodes.Add(node);
            else
                _DepletedResourceNodes.Add(node);

            node.transform.parent = _ResourceTypeParents[node.ResourceType].transform;
            node.OnNodeDepleted += OnResourceNodeDepleted;


        }


        //Debug.Log($"Active Resource Nodes: {_ActiveResourceNodes.Count}    Depleted Resource Nodes: {_DepletedResourceNodes.Count}");
    }

    private void OnResourceNodeDepleted(ResourceNode sender)
    {
        _ActiveResourceNodes.Remove(sender);
        _DepletedResourceNodes.Add(sender);
    }

}
