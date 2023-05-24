using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class PauseMenuDialog : Dialog_Base, IDialog
{
    private SceneSwitcher _SceneSwitcher;
    private Transform _MenuItems;


    private float _LastGamepadSelectionChangeTime;
    private int _SelectedMenuItemIndex;
    


    protected override void Dialog_OnAwake()
    {
        _MenuItems = transform.Find("Panel/Menu Items").transform;
    }

    protected override void Dialog_OnStart()
    {
        _SceneSwitcher = _GameManager.SceneSwitcher;

        foreach (Transform t in _MenuItems)
            t.GetComponent<MenuDialogsMenuItem>().OnMouseEnter += OnMouseEnterMenuItem;
    }

    protected override void Dialog_OnEnable()
    {

    }

    protected override void Dialog_OnUpdate()
    {
        /// NOTE: We have to use Time.unscaledTime here since the time scale is set to 0 while the game is paused.
        if (Time.unscaledTime - _LastGamepadSelectionChangeTime >= _GameManager.GamepadMenuSelectionDelay)
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

                _LastGamepadSelectionChangeTime = Time.unscaledTime;
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

                _LastGamepadSelectionChangeTime = Time.unscaledTime;
            }


        }

    }

    protected override void Dialog_OnConfirm()
    {
        if (!_SceneSwitcher.IsTransitioningToScene)
        {
            Button pressedBtn = _MenuItems.GetChild(_SelectedMenuItemIndex).GetComponent<Button>();

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            pressedBtn.OnPointerClick(eventData);

            _LastGamepadSelectionChangeTime = Time.unscaledTime;
        }
    }

    protected override void Dialog_OnCancel()
    {
        OnResumeGame();
    }

    public void OnMouseEnterMenuItem(GameObject sender)
    {
        _SelectedMenuItemIndex = GetIndexOfMenuItem(sender.transform);

        SelectMenuItem();
    }
    
    public void OnResumeGame()
    {
        _GameManager.TogglePauseGameState();
    }

    public void OnReturnToMainMenu()
    {
        // We have to reset the time scale since it is set to 0 while the game is paused. Otherwise the transition between scenes won't work.
        Time.timeScale = 1.0f;
        _SceneSwitcher.FadeToScene("Main Menu");
    }

    public void OnExitGame()
    {
        Time.timeScale = 1.0f;
        Application.Quit();
    }


    public override void OpenDialog(bool closeOtherOpenDialogs = true)
    {
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

}
