using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;


[RequireComponent(typeof(SoundSetPlayer))]
public class ResourceNode : MonoBehaviour
{
    [Header("Node Settings")]

    [Tooltip("The type of resource provided by this node.")]
    [SerializeField] private ResourceTypes _ResourceType;

    [Tooltip("The maximum amount of resources that this node may potentially contain.")]
    [SerializeField] private float _MaxAmountInNode = 50;



    private GameManager _GameManager;
    private ResourceManager _ResourceManager;
    private SoundSetPlayer _SoundSetPlayer;

    private LevelUpDialog _LevelUpDialog;

    private float _AmountAvailable;
    
    private List<IVillager> _VillagersMiningThisNode;



    public ResourceTypes ResourceType { get { return _ResourceType; } }



    public delegate void ResourceNodeEventHandler(ResourceNode sender);

    public event ResourceNodeEventHandler OnNodeDepleted;




    void Awake()
    {
        _GameManager = GameManager.Instance;
        _ResourceManager = _GameManager.ResourceManager;

        _SoundSetPlayer = GetComponent<SoundSetPlayer>();
        GetSoundSet();

        _LevelUpDialog = _GameManager.LevelUpDialog;

        _VillagersMiningThisNode = new List<IVillager>();
    }

    private void Start()
    {

    }

    public float Gather(GameObject gatherer)
    {
        if (IsDepleted)
            return 0;

        float amountBeforeGather = AmountAvailable;

        float gatherAmount = CalculateGatherAmount(gatherer);

        AmountAvailable -= gatherAmount;
        _ResourceManager.Stockpiles[_ResourceType] += gatherAmount;


        if (gatherer.CompareTag("Player"))
            _GameManager.AddToScore((int) gatherAmount * _GameManager.PlayerGatheringScoreMultiplier);


        _SoundSetPlayer.PlaySound();

        // Only fire the NodeDepleted event if the node is depleted, and it wasn't at the start of this function. Then we know this particular gather is the one that depleted it.
        // This way, we only fire the event once.
        if (IsDepleted && amountBeforeGather > 0)
        {
            _GameManager.AddToScore((int) _MaxAmountInNode * _GameManager.PlayerGatheringScoreMultiplier);
            OnNodeDepleted?.Invoke(this);
        }


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
        float randomVariance = Random.Range(0.0f, _ResourceManager.ResourceNodeAmountVariance);

        _AmountAvailable = Mathf.CeilToInt(_MaxAmountInNode - (_MaxAmountInNode * randomVariance));
    }

    public void AddVillagerToMiningList(IVillager villager)
    {
        _VillagersMiningThisNode.Add(villager);
    }

    public void RemoveVillagerFromMiningList(IVillager villager)
    {
        _VillagersMiningThisNode.Remove(villager);
    }

    public bool IsAccessible()
    {
        return false;
    }

    private void GetSoundSet()
    {
        switch (_ResourceType)
        {
            case ResourceTypes.Food:
                _SoundSetPlayer._SoundSet = GameManager.Instance.SoundParams.GetSoundSet("Sound Set - Gathering Food");
                break;

            case ResourceTypes.Stone:
                _SoundSetPlayer._SoundSet = GameManager.Instance.SoundParams.GetSoundSet("Sound Set - Mining Stone");
                break;

            case ResourceTypes.Wood:
                _SoundSetPlayer._SoundSet = GameManager.Instance.SoundParams.GetSoundSet("Sound Set - Chopping Wood");
                break;


            default:
                Debug.LogError("Could not find sound set for resource type \"ResourceType\"!");
                break;
        }
    }

    private float CalculateGatherAmount(GameObject gatherer)
    {
        float baseAmount = 0;
        if (gatherer.GetComponent<PlayerController>())
            baseAmount = _LevelUpDialog.CurrentPlayerGatherRate;
        else if (gatherer.GetComponent<Villager_Base>())
            baseAmount = _LevelUpDialog.CurrentVillagerGatherRate;
        else
            throw new Exception("Resource could not be gathered! Unknown gatherer type!");


        float randomVariance = Random.Range(0.0f, _ResourceManager.GatherAmountVariance);

        float gatherAmount = Mathf.CeilToInt(baseAmount - (baseAmount * randomVariance));

        return gatherAmount <= AmountAvailable ? gatherAmount : AmountAvailable;
    }



    /// <summary>
    /// The amount of resource still available in this node.
    /// </summary>
    public float AmountAvailable
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
