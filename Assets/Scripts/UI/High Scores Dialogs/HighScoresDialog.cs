using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class HighScoresDialog : Dialog_Base, IDialog
{
    [SerializeField] private MainMenuDialog _MainMenu;    
    [SerializeField] private GameObject _ScrollViewContentArea;
    [SerializeField] private GameObject _ScoreTableRowPrefab;

    [SerializeField] private TMP_Text _BestScoresButtonText;
    [SerializeField] private TMP_Text _BestTimesButtonText;
    [SerializeField] private TMP_Text _DoneButtonText;

    [SerializeField] private Color32 _SelectedTableColor = new Color32(255, 160, 0, 255);
    [SerializeField] private Color32 _DeselectedTablecolor = new Color32(128, 128, 128, 255);


    private HighScoreTypes _SelectedTableType = HighScoreTypes.Score;

    private List<HighScoresTableRow> _HighScoresTableUIs;

    private List<HighScoreData> _HighScoresTable;
    private List<HighScoreData> _HighTimesTable;

    private ScrollRect _ScrollRect;



    protected override void Dialog_OnStart()
    {
        if (_ScrollViewContentArea == null)
            throw new Exception("The ScrollViewContentArea property has not been set in the inspector!");

        if (_ScoreTableRowPrefab == null)
            throw new Exception("The ScoreTableRowPrefab property has not been set in the inspector!");

        if (_BestScoresButtonText == null)
            throw new Exception("The BestScoresButtonText property has not been set in the inspector!");

        if (_BestTimesButtonText == null)
            throw new Exception("The BestTimesButtonText property has not been set in the inspector!");


        _ScrollRect = transform.GetComponentInChildren<ScrollRect>();
        if (_ScrollRect == null)
            Debug.LogError("The ScrollView's ScrollRect component was not found!");


        InitHighScoresTableRows();
        LoadHighScoresTables();
        

        _BestScoresButtonText.color = _SelectedTableColor;
        _BestTimesButtonText.color = _DeselectedTablecolor;
        _DoneButtonText.color = _DeselectedTablecolor;

        gameObject.SetActive(false);
    }

    protected override void Dialog_OnUpdate()
    {

    }

    private void InitHighScoresTableRows()
    {
        _HighScoresTableUIs = new List<HighScoresTableRow>();


        for (int i = 0; i < Utils_HighScores.MAX_HIGHSCORES_TABLE_SIZE; i++)
        {
            GameObject tableRow = Instantiate(_ScoreTableRowPrefab, 
                                              new Vector3(0, -50 + (-50 * i), 0), 
                                              Quaternion.identity);

            // We set the parent here, because by default the engine keeps the prefab's original world position.
            // So we have to set the parent this way, rather than just passing it into the Instantiate() call above.
            // This false parameter tells it not to keep the original world position.
            tableRow.transform.SetParent(_ScrollViewContentArea.transform, false);

            HighScoresTableRow tableRowComponent = tableRow.GetComponent<HighScoresTableRow>();

            _HighScoresTableUIs.Add(tableRowComponent);

        } // end for

    }

    private void LoadHighScoresTables()
    {        
        _HighScoresTable = Utils_HighScores.GetHighScoresTable(HighScoreTypes.Score);
        _HighTimesTable = Utils_HighScores.GetHighScoresTable(HighScoreTypes.SurvivalTime);
    }

    public override void OpenDialog(bool closeOtherOpenDialogs = true)
    {
        // Always start the dialog with the best scores table displayed.
        _SelectedTableType = HighScoreTypes.Score;
        DisplayScoreTable();

        base.OpenDialog(closeOtherOpenDialogs);
    }

    private void DisplayScoreTable()
    {
        if (_HighScoresTableUIs == null)
        {
            //Debug.LogWarning("High score table UIs list is null!");
            return;
        }


        LoadHighScoresTables();


        // Grab the selected table type.
        List<HighScoreData> highScoresTable = null;
        if (_SelectedTableType == HighScoreTypes.Score)
            highScoresTable = _HighScoresTable;
        else if (_SelectedTableType == HighScoreTypes.SurvivalTime)
            highScoresTable = _HighTimesTable;
        

        if (highScoresTable == null)
        {
            //Debug.LogWarning("High scores table is null!");
            return;
        }


        // Display the high scores table.
        for (int i = 0; i < Utils_HighScores.MAX_HIGHSCORES_TABLE_SIZE; i++)
        {
            _HighScoresTableUIs[i].SetRowData(highScoresTable[i], _SelectedTableType);
        } // end for i

    }

    public void OnBestScoresClick()
    {
        if (_SelectedTableType == HighScoreTypes.Score)
            return;


        _SelectedTableType = HighScoreTypes.Score;

        DisplayScoreTable();

        _BestScoresButtonText.color = _SelectedTableColor;
        _BestTimesButtonText.color = _DeselectedTablecolor;
    }

    public void OnBestTimesClick()
    {
        if (_SelectedTableType == HighScoreTypes.SurvivalTime)
            return;


        _SelectedTableType = HighScoreTypes.SurvivalTime;

        DisplayScoreTable();

        _BestTimesButtonText.color = _SelectedTableColor;
        _BestScoresButtonText.color = _DeselectedTablecolor;
    }

    public void OnDoneClick()
    {
        CloseDialog();

        _MainMenu.OpenDialog();
    }

    protected override void Dialog_OnNavigate()
    {
        // Check if the user is pressing up or down. If so, scrolls the high scores table.        
        float scrollMagnitude = _InputManager_UI.Navigate.y;
        if (scrollMagnitude != 0)
        {
            float scrollableHeight = _ScrollRect.content.sizeDelta.y - _ScrollRect.viewport.rect.height;
            float scrollAmount = DIALOG_SCROLL_SPEED * scrollMagnitude;// * Time.unscaledDeltaTime;
            _ScrollRect.verticalNormalizedPosition += scrollAmount / scrollableHeight;
        }


        // Check if the user is pressing left or right a significant amount. If so, switch tabs.
        if (_InputManager_UI.Navigate.x > 0.75f)
            OnBestTimesClick();
        else if (_InputManager_UI.Navigate.x < -0.75f)
            OnBestScoresClick();
    }

    protected override void Dialog_OnSubmit()
    {
        //Debug.Log($"OnConfirm called on {this.GetType()}.");

        OnDoneClick();
    }

    protected override void Dialog_OnCancel() 
    {
        OnDoneClick();
    }

}
