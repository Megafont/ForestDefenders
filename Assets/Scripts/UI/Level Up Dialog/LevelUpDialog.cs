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
        float buffAmount = _GameManager.BuffAmountPerLevelUp;

        _DescriptionText.text = $"Select A Buff:";

        _MenuItems.GetChild(0).GetComponent<TMP_Text>().text = $"Player Attack Power\t\t+{buffAmount}  ({CurrentPlayerAttackPower + buffAmount})";
        _MenuItems.GetChild(1).GetComponent<TMP_Text>().text = $"Player Max Health\t\t+{buffAmount}  ({CurrentPlayerMaxHealth + buffAmount})";
        _MenuItems.GetChild(2).GetComponent<TMP_Text>().text = $"Villagers' Attack Power\t+{buffAmount}  ({CurrentVillagerAttackPower + buffAmount})";
        _MenuItems.GetChild(3).GetComponent<TMP_Text>().text = $"Villagers' Max Health\t\t+{buffAmount}  ({CurrentVillagerMaxHealth + buffAmount})";
        _MenuItems.GetChild(4).GetComponent<TMP_Text>().text = $"Heal Player"; // "Up To {_GameManager.PlayerHealAmount} HP";

        // Select the first menu item.
        EventSystem.current.SetSelectedGameObject(_MenuItems.GetChild(0).gameObject);

        base.OpenDialog(closeOtherOpenDialogs);
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
        playerHealth.ResetHealthToMax();
    }

}
