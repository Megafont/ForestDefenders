using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameOverDialog : Dialog_Base, IDialog
{
    [SerializeField] private TMP_Text _InputFieldLabel;
    [SerializeField] private TMP_InputField _InputField;

    [SerializeField] private TMP_Text _TitleText;
    [SerializeField] private TMP_Text _MaxComboStreakText;
    [SerializeField] private TMP_Text _ScoreText;
    [SerializeField] private TMP_Text _SurvivalTimeText;

    private bool IsRecordScore;



    protected override void Dialog_OnAwake()
    {
        _IgnoreGamePhaseText = true;
    }

    public override void OpenDialog(bool closeOtherOpenDialogs = true)
    {
        IsRecordScore = HighScores.IsNewHighScore(_GameManager.Score, _GameManager.SurvivalTime);

        _TitleText.text = IsRecordScore ? "New Record Score!" : "Your Village Has Fallen!";
        _InputFieldLabel.text = IsRecordScore ? "Enter your name:" : "Regroup and try to hold out longer next time!";

        _InputField.gameObject.SetActive(IsRecordScore);


        _MaxComboStreakText.text = _GameManager.MonsterManager.MaxComboStreak.ToString("n0");
        _ScoreText.text = GameManager.Instance.Score.ToString("n0");
        _SurvivalTimeText.text = HighScores.TimeValueToString(GameManager.Instance.SurvivalTime);


        base.OpenDialog(closeOtherOpenDialogs);


        // Set the focus to the input field so the user doesn't have to click on it first to start typing.
        _InputField.ActivateInputField();
    }

    public void OnDoneClick()
    {
        if (IsRecordScore)
        {
            if (string.IsNullOrWhiteSpace(_InputField.text))
                return;

            HighScores.RegisterHighScore(_InputField.text,
                                         GameManager.Instance.Score,
                                         GameManager.Instance.SurvivalTime);
        }


        GameManager.Instance.SceneSwitcher.FadeToScene("Main Menu");

        CloseDialog();

    }


}
