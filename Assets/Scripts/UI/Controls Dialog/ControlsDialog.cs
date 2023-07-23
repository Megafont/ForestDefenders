using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem.Samples.RebindUI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEditor;

public class ControlsDialog : Dialog_Base, IDialog
{
    [Tooltip("The dialog that opens this one.")]
    [SerializeField] private Dialog_Base _ParentDialog;

    [Header("Control Types Menu")]
    [SerializeField] private GameObject _ControlTypesMenuPane;
    [SerializeField] private Transform _ControlTypesMenuItems;

    [Header("Scroll View")]
    [SerializeField] private GameObject _ScrollView;
    [SerializeField] private GameObject _ScrollViewContentArea;
    [SerializeField] private GameObject _KeyboardAndMouseControlsPane;
    [SerializeField] private GameObject _GamepadControlsPane;



    private ControlsPageTypes _SelectedControlsPage = ControlsPageTypes.KeyboardAndMouse;

    private ScrollRect _ScrollRect;

    private GameObject _CurrentSelectedObject;
    private GameObject _PrevSelectedObject;



    protected override void Dialog_OnStart()
    {
        _CurrentSelectedObject = null;
        _PrevSelectedObject = null;


        _ScrollRect = transform.GetComponentInChildren<ScrollRect>();
        if (_ScrollRect == null)
            Debug.LogError("The ScrollView's ScrollRect component was not found!");


        // Hook up the mouse over event on each menu item.
        foreach (Transform t in _ControlTypesMenuItems)
            t.GetComponent<MenuDialogsMenuItem>().OnMouseEnter += OnMouseEnterMenuItem;


        gameObject.SetActive(false);
    }

    protected override void Dialog_OnUpdate()
    {
        if (EventSystem.current)
        {
            _PrevSelectedObject = _CurrentSelectedObject;
            _CurrentSelectedObject = EventSystem.current.currentSelectedGameObject;

            if (_CurrentSelectedObject)
                OnSelectedButtonChanged(_CurrentSelectedObject.GetComponent<RectTransform>());
        }
        else
        {
            _PrevSelectedObject = null;
            _CurrentSelectedObject = null;
        }
    }

    public override void OpenDialog(bool closeOtherOpenDialogs = true)
    {
        // Always start this dialog in the control types menu.
        OpenControlTypesMenuPane();

        base.OpenDialog(closeOtherOpenDialogs);
    }

    public void OpenControlTypesMenuPane()
    {
        _SelectedControlsPage = ControlsPageTypes.ControlTypesMenu;

        CloseAllPanes();

        _ControlTypesMenuPane.SetActive(true);
        Utils_UI.SelectFirstButtonInPane(_ControlTypesMenuPane.transform);
    }

    public void OpenKeyboardAndMouseControlsPane()
    {
        if (_SelectedControlsPage == ControlsPageTypes.KeyboardAndMouse)
            return;


        _SelectedControlsPage = ControlsPageTypes.KeyboardAndMouse;

        CloseAllPanes();        
        _ScrollView.SetActive(true);
        //_ScrollRect.verticalNormalizedPosition = 0; // Scroll the scroll view back to the top.
        _KeyboardAndMouseControlsPane.SetActive(true);
        Utils_UI.SelectFirstButtonInPane(_KeyboardAndMouseControlsPane.transform);
    }

    public void OpenGamepadControlsPane()
    {
        if (_SelectedControlsPage == ControlsPageTypes.Gamepad)
            return;


        _SelectedControlsPage = ControlsPageTypes.Gamepad;

        CloseAllPanes();

        _ScrollView.SetActive(true);
        //_ScrollRect.verticalNormalizedPosition = 0; // Scroll the scroll view back to the top.
        _GamepadControlsPane.SetActive(true);
        Utils_UI.SelectFirstButtonInPane(_GamepadControlsPane.transform);
    }

    public void OnReturnToMainMenuClick()
    {
        _InputManager.SaveKeyBindings();

        CloseDialog();


        if (_ParentDialog is MainMenuDialog)
            ((MainMenuDialog)_ParentDialog).OpenDialog();
        else if (_ParentDialog is PauseMenuDialog)
            ((PauseMenuDialog)_ParentDialog).OpenDialog();
        else
            throw new System.Exception("The specified parent dialog is of an invalid type!");
    }

    private void CloseAllPanes()
    {
        _ControlTypesMenuPane.SetActive(false);
        _KeyboardAndMouseControlsPane.SetActive(false);
        _GamepadControlsPane.SetActive(false);

        _ScrollView.SetActive(false);
    }

    private EventSystem _EventSystem;
    public void OnRebindingStarted()
    {
        // Grab a reference to the current EventSystem and disable it. This prevents the
        // selected button from being able to change while the user is rebinding a control.
        // We also save a reference here, because disabling the current EventSystem apparently
        // causes EventSystem.current to become null. This way, OnBindingEnded() can use the
        // stored reference to re-enable this EventSystem.

        _EventSystem = EventSystem.current;
        _EventSystem.enabled = false;
    }

    public void OnRebindingEnded()
    {
        _EventSystem.enabled = true;
    }

    private void OnSelectedButtonChanged(RectTransform button)
    {
        if (_SelectedControlsPage != ControlsPageTypes.ControlTypesMenu)
            _ScrollRect.FocusOnItem(button);
    }

    /// <summary>
    /// We simply do nothing in this event, because otherwise it will interrupt the player clicking on a button.
    /// </summary>
    protected override void Dialog_OnSubmit()
    {

    }
    
    /// <summary>
    /// We simply do nothing in this event, since we only want the player to exit this dialog by choosing
    /// Return to Main Menu from the control types menu.
    /// </summary>
    protected override void Dialog_OnCancel() 
    {

    }

    public void OnMouseEnterMenuItem(GameObject sender)
    {
        int index = Utils_UI.GetIndexOfMenuItem(_ControlTypesMenuItems, sender.transform);

        Utils_UI.SelectMenuItem(_ControlTypesMenuItems, index);
    }

    public void ResetAllPlayerControlsInCategory(Button button)
    {
        Transform parent = button.gameObject.transform.parent.parent;

        RebindActionUI[] rebindActionUIs = parent.GetComponentsInChildren<RebindActionUI>();


        foreach (RebindActionUI rebindUI in rebindActionUIs)
        {
            Button resetButton = rebindUI.transform.Find("ResetToDefaultButton").transform.Find("ResetButton").GetComponent<Button>();

            //Debug.Log($"Resetting {resetButton.transform.parent.parent.name}...");
            resetButton.onClick.Invoke();
        }

    }
}
