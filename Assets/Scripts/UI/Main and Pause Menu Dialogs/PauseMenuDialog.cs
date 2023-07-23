using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class PauseMenuDialog : Dialog_Base, IDialog
{
    [SerializeField] HowToPlayDialog _HowToPlayDialog;
    [SerializeField] ControlsDialog _ControlsDialog;



    private SceneSwitcher _SceneSwitcher;
    private Transform _MenuItems;
   


    protected override void Dialog_OnAwake()
    {
        _MenuItems = transform.Find("Panel/Menu Items").transform;
    }

    protected override void Dialog_OnStart()
    {
        _SceneSwitcher = _GameManager.SceneSwitcher;

        // Hook up the mouse over event on each menu item.
        foreach (Transform t in _MenuItems)
            t.GetComponent<MenuDialogsMenuItem>().OnMouseEnter += OnMouseEnterMenuItem;
    }

    protected override void Dialog_OnUpdate()
    {
        if (Utils_UI.GetSelectedMenuItemIndex(_MenuItems) < 0)
        {
            // No menu items are selected. This can happen if you click outside the menu panel. So select the first menu item.
            SelectMenuItem(0);
        }

        if (_InputManager_UI.UnpauseGame)
            OnSelectedResumeGame();
    }

    protected override void Dialog_OnEnable()
    {

    }

    // Dialog_OnSubmit() is omitted here on purpose, as it is not needed. The UI clicks the selected button for us when the user presses the Submit button.

    protected override void Dialog_OnCancel()
    {
        OnSelectedResumeGame();
    }

    public void OnMouseEnterMenuItem(GameObject sender)
    {
        int index = GetIndexOfMenuItem(sender.transform);

        SelectMenuItem(index);
    }
    
    public void OnSelectedResumeGame()
    {
        _GameManager.TogglePauseGameState();
    }

    public void OnSelectedHowToPlay()
    {
        CloseDialog();

        _HowToPlayDialog.OpenDialog();
    }

    public void OnSelectedControls()
    {
        CloseDialog();

        _ControlsDialog.OpenDialog();
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


    public override void OpenDialog(bool closeOtherOpenDialogs = false)
    {        
        SelectMenuItem(0);

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

    private void SelectMenuItem(int index)
    {
        // Tell the Unity EventSystem to select the correct button.
        EventSystem.current.SetSelectedGameObject(_MenuItems.GetChild(index).gameObject);
    }

}
