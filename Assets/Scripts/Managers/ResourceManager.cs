using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;


public class ResourceManager : MonoBehaviour
{
    public GameObject ResourcesParent;


    [Header("Stockpiles Settings")]
    public uint ResourceStockpilesStartAmount = 0;

    public uint ResourceStockpilesLowThreshold = 250;
    public uint ResourceStockpilesOkThreshold = 500;
    public uint ResourceStockpilesPlentifulThreshold = 1000;


    [Header("Gathering Settings")]

    [Tooltip("The max percentage that the amount of resources in a node can vary by. If this percentage is 0, then the amount of resources in a node will always be equal to node.MaxAmountInNode.")]
    [Range(0f, 1f)]
    public float ResourceNodeAmountVariance = 0.20f;
    [Tooltip("The max percentage that the amount of resources obtained per gather can vary by. If this percentage is 0, then the amount obtained per gather is always equal to the character's gathering rate stat.")]
    [Range(0f, 1f)]
    public float GatherAmountVariance = 0.15f;



    private Dictionary<ResourceTypes, GameObject> _ResourceTypeParents;
    private Dictionary<ResourceTypes, float> _ResourceStockpilesByType;
    private Dictionary<ResourceTypes, List<ResourceNode>> _ResourceNodesByType;

    private List<ResourceNode> _AllResourceNodes;

    private List<ResourceNode> _ActiveResourceNodes;
    private List<ResourceNode> _DepletedResourceNodes;



    void Awake()
    {
        _ResourceTypeParents = new Dictionary<ResourceTypes, GameObject>();

        _ResourceNodesByType = new Dictionary<ResourceTypes, List<ResourceNode>>();

        _AllResourceNodes = new List<ResourceNode>();
        _ActiveResourceNodes = new List<ResourceNode>();
        _DepletedResourceNodes = new List<ResourceNode>();

        InitResourceTypeParentObjects();
        InitResourceStockpiles();
        InitResourceNodeLists();
    }

    private void Start()
    {
        RestoreResourceNodes();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        Utils_Misc.DestroyAllChildGameObjects(ResourcesParent);
    }


    /// <summary>
    /// Adds the specified amount of resources to the specified stockpile.
    /// </summary>
    /// <param name="stockpile">The stockpile to add resources to.</param>
    /// <param name="amount">The amount of resources being added.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">When amount is 0 or negative.</exception>
    public void AddToStockpile(ResourceTypes stockpile, float amount)
    {
        if (amount < 1)
            throw new Exception($"The amount of resources added to the \"{stockpile}\" stockpile must be positive!");

        _ResourceStockpilesByType[stockpile] += amount;
    }

    /// <summary>
    /// This function will attempt to expend the requested amount of resources from the specified stockpile.
    /// </summary>
    /// <param name="stockpile">The stockpile to request resources from.</param>
    /// <param name="amount">The amount of resources being requested.</param>
    /// <returns>True if the resources were expended, or false if there wasn't enough.</returns>
    /// <exception cref="ArgumentException">When amount is 0 or negative.</exception>
    public bool ExpendFromStockpile(ResourceTypes stockpile, float amount)
    {
        if (amount < 1)
            throw new ArgumentException($"The amount of resources requested from the \"{stockpile}\" stockpile must be positive!");


        if (_ResourceStockpilesByType[stockpile] >= amount)
        {
            _ResourceStockpilesByType[stockpile] -= amount;
            return true;
        }


        return false;
    }

    public bool IsStockpileLevelLow(ResourceTypes stockpile)
    {
        return _ResourceStockpilesByType[stockpile] <= ResourceStockpilesLowThreshold;
    }

    public bool IsStockpileLevelOK(ResourceTypes stockpile)
    {
        return _ResourceStockpilesByType[stockpile] >= ResourceStockpilesOkThreshold;
    }

