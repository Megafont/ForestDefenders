using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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


    public string BuildingCategory { get; protected set; }
    public string BuildingName { get; protected set; }
    
    public Health HealthComponent { get { return _Health; } }


    protected BuildingDefinition _BuildingDefinition;
    protected VillageManager _VillageManager;



    void Awake()
    {
        _VillageManager = GameManager.Instance.VillageManager;


        _Collider = GetComponent<MeshCollider>();
        _Health = GetComponent<Health>();
        _NavMeshObstacle = GetComponent<NavMeshObstacle>();

        
        _Health.OnDeath += OnDeath;
        _Health.OnTakeDamage += OnTakeDamage;


        InitBuilding();
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
        BuildingCategory = buildingCategory;
        BuildingName = buildingName;

        _BuildingDefinition = BuildModeDefinitions.GetBuildingDefinition(BuildingCategory, BuildingName);
        _Health.MaxHealth = _BuildingDefinition.MaxHealth;
        _Health.Heal(_BuildingDefinition.MaxHealth, gameObject);
    }

    protected virtual void InitBuilding()
    {
        // This class is the only one that sets these fields directly. It does not call the InitBuilding() method like all subclasses, since
        // there is no building with category and name both equal to "None". So calling that method would crash the game.
        BuildingCategory = "None";
        BuildingName = "None";
    }


    protected virtual void UpdateBuilding()
    {
        if (_Health.CurrentHealth <= 0)
            return;
    }

    protected virtual void OnDeath(GameObject sender)
    {
        StartCoroutine(FadeOutAfterDeath());
    }

    protected virtual void OnTakeDamage(GameObject sender, GameObject attacker, float amount)
    {
        _VillageManager.RequestBackup(gameObject, attacker);
    }


    WaitForSeconds _FadeOutDelay = new WaitForSeconds(2.0f);
    protected virtual IEnumerator FadeOutAfterDeath()
    {       
        //yield return _FadeOutDelay;

        Destroy(gameObject);

        yield break;
    }

}
