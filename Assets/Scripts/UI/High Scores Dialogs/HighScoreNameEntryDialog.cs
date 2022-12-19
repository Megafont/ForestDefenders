using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HighScoreNameEntryDialog : Dialog_Base, IDialog
{
    [SerializeField] TMP_InputField _InputField;

    [SerializeField] TMP_Text _ScoreText;
    [SerializeField] TMP_Text _SurvivalTimeText;



    public override void OpenDialog(bool closeOtherOpenDialogs = true)
    {
        _ScoreText.text = GameManager.Instance.Score.ToString("n0");
        _SurvivalTimeText.text = HighScores.TimeValueToString(GameManager.Instance.SurvivalTime);


        base.OpenDialog(closeOtherOpenDialogs);


        // Set the focus to the input field so the user doesn't have to click on it first to start typing.
        _InputField.ActivateInputField();
    }

    public void OnDoneClick()
    {
        if (string.IsNullOrWhiteSpace(_InputField.text))
            return;


        HighScores.RegisterHighScore(_InputField.text, 
                                     GameManager.Instance.Score,
                                     GameManager.Instance.SurvivalTime);

        GameManager.Instance.SceneSwitcher.FadeToScene("Main Menu");

        CloseDialog();

    }


}