    public bool IsStockpileLevelPlentiful(ResourceTypes stockpile)
    {
        return _ResourceStockpilesByType[stockpile] >= ResourceStockpilesPlentifulThreshold;
    }

    public float GetStockpileLevel(ResourceTypes stockpile)
    {
        return _ResourceStockpilesByType[stockpile];
    }

    public void RestoreResourceNodes()
    {
        if (_AllResourceNodes == null)
            return;


        foreach (ResourceNode node in _AllResourceNodes)
            node.RestoreNode();


        _ActiveResourceNodes.Clear();
        _ActiveResourceNodes.AddRange(_AllResourceNodes);


        // ONLY ADD ONES BACK IN THE LIST THAT ARE ACCESSIBLE!

        _DepletedResourceNodes.Clear();
    }

    public ResourceTypes GetLowestResourceStockpileType()
    {
        ResourceTypes lowestResourceType = ResourceTypes.Wood;
        float minAmount = float.MaxValue;


        foreach (KeyValuePair<ResourceTypes, float> pair in _ResourceStockpilesByType)
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
        if (!_AllResourceNodes.Contains(newNode))
            _AllResourceNodes.Add(newNode);


        if (!newNode.IsDepleted && !_ActiveResourceNodes.Contains(newNode))
            _ActiveResourceNodes.Add(newNode);
        else if (!_DepletedResourceNodes.Contains(newNode))
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
        Utils_Misc.DestroyAllChildGameObjects(ResourcesParent);


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
        _ResourceStockpilesByType = new Dictionary<ResourceTypes, float>();


        foreach (int i in Enum.GetValues(typeof(ResourceTypes)))
            _ResourceStockpilesByType.Add((ResourceTypes) i, ResourceStockpilesStartAmount);

    }

    private void InitResourceNodeLists()
    {     
        foreach (int i in Enum.GetValues(typeof(ResourceTypes)))
            _ResourceNodesByType.Add((ResourceTypes) i, new List<ResourceNode>());


        DetectResourceNodes();
    }

    private void DetectResourceNodes()
    {
        ResourceNode[] resourceNodes = FindObjectsOfType<ResourceNode>();


        foreach (ResourceNode node in resourceNodes)
        {
            // If this resource node is a building, skip it.
            if (node.TryGetComponent(out IBuilding building /*&& building.gameObject.tag == "Building Prefab"*/))
                continue;


            _ResourceNodesByType[node.ResourceType].Add(node);
            
            if (!_AllResourceNodes.Contains(node))
                _AllResourceNodes.Add(node);

            if (!node.IsDepleted && !_ActiveResourceNodes.Contains(node))
                _ActiveResourceNodes.Add(node);
            else if (!_DepletedResourceNodes.Contains(node))
                _DepletedResourceNodes.Add(node);


            //node.transform.parent = _ResourceTypeParents[node.ResourceType].transform;
            node.OnNodeDepleted += OnResourceNodeDepleted;

        } // end foreach node


        //Debug.Log($"Active Resource Nodes: {_ActiveResourceNodes.Count}    Depleted Resource Nodes: {_DepletedResourceNodes.Count}");
    }



    private void OnResourceNodeDepleted(ResourceNode sender)
    {
        _ActiveResourceNodes.Remove(sender);
        _DepletedResourceNodes.Add(sender);
    }



    public ResourceNode GetResourceNode(int index)
    {
        return _AllResourceNodes[index];
    }

    public ResourceNode GetActiveResourceNode(int index)
    {
        return _ActiveResourceNodes[index];
    }

    public ResourceNode GetDepletedResourceNode(int index)
    {
        return _DepletedResourceNodes[index];
    }


    public int AllResourceNodesCount { get { return _AllResourceNodes.Count; } }
    public int ActiveResourceNodesCount { get { return _ActiveResourceNodes.Count; } }
    public int DepletedResourceNodesCount { get { return _DepletedResourceNodes.Count; } }

}
