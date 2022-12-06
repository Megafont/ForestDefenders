using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HighScoreNameEntryDialog : MonoBehaviour
{
    [SerializeField] TMP_InputField _InputField;

    [SerializeField] TMP_Text _ScoreText;
    [SerializeField] TMP_Text _SurvivalTimeText;

    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (_ScoreText == null)
            return;

        _ScoreText.text = GameManager.Instance.Score.ToString();
        _SurvivalTimeText.text = HighScores.TimeValueToString(GameManager.Instance.SurvivalTime);

        // Set the focus to the input field so the user doesn't have to click on it first to start typing.
        _InputField.ActivateInputField();
    }

    void OnGUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnDoneClick()
    {
        if (string.IsNullOrWhiteSpace(_InputField.text))
            return;


        HighScores.RegisterHighScore(_InputField.text, 
                                     GameManager.Instance.Score,
                                     GameManager.Instance.SurvivalTime);

        gameObject.SetActive(false);

        GameManager.Instance.SceneSwitcher.FadeToScene("Main Menu");
    }


}
