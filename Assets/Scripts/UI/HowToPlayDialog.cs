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


    protected override void Dialog_OnAwake()
    {

    }

    protected override void Dialog_OnStart()
    {

    }

    public void OnDoneClicked()
    {
        ReturnToMainMenu();
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
