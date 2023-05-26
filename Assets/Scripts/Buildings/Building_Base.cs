using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// This is the base class for all buildings.
/// </summary>
[RequireComponent(typeof(Health))]
public abstract class Building_Base : MonoBehaviour, IBuilding
{
    protected MeshCollider _Collider;
    protected Health _Health;
    protected NavMeshObstacle _NavMeshObstacle;

    protected BuildingDefinition _BuildingDefinition;
    protected VillageManager_Buildings _VillageManager_Buildings;
    protected VillageManager_Villagers _VillageManager_Villagers;

    protected AudioSource _AudioSource;

    protected GameManager _GameManager;
    protected BuildModeManager _BuildModeManager;

    protected bool _IsDeconstructing = false;

    public AnimationCurve _Curve;


    void Awake()
    {
        _GameManager = GameManager.Instance;
        _BuildModeManager = _GameManager.BuildModeManager;

        _AudioSource = gameObject.AddComponent<AudioSource>();

        _VillageManager_Buildings = GameManager.Instance.VillageManager_Buildings;
        _VillageManager_Villagers = GameManager.Instance.VillageManager_Villagers;


        _Collider = GetComponent<MeshCollider>();
        _Health = GetComponent<Health>();
        _NavMeshObstacle = GetComponent<NavMeshObstacle>();

        
        _Health.OnDeath += OnDeath;
        _Health.OnTakeDamage += OnTakeDamage;


        InitBuilding();
        ConfigureBuildingComponents();
    }

    void Start()
    {

    }

    void FixedUpdate()
    {
        UpdateBuilding();
    }



    public BuildingDefinition GetBuildingDefinition()
    {
        return _BuildingDefinition;
    }

    public virtual Mesh GetMesh()
    {
        return GetComponent<MeshFilter>().sharedMesh;
    }


    protected void ConfigureBasicBuildingSetup(string buildingCategory, string buildingName)
    {
        Category = buildingCategory;
        Name = buildingName;

        _BuildingDefinition = BuildModeDefinitions.GetBuildingDefinition(Category, Name);
    }

    protected virtual void ConfigureBuildingComponents()
    {
        _Health.SetMaxHealth(_BuildingDefinition.MaxHealth);
        _Health.ResetHealthToMax();
    }

    protected virtual void InitBuilding()
    {
        ConfigureBasicBuildingSetup("None", "None");
    }


    protected virtual void UpdateBuilding()
    {
        if (_Health.CurrentHealth <= 0)
            return;
    }

    public void Deconstruct(GameObject sender)
    {
        if (_IsDeconstructing)
            return;


        _IsDeconstructing = true;

        OnDeath(sender, null);
    }

    protected virtual void OnDeath(GameObject sender, GameObject attacker)
    {
        _AudioSource.clip = _BuildModeManager.BuildingDestructionSound;
        _AudioSource.volume = _BuildModeManager.BuildingDestructionSoundVolume;
        _AudioSource.Play();

        StartCoroutine(FadeOutAfterDeath());
    }

    protected virtual void OnTakeDamage(GameObject sender, GameObject attacker, float amount, DamageTypes damageType)
    {
        _VillageManager_Villagers.RequestBackup(gameObject, attacker);
    }


    WaitForSeconds _FadeOutDelay = new WaitForSeconds(3.0f);
    protected virtual IEnumerator FadeOutAfterDeath()
    {
        // Start the shrink animation and wait for it to complete.
        yield return StartCoroutine(Utils_Misc.ShrinkObjectToNothing(transform, 0.4f));

        // This extra delay is just to allow the building destruction sound enough time to finish playing.
        yield return new WaitForSeconds(1.5f);

        Destroy(gameObject);
    }



    /// <summary>
    /// This method is called on instances of this class that are on prefabs that have been loaded but NOT
    /// instantiated. It performs a partial initialization, so this class will still know certain things
    /// about the building it represents (like type and category). This is necessary, because the Awake()
    /// method is not called on a prefab that has been loaded but NOT instantiated.
    /// </summary>
    public void InitAsPrefab()
    {
        InitBuilding();
    }



    public AudioSource AudioSource { get { return _AudioSource; } }

    public string Category { get; protected set; }
    public string Name { get; protected set; }

    public Health HealthComponent { get { return _Health; } }

    public bool IsDeconstructing { get { return _IsDeconstructing; } }

}
