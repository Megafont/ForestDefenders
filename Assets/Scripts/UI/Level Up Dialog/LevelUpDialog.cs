using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;
using UnityEditor.SceneManagement;

public class LevelUpDialog : Dialog_Base
{
    [Range(2, 5)]
    public float NumberOfBuffOptions = 4;

    [Tooltip("This much time in seconds must elapse before the menu will respond to another input event to keep it from moving too fast.")]
    public float GamepadMenuSelectionDelay = 0.1f;

    public float CurrentPlayerAttackPower { get; private set; }
    public float CurrentPlayerMaxHealth { get; private set; }
    public float CurrentPlayerGatherRate { get; private set; }

    public float CurrentVillagerAttackPower { get; private set; }
    public float CurrentVillagerMaxHealth { get; private set; }
    public float CurrentVillagerGatherRate { get; private set; }



    private GameObject _Player;
    private VillageManager_Villagers _VillageManager_Villagers;

    private TMP_Text _DescriptionText;
    private Transform _MenuItems;


    private float _LastGamepadSelectionChange;
    private int _SelectedMenuItemIndex;



    protected override void Dialog_OnAwake()
    {
        _IgnoreGamePhaseText = true;

        _DescriptionText = transform.Find("Panel/Description Text (TMP)").GetComponent<TMP_Text>();
        _MenuItems = transform.Find("Panel/Menu Items").transform;


        CurrentPlayerAttackPower = _GameManager.PlayerStartingAttackPower;
        CurrentPlayerMaxHealth = _GameManager.PlayerStartingMaxHealth;
        CurrentPlayerGatherRate = _GameManager.PlayerStartingGatherRate;

        CurrentVillagerAttackPower = _GameManager.VillagersStartingAttackPower;
        CurrentVillagerMaxHealth = _GameManager.VillagersStartingMaxHealth;
        CurrentVillagerGatherRate = _GameManager.VillagersStartingGatherRate;
    }

    protected override void Dialog_OnStart()
    {
        if (!_GameManager.PlayerIsInGame())
            return;

        _Player = _GameManager.Player;
        _VillageManager_Villagers = _GameManager.VillageManager_Villagers;

        Health pHealth = _Player.GetComponent<Health>();
        pHealth.MaxHealth = CurrentPlayerMaxHealth;
        pHealth.ResetHealthToMax();


        foreach (Transform t in _MenuItems)
            t.GetComponent<LevelUpMenuItem>().OnMouseUp += OnButtonClicked;

    }

    protected override void Dialog_OnUpdate()
    {
        if (Time.time - _LastGamepadSelectionChange >= GamepadMenuSelectionDelay)
        {
            // If the mouse has caused the selection to be lost by clicking not on a button, then reselect the currently selected button according to this class's stored index.
            if (EventSystem.current.currentSelectedGameObject == null)
                SelectMenuItem();

            //Debug.Log("Selected: " + EventSystem.current.currentSelectedGameObject.name);

            float y = _InputManager_UI.Navigate.y;
            if (y < -0.5f) // User is pressing down
            {
                // Skip the next item if it is disabled.
                while (true)
                {
                    _SelectedMenuItemIndex++;

                    if (_SelectedMenuItemIndex >= _MenuItems.childCount)
                        _SelectedMenuItemIndex = 0;

                    SelectMenuItem();

                    Button selected = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
                    if (selected != null && selected.IsInteractable())
                        break;
                }

                _LastGamepadSelectionChange = Time.time;
            }
            else if (y > 0.5f) // User is pressing up
            {
                // Skip the next item if it is disabled.
                while (true)
                {
                    _SelectedMenuItemIndex--;

                    if (_SelectedMenuItemIndex < 0)
                        _SelectedMenuItemIndex = _MenuItems.childCount - 1;

                    SelectMenuItem();

                    Button selected = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
                    if (selected != null && selected.IsInteractable())
                        break;
                }

                _LastGamepadSelectionChange = Time.time;
            }



            if (_InputManager_UI.Confirm)
            {
                Button pressedBtn = _MenuItems.GetChild(_SelectedMenuItemIndex).GetComponent<Button>();

                PointerEventData eventData = new PointerEventData(EventSystem.current);
                pressedBtn.OnPointerClick(eventData);

                _LastGamepadSelectionChange = Time.time;
            }

        }

    }


    public override void OpenDialog(bool closeOtherOpenDialogs = true)
    {
        _DescriptionText.text = $"Select A Buff:";


        RefreshMenuItems();


        // Select the first menu item.
        EventSystem.current.SetSelectedGameObject(_MenuItems.GetChild(0).gameObject);

        base.OpenDialog(closeOtherOpenDialogs);
    }

