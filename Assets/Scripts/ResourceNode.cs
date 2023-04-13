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
    private VillageManager_Villagers _VillageManager_Villagers;

    private LevelUpDialog _LevelUpDialog;

    private float _AmountAvailable;

    private List<IVillager> _VillagersMiningThisNode;

    private FloatingStatusBar _FloatingStatBar;


    public LevelAreas ParentArea { get; private set; } = LevelAreas.Unknown;
    public ResourceTypes ResourceType { get { return _ResourceType; } }



    public delegate void ResourceNodeEventHandler(ResourceNode sender);

    public event ResourceNodeEventHandler OnNodeDepleted;




    void Awake()
    {
        if (Utils_World.DetectAreaNumberFromGroundPosition(transform.position.x, transform.position.z, LayerMask.GetMask(new string[] { "Ground" }), out LevelAreas area))
            ParentArea = area;
        

        _GameManager = GameManager.Instance;
        _ResourceManager = _GameManager.ResourceManager;

        InitFloatingStatusBar();

        _SoundSetPlayer = GetComponent<SoundSetPlayer>();
        GetSoundSet();

        _VillageManager_Villagers = _GameManager.VillageManager_Villagers;

        _LevelUpDialog = _GameManager.LevelUpDialog;

        _VillagersMiningThisNode = new List<IVillager>();
    }

    private void Start()
    {

    }

    private void InitFloatingStatusBar()
    {
        GameObject bar = Instantiate(_GameManager.GetFloatingStatusBarPrefab(),
                                     transform.position,
                                     Quaternion.identity,
                                     this.transform);

        _FloatingStatBar = bar.GetComponent<FloatingStatusBar>();
        _FloatingStatBar.MaxValue = AmountAvailable;
        _FloatingStatBar.Label = $"{_ResourceType.ToString()}:";
        _FloatingStatBar.SetValue(AmountAvailable);
    }

    public float Gather(GameObject gatherer)
    {
        float amountBeforeGather = AmountAvailable;

        float gatherAmount = CalculateGatherAmount(gatherer);


        // Check if this gatherer is a villager.
        IVillager villager = gatherer.GetComponent<IVillager>();


        // Simply return 0 if this node is now depleted.
        if (IsDepleted) 
            return 0;


        // If the gatherer is a villager, expend food used to do the work.
        // NOTE: We don't do this if it is the player to ensure that they can still gather even if food is in short supply.
        if (gatherer.CompareTag("Villager"))
        {
            int foodAmount = Mathf.RoundToInt(_VillageManager_Villagers.GatheringCostMultiplier * gatherAmount);

            // If there is not enough food available, then tell the villager to stop targeting this resource node.
            if (foodAmount > _ResourceManager.Stockpiles[ResourceTypes.Food])
            {
                if (gatherer.CompareTag("Villager"))
                    villager.SetTarget(null, true);

                return 0;
            }


            _ResourceManager.Stockpiles[ResourceTypes.Food] -= foodAmount;
        }


        // Gather the resource and add it to the appropriate stockpile.
        AmountAvailable -= gatherAmount;
        _ResourceManager.Stockpiles[_ResourceType] += gatherAmount;


        // If the gatherer is the player, add some points to their score.
        if (gatherer.CompareTag("Player"))
            _GameManager.AddToScore((int) gatherAmount * _GameManager.PlayerGatheringScoreMultiplier);


        _SoundSetPlayer.PlaySound();


        // Only fire the NodeDepleted event if the node is depleted, and it wasn't already empty at the start of this function. Then we know this particular gather is the one that depleted it.
        // This ensures we only fire the event once.
        if (IsDepleted && amountBeforeGather > 0)
            OnNodeDepleted?.Invoke(this);


        return gatherAmount;
    }

    /// <summary>
    /// Sets the node's current resource amount to 0.
    /// </summary>
    public void ClearNode()
    {
        AmountAvailable = 0;
    }

    /// <summary>
    /// Sets the node's current resource amount back to the maximum.
    /// </summary>
    public void RestoreNode()
    {
        float randomVariance = Random.Range(0.0f, _ResourceManager.ResourceNodeAmountVariance);

        _AmountAvailable = Mathf.CeilToInt(_MaxAmountInNode - (_MaxAmountInNode * randomVariance));

        _FloatingStatBar.MaxValue = _AmountAvailable;
        _FloatingStatBar.SetValue(_AmountAvailable);
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
            _FloatingStatBar.SetValue(_AmountAvailable);
        }
    }

    public bool IsDepleted { get { return _AmountAvailable == 0; } }

    public int VillagersMiningThisNode { get { return _VillagersMiningThisNode.Count; } }

}
