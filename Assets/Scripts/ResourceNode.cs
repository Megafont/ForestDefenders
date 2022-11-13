using Cinemachine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;



public class ResourceNode : MonoBehaviour
{
    protected ResourceManager _ResourceManager;


    [Header("Node Settings")]

    [Tooltip("The type of resource provided by this node.")]
    public ResourceTypes ResourceType;

    [Tooltip("The maximum amount of resources that can be gathered from this node.")]
    public int TotalAmountInNode = 100;


    [Header("Gathering Settings")]

    [Tooltip("The base amount of this resource obtained per gather.")]
    public int AmountGainedPerGather = 4;

    [Tooltip("The amount of random variance that the amount obtained per gather can vary by (the amount received per gather is AmountPerGather + or - GatherVariance).")]
    public int GatherAmountVariance = 2;



    private int _AmountAvailable;

    private List<IVillager> _VillagersMiningThisNode;



    public delegate void ResourceNodeEventHandler(ResourceNode sender);

    public event ResourceNodeEventHandler OnNodeDepleted;




    private void Awake()
    {
        _VillagersMiningThisNode = new List<IVillager>();

        AmountAvailable = TotalAmountInNode;
    }

    // Start is called before the first frame update
    void Start()
    {
        _ResourceManager = GameManager.Instance.ResourceManager;
    }



    public int Gather()
    {
        if (IsDepleted)
            return 0;


        int gatherAmount = CalculateGatherAmount();
        
        AmountAvailable -= gatherAmount;
        _ResourceManager.Stockpiles[ResourceType] += gatherAmount;


        if (IsDepleted)
            OnNodeDepleted?.Invoke(this);


        return gatherAmount;
    }

    /// <summary>
    /// Sets the node's current resource amount back to the maximum.
    /// </summary>
    public void RestoreNode()
    {
        _AmountAvailable = TotalAmountInNode;
    }

    public void AddVillagerToMiningList(IVillager villager)
    {
        _VillagersMiningThisNode.Add(villager);
    }

    public void RemoveVillagerFromMiningList(IVillager villager)
    {
        _VillagersMiningThisNode.Remove(villager);
    }



    protected int CalculateGatherAmount()
    {
        int gatherAmount = AmountGainedPerGather + Random.Range(-GatherAmountVariance, GatherAmountVariance);

        return gatherAmount <= AmountAvailable ? gatherAmount: AmountAvailable;
    }



    /// <summary>
    /// The amount of resource still available in this node.
    /// </summary>
    public int AmountAvailable
    {
        get
        {
            return _AmountAvailable;
        }
        private set
        {
            _AmountAvailable = value >= 0 ? value : 0;
        }
    }

    public bool IsDepleted { get { return _AmountAvailable == 0; } }

    public int VillagersMiningThisNode { get { return _VillagersMiningThisNode.Count; } }

}
