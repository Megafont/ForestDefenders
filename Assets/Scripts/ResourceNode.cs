using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(SoundSetPlayer))]
public class ResourceNode : MonoBehaviour
{  
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



    private ResourceManager _ResourceManager;
    private SoundSetPlayer _SoundSetPlayer;
    private SoundSet _SoundSet;

    private int _AmountAvailable;

    private List<IVillager> _VillagersMiningThisNode;



    public delegate void ResourceNodeEventHandler(ResourceNode sender);

    public event ResourceNodeEventHandler OnNodeDepleted;




    private void Awake()
    {
        _ResourceManager = GameManager.Instance.ResourceManager;

        _SoundSetPlayer = GetComponent<SoundSetPlayer>();
        GetSoundSet();

        _VillagersMiningThisNode = new List<IVillager>();

        AmountAvailable = TotalAmountInNode;
    }

    // Start is called before the first frame update
    void Start()
    {
    }



    public int Gather()
    {
        if (IsDepleted)
            return 0;


        int gatherAmount = CalculateGatherAmount();
        
        AmountAvailable -= gatherAmount;
        _ResourceManager.Stockpiles[ResourceType] += gatherAmount;

        _SoundSetPlayer.PlaySound();

        if (IsDepleted)
            OnNodeDepleted?.Invoke(this);


        return gatherAmount;
    }

    /// <summary>
    /// Sets the node's current resource amount to 0.
    /// </summary>
    public void ClearNode()
    {
        _AmountAvailable = 0;
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

    private void GetSoundSet()
    {
        switch (ResourceType)
        {
            case ResourceTypes.Food:
                _SoundSetPlayer.SoundSet = GameManager.Instance.SoundParams.GetSoundSet("Sound Set - Gathering Food");
                break;

            case ResourceTypes.Stone:
                _SoundSetPlayer.SoundSet = GameManager.Instance.SoundParams.GetSoundSet("Sound Set - Mining Stone");
                break;

            case ResourceTypes.Wood:
                _SoundSetPlayer.SoundSet = GameManager.Instance.SoundParams.GetSoundSet("Sound Set - Chopping Wood");
                break;


            default:
                Debug.LogError("Could not find sound set for resource type \"ResourceType\"!");
                break;
        }
    }

    private int CalculateGatherAmount()
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
