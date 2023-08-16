using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;



public class HowToPlayDialog : Dialog_Base, IDialog
{

    [SerializeField] private Dialog_Base _ParentDialog;
    [SerializeField] private ScrollRect _ScrollRect;



    protected override void Dialog_OnAwake()
    {

    }

    protected override void Dialog_OnStart()
    {

    }

    public override void OpenDialog(bool closeOtherOpenDialogs = true)
    {
        // Always start scrolled to the top of the text box.
        _ScrollRect.normalizedPosition = Vector2.one;

        base.OpenDialog(closeOtherOpenDialogs);
    }

    public void OnDoneClicked()
    {
        ReturnToMainMenu();
    }

    protected override void Dialog_OnNavigate()
    {
        float scrollMagnitudeY = _InputManager_UI.Navigate.y;
        //float scrollMagnitudeX = _InputManager_UI.Navigate.x;


        // Check if the user is pressing up or down. If so, scrolls the high scores table.        
        if (scrollMagnitudeY != 0)
        {
            float scrollableHeight = _ScrollRect.content.sizeDelta.y - _ScrollRect.viewport.rect.height;
            float scrollAmount = DIALOG_SCROLL_SPEED * scrollMagnitudeY;
            _ScrollRect.verticalNormalizedPosition += scrollAmount / scrollableHeight;
        }
        

    }

    protected override void Dialog_OnSubmit()
    {
        ReturnToMainMenu();
    }

    protected override void Dialog_OnCancel()
    {
        ReturnToMainMenu();
    }

    public void ReturnToMainMenu()
    {
        CloseDialog();


        if (_ParentDialog is MainMenuDialog)
            ((MainMenuDialog)_ParentDialog).OpenDialog();
        else if (_ParentDialog is PauseMenuDialog)
            ((PauseMenuDialog)_ParentDialog).OpenDialog();
        else
            throw new System.Exception("The specified parent dialog is of an invalid type!");
    }

}
