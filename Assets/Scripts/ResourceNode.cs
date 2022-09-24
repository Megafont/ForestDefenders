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
    public int TotalAmountInNode = 50;


    [Header("Gathering Settings")]

    [Tooltip("The base amount of this resource obtained per gather.")]
    public int AmountGainedPerGather = 3;

    [Tooltip("The amount of random variance that the amount obtained per gather can vary by (the amount received per gather is AmountPerGather + or - GatherVariance).")]
    public int GatherAmountVariance = 2;


    public IVillager VillagerWorkingThisNode;


    private int _AmountAvailable;


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



    // Start is called before the first frame update
    void Start()
    {
        _ResourceManager = GameManager.Instance.ResourceManager;

        AmountAvailable = TotalAmountInNode;
    }



    public int Gather()
    {
        int gatherAmount = CalculateGatherAmount();
        
        AmountAvailable -= gatherAmount;
        _ResourceManager.Stockpiles[ResourceType] += gatherAmount;

        return gatherAmount;
    }


    protected int CalculateGatherAmount()
    {
        int gatherAmount = AmountGainedPerGather + Random.Range(-GatherAmountVariance, GatherAmountVariance);

        return gatherAmount <= AmountAvailable ? gatherAmount: AmountAvailable;
    }

}
