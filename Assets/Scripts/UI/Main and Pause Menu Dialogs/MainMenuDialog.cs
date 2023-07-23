using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MainMenuDialog : Dialog_Base, IDialog
{
    [SerializeField] GameObject _TitleDisplayCanvas;
    [SerializeField] CharacterSelectionDialog _CharacterSelectionDialog;
    [SerializeField] HowToPlayDialog _HowToPlayDialog;
    [SerializeField] ControlsDialog _ControlsDialog;
    [SerializeField] HighScoresDialog _HighScoresDialog;


    private Transform _MenuItems;



    protected override void Dialog_OnAwake()
    {
        _MenuItems = transform.Find("Panel/Menu Items").transform;
    }

    protected override void Dialog_OnStart()
    {
        // Hook up the mouse over event on each menu item.
        foreach (Transform t in _MenuItems)
            t.GetComponent<MenuDialogsMenuItem>().OnMouseEnter += OnMouseEnterMenuItem;

        OpenDialog();
    }

    protected override void Dialog_OnEnable()
    {
        if (_TitleDisplayCanvas)
            _TitleDisplayCanvas.SetActive(true);
    }

    protected override void Dialog_OnUpdate()
    {
        if (Utils_UI.GetSelectedMenuItemIndex(_MenuItems) < 0)
        {
            // No menu items are selected. This can happen if you click outside the menu panel. So select the first menu item.
            SelectMenuItem(0);
        }
    }

    // Dialog_OnSubmit() is omitted here on purpose, as it is not needed. The UI clicks the selected button for us when the user presses the Submit button.

    protected override void Dialog_OnCancel()
    {
        // We do nothing here, as the player should not be able to cancel out of the main menu.
    }

    public void OnMouseEnterMenuItem(GameObject sender)
    {
        int index = GetIndexOfMenuItem(sender.transform);

        SelectMenuItem(index);
    }
    
    public void OnSelectedStartGame()
    {
        CloseSelfAndHideTitleDisplay();

        _CharacterSelectionDialog.OpenDialog();
    }

    public void OnSelectedHowToPlay()
    {
        CloseSelfAndHideTitleDisplay();

        _HowToPlayDialog.OpenDialog();
    }

    public void OnSelectedControls()
    {
        CloseSelfAndHideTitleDisplay();

        _ControlsDialog.OpenDialog();
    }

    public void OnSelectedHighScores()
    {
        CloseSelfAndHideTitleDisplay();

        _HighScoresDialog.OpenDialog();
    }

    public void OnSelectedExitGame()
    {
        Application.Quit();
    }

    public override void OpenDialog(bool closeOtherOpenDialogs = false)
    {
        if (_TitleDisplayCanvas)
            _TitleDisplayCanvas.SetActive(true);

        // Select the first menu item.
        SelectMenuItem(0);
        
        base.OpenDialog(closeOtherOpenDialogs);
    }

    private void CloseSelfAndHideTitleDisplay()
    {
        _TitleDisplayCanvas.SetActive(false);
        CloseDialog();
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