    private void RefreshMenuItems()
    {
        // Setup the buff player attack option
        GameObject menuItem = _MenuItems.GetChild(0).gameObject;
        RefreshStatMenuItem(menuItem, "Player Attack Power", 2, CurrentPlayerAttackPower, _GameManager.MaxAttackPowerCap, _GameManager.PlayerAttackBuffAmountPerLevelUp);


        // Setup the buff player max health option
        menuItem = _MenuItems.GetChild(1).gameObject;
        RefreshStatMenuItem(menuItem, "Player Max Health", 2, CurrentPlayerMaxHealth, _GameManager.MaxHealthCap, _GameManager.PlayerHealthBuffAmountPerLevelUp);

        // Setup the buff player gather rate option
        menuItem = _MenuItems.GetChild(2).gameObject;
        RefreshStatMenuItem(menuItem, "Player Gather Rate", 2, CurrentPlayerGatherRate, _GameManager.MaxGatheringCap, _GameManager.PlayerGatheringBuffAmountPerLevelUp);

        // Setup the heal player option.
        menuItem = _MenuItems.GetChild(3).gameObject;
        Health pHealth = _Player.GetComponent<Health>();
        float amountToHeal = pHealth.MaxHealth - pHealth.CurrentHealth;
        RefreshHealMenuItem(menuItem, "Heal Player", pHealth.CurrentHealth, pHealth.MaxHealth, amountToHeal);


        // Setup the buff villager attack option
        menuItem = _MenuItems.GetChild(4).gameObject;
        RefreshStatMenuItem(menuItem, "Villager's Attack Power", 1, CurrentVillagerAttackPower, _GameManager.MaxAttackPowerCap, _GameManager.VillagersAttackBuffAmountPerLevelUp);


        // Setup the buff villager attack option
        menuItem = _MenuItems.GetChild(5).gameObject;
        RefreshStatMenuItem(menuItem, "Villager's Max Health", 2, CurrentVillagerMaxHealth, _GameManager.MaxHealthCap, _GameManager.VillagersHealthBuffAmountPerLevelUp);

        // Setup the buff villager gather rate option
        menuItem = _MenuItems.GetChild(6).gameObject;
        RefreshStatMenuItem(menuItem, "Villager's Gather Rate", 1, CurrentVillagerGatherRate, _GameManager.MaxGatheringCap, _GameManager.VillagersGatheringBuffAmountPerLevelUp);

    }

    private void RefreshStatMenuItem(GameObject menuItem, string descriptionText, int tabCount, float statCurrentValue, float statMaxValue, float buffAmount)
    {
        if (CurrentVillagerMaxHealth < _GameManager.MaxHealthCap)
        {
            string strTabs = new string('\t', tabCount);
            menuItem.GetComponent<TMP_Text>().text = $"{descriptionText}{strTabs}+{buffAmount}  ({statCurrentValue + buffAmount})";
            menuItem.GetComponent<Button>().interactable = true;
        }
        else
        {
            menuItem.GetComponent<TMP_Text>().text = $"{descriptionText}";
            menuItem.GetComponent<Button>().interactable = false;
        }
    }    

    private void RefreshHealMenuItem(GameObject menuItem, string descriptionText, float healthCurrentValue, float healthMaxValue, float amountToHeal)
    {
        if (healthCurrentValue < healthMaxValue &&
            _GameManager.ResourceManager.Stockpiles[ResourceTypes.Food] >= amountToHeal)
        {
            menuItem.GetComponent<TMP_Text>().text = $"Heal Player\t\t\t(-{amountToHeal} Food)"; // "Up To {_GameManager.PlayerHealAmount} HP";
            menuItem.GetComponent<Button>().interactable = true;
        }
        else
        {
            menuItem.GetComponent<TMP_Text>().text = $"Heal Player";
            menuItem.GetComponent<Button>().interactable = false;
        }
    }

    private int GetIndexOfMenuItem(Transform child)
    {
        int index = 0;
        foreach (Transform t in _MenuItems)
        {
            if (t == child)
                return index;

            index++;
        }


        return -1;
    }

    private void SelectMenuItem()
    {
        // Tell the Unity EventSystem to select the correct button.
        EventSystem.current.SetSelectedGameObject(_MenuItems.GetChild(_SelectedMenuItemIndex).gameObject);
    }



    public void OnButtonClicked(GameObject sender)
    {
        _SelectedMenuItemIndex = GetIndexOfMenuItem(sender.transform);
    }



    public void BuffPlayerAttackPower()
    {
        CloseDialog();

        CurrentPlayerAttackPower += _GameManager.PlayerAttackBuffAmountPerLevelUp;
        _Player.GetComponent<PlayerController>().AttackPower = CurrentPlayerAttackPower;        
    }

    public void BuffPlayerMaxHealth()
    {
        CloseDialog();

        CurrentPlayerMaxHealth += _GameManager.PlayerHealthBuffAmountPerLevelUp;
        
        Health pHealth = _Player.GetComponent<Health>();
        pHealth.MaxHealth = CurrentPlayerMaxHealth;
        pHealth.Heal(_GameManager.PlayerHealthBuffAmountPerLevelUp, null);
        
    }

    public void BuffPlayerGatherRate()
    {
        CloseDialog();

        CurrentPlayerGatherRate += _GameManager.PlayerGatheringBuffAmountPerLevelUp;
    }


    public void BuffHealPlayer()
    {
        CloseDialog();

        Health playerHealth = _Player.GetComponent<Health>();
        float healAmount = playerHealth.MaxHealth - playerHealth.CurrentHealth;
        playerHealth.ResetHealthToMax();

        _GameManager.ResourceManager.Stockpiles[ResourceTypes.Food] -= Mathf.CeilToInt(healAmount * _GameManager.PlayerHealFoodCostMultiplier);

    }

    public void BuffVillagerAttackPower()
    {
        CloseDialog();

        CurrentVillagerAttackPower += _GameManager.VillagersAttackBuffAmountPerLevelUp;
        _VillageManager_Villagers.BuffVillagers(false);
    }

    public void BuffVillagerMaxHealth()
    {
        CloseDialog();

        CurrentVillagerMaxHealth += _GameManager.VillagersHealthBuffAmountPerLevelUp;
        _VillageManager_Villagers.BuffVillagers(true);
    }

    public void BuffVillagerGatherRate()
    {
        CloseDialog();

        CurrentVillagerGatherRate += _GameManager.VillagersGatheringBuffAmountPerLevelUp;
    }


}
