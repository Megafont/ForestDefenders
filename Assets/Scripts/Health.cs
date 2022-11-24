using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


public class Health : MonoBehaviour
{
    public float MaxHealth = 100;

    public bool EnableDamageFlash = true;
    public Color DamageFlashColor = Color.red;
    public float DamageFlashTime = 0.1f;

    private float _LastDamageTime;



    // These hold references to all child objects with MeshRenderers or SkinnedMeshRenderers so the entire object can be made to flash when damage is taken.
    private List<MeshRenderer> _MeshRenderers;
    private List<SkinnedMeshRenderer> _SkinnedMeshRenderers;



    public float CurrentHealth { get; private set; }


    public delegate void Health_OnDeathEventHandler(GameObject sender);
    public delegate void Health_OnDamagedEventHandler(GameObject sender, GameObject attacker, float amount);
    public delegate void Health_OnHealedEventHandler(GameObject sender, GameObject healer, float amount);

    public event Health_OnDeathEventHandler OnDeath;
    public event Health_OnHealedEventHandler OnHeal;
    public event Health_OnDamagedEventHandler OnTakeDamage;



    private void Awake()
    {
        _MeshRenderers = new List<MeshRenderer>();
        _SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();

        CurrentHealth = MaxHealth;

        _LastDamageTime = Time.time;
        FindRenderers();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DealDamage(float amount, GameObject attacker)
    {
        if (amount <= 0)
            throw new Exception("The damage amount must be positive!");


        // If this entity is already dead, then do nothing. This way the code below won't spam the OnDeath event when an entity is dead.
        if (CurrentHealth <= 0)
            return;


        if (EnableDamageFlash &&
            Time.time - _LastDamageTime > DamageFlashTime)
        {
            _LastDamageTime = Time.time;
            

            // Only do the damage flash effect if the AI is enabled.
            if (gameObject.activeSelf)
                StartCoroutine(DoDamageFlash());
        }

            
        CurrentHealth -= amount;

        OnTakeDamage?.Invoke(gameObject, attacker, amount);

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnDeath?.Invoke(gameObject);
        }


        SpawnHealthPopup(-amount);
    }

    public void Heal(float amount, GameObject healer)
    {
        if (amount <= 0)
            throw new Exception("The damage amount must be positive!");


        CurrentHealth += amount;

        if (CurrentHealth >= MaxHealth)
            CurrentHealth = MaxHealth;

        OnHeal?.Invoke(gameObject, healer, amount);


        SpawnHealthPopup(amount);
    }

    /// <summary>
    /// Resets health to max without firing any health changed events.
    /// </summary>
    public void ResetHealthToMax()
    {
        CurrentHealth = MaxHealth;
    }

    private IEnumerator DoDamageFlash()
    {
        foreach (MeshRenderer renderer in _MeshRenderers)
            renderer.material.color = DamageFlashColor;
        foreach (SkinnedMeshRenderer renderer in _SkinnedMeshRenderers)
            renderer.material.color = DamageFlashColor;

        yield return new WaitForSeconds(DamageFlashTime);

        foreach (MeshRenderer renderer in _MeshRenderers)
            renderer.material.color = Color.white;
        foreach (SkinnedMeshRenderer renderer in _SkinnedMeshRenderers)
            renderer.material.color = Color.white;
    }

    private void SpawnHealthPopup(float healthChangedAmount)
    {
        Vector3 startPos = transform.position;
        
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        IBuilding building = GetComponent<Building_Base>();

        if (agent != null)
            startPos.y += agent.height / 2;
        else if (building != null)
            startPos.y += building.GetBuildingDefinition().Height - 1.0f;
        else if (gameObject.CompareTag("Player"))
            startPos.y += 1;
        

        HealthPopup popup = HealthPopup.ShowHealthPopup(startPos, healthChangedAmount);
    }

    private void FindRenderers()
    {
        _MeshRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        _SkinnedMeshRenderers = new List<SkinnedMeshRenderer>(GetComponentsInChildren<SkinnedMeshRenderer>());        
    }

}
