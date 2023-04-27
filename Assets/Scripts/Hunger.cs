using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;


public class Hunger : MonoBehaviour
{
    [Tooltip("The maximum hunger level this character can handle.")]
    [SerializeField] private float _MaxHunger = 100;

    [Tooltip("How often in seconds that hunger will increase.")]
    [SerializeField] private float _HungerIncreaseFrequency = 5f;
    [Tooltip("How many points hunger increases by each time.")]
    [SerializeField] private float _HungerIncreaseAmount = 1f;
    [Tooltip("The amount of food needed to reduce hunger by 1 point.")]
    [SerializeField] private float _FoodCostToHealOneHungerPoint = 1f;
    [Tooltip("The amount of hunger points that will be healed each time the character eats.")]
    [SerializeField] private float _HungerPointsToHealEachTimeOnEating = 5f;    
    [Tooltip("How often in seconds that starvation will cause damage.")]
    [SerializeField] private float _StarvationDamageFrequency = 9f; // Defaulting this to 9 seconds means that starvation will kill in 2 minutes if character doesn't eat (when _StarvationDamagePercent is 5%).
    [Tooltip("The percentage of the character's health that is lost each time starvation damage is dealt.")]
    [Range(0f, 1f)]
    [SerializeField] private float _StarvationDamagePercent = 0.05f;



    private GameManager _GameManager;
    private ResourceManager _ResourceManager;
    
    private float _LastHungerIncreaseTime;
    
    private float _StarvationTimeElapsed;
        
    private Health _ParentHealthComponent;



    public delegate void Hunger_OnHungerChangedEventHandler(GameObject sender, float changeAmount);

    public event Hunger_OnHungerChangedEventHandler OnHungerChanged;



    // Start is called before the first frame update
    void Start()
    {
        _GameManager = GameManager.Instance;
        _ResourceManager = _GameManager.ResourceManager;

        _ParentHealthComponent = GetComponent<Health>();    
    }

    // Update is called once per frame
    void Update()
    {
        DoHungerCheck();
        DoStarvationCheck();
    }


    private void DoHungerCheck()
    {
        // Is it time for hunger to increase again?
        if (Time.time - _LastHungerIncreaseTime >= _HungerIncreaseFrequency)
        {
            float prevHungerLevel = CurrentHunger;


            // Increase the character's hunger level.
            _LastHungerIncreaseTime = Time.time;
            CurrentHunger = Mathf.Min(CurrentHunger + _HungerIncreaseAmount, 100f);


            // If we are NOT the player and hunger level is greater than 0, then try to eat some food to alleviatge it.
            if (gameObject.tag != "Player" && CurrentHunger > 0)
                AttemptToEat();


            OnHungerChanged?.Invoke(gameObject, CurrentHunger - prevHungerLevel);
        }

    }

    private void DoStarvationCheck()
    {
        if (CurrentHunger == MaxHunger)
        {
            _StarvationTimeElapsed += Time.deltaTime;

            // Is it time to apply starvation damage again?
            if (_StarvationTimeElapsed >= _StarvationDamageFrequency &&
                _ParentHealthComponent.CurrentHealth > 0)
            {
                float damageAmount = _ParentHealthComponent.CurrentHealth * _StarvationDamagePercent;
                _ParentHealthComponent.DealDamage(damageAmount, DamageTypes.Starvation, null);
                _StarvationTimeElapsed -= _StarvationDamageFrequency;
            }
        }

    }

    private void AttemptToEat()
    {
        float foodNeeded = CurrentHunger * _FoodCostToHealOneHungerPoint;


        // Draw a random number to see if this character will eat some food this time.
        // The more hungery the character is, the more likely it is that they will try to eat.
        float rand = Random.Range(1, 100);
        if (rand <= CurrentHunger)
        {
            // How many points of hunger will be alleviated this time?
            float hungerPtsToRestore = Mathf.Min(CurrentHunger, HungerPointsToHealEachTimeOnEating);

            // Calculate total food cost.
            float totalFoodCost = _FoodCostToHealOneHungerPoint * hungerPtsToRestore;


            if (_ResourceManager.ExpendFromStockpile(ResourceTypes.Food, totalFoodCost))
            {
                //Debug.Log($"Eating. Healed {hungerPtsToRestore} hunger with {totalFoodCost} food.");

                CurrentHunger -= hungerPtsToRestore;

                // NOTE: We DO NOT invoke the OnHungerChanged event here, because that is handled
                // in DoHungerCheck(), which calls this function.
            }
        }


    }

    /// <summary>
    /// This function is used by the player object to alleviate hunger when he gathers food.
    /// </summary>
    /// <param name="amount">The amount of hunger to alleviate.</param>
    public void AlleviateHunger(float amount)
    {
        CurrentHunger = Mathf.Max(0, CurrentHunger - amount);

        OnHungerChanged?.Invoke(gameObject, -amount);
    }

    public void SetMaxHunger(float amount)
    {
        _MaxHunger = amount;
        TotalMaxHungerIncrease = 0;
    }

    public void IncreaseMaxHealth(float amount)
    {
        _MaxHunger += amount;
        TotalMaxHungerIncrease += amount;
    }



    public float CurrentHunger { get; private set; } = 0;
    public float HungerPointsToHealEachTimeOnEating { get { return _HungerPointsToHealEachTimeOnEating; } }
    public float HungerReductionFoodAmount { get { return _FoodCostToHealOneHungerPoint; } }
    public float MaxHunger
    {
        get { return _MaxHunger; }
        private set { _MaxHunger = value; }
    }

    public float TotalMaxHungerIncrease { get; private set; }

}
