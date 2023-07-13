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

    protected override void Dialog_OnSubmit()
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
        OnSelectedResumeGame();
    }

    public void OnMouseEnterMenuItem(GameObject sender)
    {
        _SelectedMenuItemIndex = GetIndexOfMenuItem(sender.transform);

        SelectMenuItem();
    }
    
    public void OnSelectedResumeGame()
    {
        _GameManager.TogglePauseGameState();
    }

    public void OnSelectedReturnToMainMenu()
    {
        // We have to reset the time scale since it is set to 0 while the game is paused. Otherwise the transition between scenes won't work.
        Time.timeScale = 1.0f;
        _SceneSwitcher.FadeToScene("Main Menu");
    }

    public void OnSelectedExitGame()
    {
        Time.timeScale = 1.0f;
        Application.Quit();
    }


    public override void OpenDialog(bool closeOtherOpenDialogs = true)
    {
        // Select the first menu item.
        _SelectedMenuItemIndex = 0;
        SelectMenuItem();

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
