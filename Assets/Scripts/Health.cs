using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;


public class Health : MonoBehaviour
{
    public float MaxHealth = 100;

    public bool EnableDamageFlash = true;
    public Color DamageFlashColor = Color.red;
    public float DamageFlashTime = 0.1f;

    [Tooltip("The damage types this entity is resistant to.")]
    public DamageTypes IsResistantTo;
    [Tooltip("The damage types this entity is vulnerable to.")]
    public DamageTypes IsVulnerableTo;

    public bool IsInvincible = false;


    private GameManager _GameManager;
    private MonsterManager _MonsterManager;

    private float _LastDamageTime;



    // These hold references to all child objects with MeshRenderers or SkinnedMeshRenderers so the entire object can be made to flash when damage is taken.
    private List<MeshRenderer> _MeshRenderers;
    private List<SkinnedMeshRenderer> _SkinnedMeshRenderers;



    public float CurrentHealth { get; private set; }


    public delegate void Health_OnDeathEventHandler(GameObject sender, GameObject attacker);
    public delegate void Health_OnDamagedEventHandler(GameObject sender, GameObject attacker, float amount, DamageTypes damageType);
    public delegate void Health_OnHealedEventHandler(GameObject sender, GameObject healer, float amount);
    public delegate void Health_OnHealthChangedEventHandler(GameObject sender, GameObject changeSource, float changeAmount);

    public event Health_OnDeathEventHandler OnDeath;
    public event Health_OnHealedEventHandler OnHeal;
    public event Health_OnDamagedEventHandler OnTakeDamage;
    public event Health_OnHealthChangedEventHandler OnHealthChanged;



    private void Awake()
    {
        _GameManager = GameManager.Instance;
        _MonsterManager = _GameManager.MonsterManager;

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

    public void DealDamage(float amount, DamageTypes damageType, GameObject attacker)
    {
        if (amount <= 0)
            throw new Exception("The damage amount must be positive!");


        // If this entity is already dead, then do nothing. This way the code below won't spam the OnDeath event when an entity is dead.
        if (CurrentHealth <= 0 || IsInvincible)
            return;


        if (EnableDamageFlash &&
            Time.time - _LastDamageTime > DamageFlashTime)
        {
            _LastDamageTime = Time.time;
            

            // Only do the damage flash effect if the AI is enabled.
            if (gameObject.activeSelf)
                StartCoroutine(DoDamageFlash());
        }



        float randomVariance = Random.Range(0.0f, _GameManager.AttackDamageVariance);
        float changeAmount = Mathf.CeilToInt(amount - (amount * randomVariance));
        changeAmount = Mathf.Max(changeAmount, 1.0f); // Enforce that the attack will always at least 1 point of damage.

        float buffAmount = 0;
        if (GetComponent<Monster_Base>() != null)
        {
            if ((IsResistantTo & damageType) > 0)
                buffAmount = (amount * _MonsterManager.DamageResistanceBuffAmount) * -1;
            else if ((IsVulnerableTo & damageType) > 0)
                buffAmount = amount * _MonsterManager.DamageVulnerabilityBuffAmount;
        }

        changeAmount += buffAmount;
        //Debug.Log($"Amount: {amount}    Buff: {buffAmount}");

        CurrentHealth -= changeAmount;
        if (CurrentHealth < 0)
            changeAmount = changeAmount + CurrentHealth; // Calculate how many damage points it took to kill this entity. So if it had 10 health and took 12 damage, this will return 10.

        OnHealthChanged?.Invoke(gameObject, attacker, -changeAmount);
        OnTakeDamage?.Invoke(gameObject, attacker, changeAmount, damageType);

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnDeath?.Invoke(gameObject, attacker);
        }


        SpawnHealthPopup(-changeAmount, buffAmount);
    }

    public void Heal(float amount, GameObject healer)
    {
        if (amount <= 0)
            throw new Exception("The damage amount must be positive!");


        CurrentHealth += amount;

        float changeAmount = amount;
        if (CurrentHealth > MaxHealth)
        {
            changeAmount = CurrentHealth - MaxHealth;
            CurrentHealth = MaxHealth;
        }
            

        OnHealthChanged?.Invoke(gameObject, healer, changeAmount);
        OnHeal?.Invoke(gameObject, healer, changeAmount);

        SpawnHealthPopup(changeAmount);
    }

    /// <summary>
    /// Resets health to max without firing any health changed events.
    /// </summary>
    public void ResetHealthToMax()
    {
        float changeAmount = MaxHealth - CurrentHealth;
        CurrentHealth = MaxHealth;

        OnHealthChanged?.Invoke(gameObject, null, changeAmount);
        OnHeal?.Invoke(gameObject, null, changeAmount);
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

    private void SpawnHealthPopup(float healthChangedAmount, float buffAmount = 0)
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
        

        HealthPopup popup = HealthPopup.ShowHealthPopup(startPos, healthChangedAmount, buffAmount);
    }

    private void FindRenderers()
    {
        _MeshRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        _SkinnedMeshRenderers = new List<SkinnedMeshRenderer>(GetComponentsInChildren<SkinnedMeshRenderer>());        
    }

}
