using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;


public class LevelUpDialog : Dialog_Base
{
    [Tooltip("This much time in seconds must elapse before the menu will respond to another input event to keep it from moving too fast.")]
    public float GamepadMenuSelectionDelay = 0.1f;

    public float CurrentPlayerAttackPower { get; private set; }
    public float CurrentPlayerMaxHealth { get; private set; }
    public float CurrentVillagerAttackPower { get; private set; }
    public float CurrentVillagerMaxHealth { get; private set; }


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
        CurrentVillagerAttackPower = _GameManager.VillagersStartingAttackPower;
        CurrentVillagerMaxHealth = _GameManager.VillagersStartingMaxHealth;
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
            if (y < -0.5f) // User is pressing up
            {
                _SelectedMenuItemIndex++;

                if (_SelectedMenuItemIndex >= _MenuItems.childCount)
                    _SelectedMenuItemIndex = 0;

                SelectMenuItem();

                _LastGamepadSelectionChange = Time.time;
            }
            else if (y > 0.5f) // User is pressing down
            {
                _SelectedMenuItemIndex--;

                if (_SelectedMenuItemIndex < 0)
                    _SelectedMenuItemIndex = _MenuItems.childCount - 1;

                SelectMenuItem();

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
        float buffAmount = _GameManager.BuffAmountPerLevelUp;


        // Setup the buff player attack option
        Transform menuItem = _MenuItems.GetChild(0);
        if (CurrentPlayerAttackPower < _GameManager.PlayerMaxAttackCap)
        {
            menuItem.GetComponent<TMP_Text>().text = $"Player Attack Power\t\t+{buffAmount}  ({CurrentPlayerAttackPower + buffAmount})";
            menuItem.GetComponent<Button>().interactable = true;
        }
        else
        {
            menuItem.GetComponent<TMP_Text>().text = $"Player Attack Power";
            menuItem.GetComponent<Button>().interactable = false;
        }


        // Setup the buff player max health option
        menuItem = _MenuItems.GetChild(1);
        if (CurrentPlayerMaxHealth < _GameManager.PlayerMaxHealthCap)
        {
            menuItem.GetComponent<TMP_Text>().text = $"Player Max Health\t\t+{buffAmount}  ({CurrentPlayerMaxHealth + buffAmount})";
            menuItem.GetComponent<Button>().interactable = true;
        }
        else
        {
            menuItem.GetComponent<TMP_Text>().text = $"Player Max Health";
            menuItem.GetComponent<Button>().interactable = false;
        }


        // Setup the buff villager attack option
        menuItem = _MenuItems.GetChild(2);
        if (CurrentVillagerAttackPower < _GameManager.VillagersMaxAttackCap)
        {
            menuItem.GetComponent<TMP_Text>().text = $"Villagers' Attack Power\t+{buffAmount}  ({CurrentVillagerAttackPower + buffAmount})";
            menuItem.GetComponent<Button>().interactable = true;
        }
        else
        {
            menuItem.GetComponent<TMP_Text>().text = $"Villagers' Attack Power";
            menuItem.GetComponent<Button>().interactable = false;
        }


        // Setup the buff villager max health option
        menuItem = _MenuItems.GetChild(3);
        if (CurrentVillagerMaxHealth < _GameManager.VillagersMaxHealthCap)
        {
            menuItem.GetComponent<TMP_Text>().text = $"Villagers' Max Health\t\t+{buffAmount}  ({CurrentVillagerMaxHealth + buffAmount})";
            menuItem.GetComponent<Button>().interactable = true;
        }
        else
        {
            menuItem.GetComponent<TMP_Text>().text = $"Villagers' Max Health\t\t+{buffAmount}  ({CurrentVillagerMaxHealth + buffAmount})";
            menuItem.GetComponent<Button>().interactable = false;
        }


        // Setup the heal player option.
        menuItem = _MenuItems.GetChild(4);
        Health pHealth = _Player.GetComponent<Health>();
        float amountToHeal = pHealth.MaxHealth - pHealth.CurrentHealth;

        if (pHealth.CurrentHealth < pHealth.MaxHealth &&
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

    public void OnBuffPlayerAttack()
    {
        CloseDialog();

        CurrentPlayerAttackPower += _GameManager.BuffAmountPerLevelUp;
        _Player.GetComponent<PlayerController>().AttackPower = CurrentPlayerAttackPower;

        
    }

    public void OnBuffPlayerMaxHealth()
    {
        CloseDialog();

        CurrentPlayerMaxHealth += _GameManager.BuffAmountPerLevelUp;
        
        Health pHealth = _Player.GetComponent<Health>();
        pHealth.MaxHealth = CurrentPlayerMaxHealth;
        pHealth.Heal(_GameManager.BuffAmountPerLevelUp, null);
        
    }

    public void OnBuffVillagerAttack()
    {
        CloseDialog();

        CurrentVillagerAttackPower += _GameManager.BuffAmountPerLevelUp;
        _VillageManager_Villagers.BuffVillagers(false);
    }

    public void OnBuffVillagerMaxHealth()
    {
        CloseDialog();

        CurrentVillagerMaxHealth += _GameManager.BuffAmountPerLevelUp;
        _VillageManager_Villagers.BuffVillagers(true);
    }

    public void OnBuffHealPlayer()
    {
        CloseDialog();

        Health playerHealth = _Player.GetComponent<Health>();
        float healAmount = playerHealth.MaxHealth - playerHealth.CurrentHealth;
        playerHealth.ResetHealthToMax();

        _GameManager.ResourceManager.Stockpiles[ResourceTypes.Food] -= Mathf.CeilToInt(healAmount * _GameManager.PlayerHealFoodCostMultiplier);

    }

}
