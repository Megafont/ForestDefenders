using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;



[SelectionBase]
[RequireComponent(typeof(SoundSetPlayer))]
public class ResourceNode : MonoBehaviour
{
    [Header("Node Settings")]

    [Tooltip("The type of resource provided by this node.")]
    [SerializeField] private ResourceTypes _ResourceType;

    [Tooltip("The maximum amount of resources that this node may potentially contain.")]
    [SerializeField] private float _MaxAmountInNode = 50;

    [Tooltip("Whether or not this resource node should be destroyed upon depletion.")]
    [SerializeField] private bool _DestroyOnDepletion = false;

    [Tooltip("A prefab of the death particles system to be played when the resource node is destroyed. This prefab is only instantiated if DestroyOnDepletion is enabled.")]
    [SerializeField] private GameObject _DeathParticlesPrefab;

    [SerializeField] protected AudioClip _DestroySound;



    private GameManager _GameManager;
    private BuildModeManager _BuildModeManager;
    private ResourceManager _ResourceManager;
    private SoundSetPlayer _SoundSetPlayer;
    private VillageManager_Villagers _VillageManager_Villagers;

    private LevelUpDialog _LevelUpDialog;

    private float _AmountAvailable;

    private List<IVillager> _VillagersMiningThisNode;

    private FloatingStatusBar _FloatingStatBar;

    private GameObject _DeathParticles;
    


    public LevelAreas ParentArea { get; private set; } = LevelAreas.Unknown;
    public ResourceTypes ResourceType { get { return _ResourceType; } }



    public delegate void ResourceNodeEventHandler(ResourceNode sender);

    public event ResourceNodeEventHandler OnNodeDepleted;




    void Awake()
    {
        LevelAreas area = Utils_World.DetectAreaNumberFromPosition(transform.position);
        if (area != LevelAreas.Unknown)    
            ParentArea = area;

        if (_DestroyOnDepletion)
            _DeathParticles = Instantiate(_DeathParticlesPrefab, transform);


        _GameManager = GameManager.Instance;
        _BuildModeManager = _GameManager.BuildModeManager;
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
        _FloatingStatBar.Label = $"{_ResourceType}:";
        _FloatingStatBar.SetValue(AmountAvailable);

        if (AmountAvailable < 1)
            _FloatingStatBar.gameObject.SetActive(false); // Make the floating status bar disappear.
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



        // Gather the resource and add it to the appropriate stockpile.
        AmountAvailable -= gatherAmount;
        if (gatherAmount > 0)
            _ResourceManager.AddToStockpile(_ResourceType, gatherAmount);


        // If the gatherer is the player, add some points to their score.
        if (gatherer.CompareTag("Player"))
            _GameManager.AddToScore((int) gatherAmount * _GameManager.PlayerGatheringScoreMultiplier);


        _SoundSetPlayer.PlayRandomSound();


        // Only fire the NodeDepleted event if the node is depleted, and it wasn't already empty at the start of this function. Then we know this particular gather is the one that depleted it.
        // This ensures we only fire the event once.
        if (IsDepleted && amountBeforeGather > 0)
        {
            _FloatingStatBar.gameObject.SetActive(false); // Make the floating status bar disappear.
            OnNodeDepleted?.Invoke(this);

            if (_DestroyOnDepletion)
                StartCoroutine(DestroyOnDepletion());
        }


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
        // Just abort this function if the resource node has been destroyed.
        if (IsDestroyed)
            return;


        float randomVariance = Random.Range(0.0f, _ResourceManager.ResourceNodeAmountVariance);

        _AmountAvailable = Mathf.CeilToInt(_MaxAmountInNode - (_MaxAmountInNode * randomVariance));

        _FloatingStatBar.MaxValue = _AmountAvailable;
        _FloatingStatBar.SetValue(_AmountAvailable);

        if (!_BuildModeManager.IsBuildModeActive && _GameManager.GameState != GameStates.GameOver)
            _FloatingStatBar.gameObject.SetActive(true); // Make the floating status bar visible.
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


    WaitForSeconds _DeathDelay = new WaitForSeconds(0.4f);
    private IEnumerator DestroyOnDepletion()
    {
        IsDestroyed = true;
        _ResourceManager.RemoveResourceNode(this);

        _FloatingStatBar.gameObject.SetActive(false);

        PlayDestroySFX();

        _DeathParticles.GetComponent<ParticleSystem>().Play();

        // Start the shrink animation and wait for it to complete.
        yield return StartCoroutine(Utils_Misc.ShrinkObjectToNothing(transform, 0.4f));



        yield return _DeathDelay;

        Destroy(gameObject);
    }

    private void PlayDestroySFX()
    {
        if (_DestroySound)
            AudioSource.PlayClipAtPoint(_DestroySound, transform.position, 1.0f);
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

    public bool IsDestroyed { get; private set; } = false;
    public int VillagersMiningThisNode { get { return _VillagersMiningThisNode.Count; } }

}
