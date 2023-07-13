using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MainMenuDialog : Dialog_Base, IDialog
{
    [Tooltip("This much time in seconds must elapse before the menu will respond to another input event to keep it from moving too fast.")]

    [SerializeField] GameObject _TitleDisplayCanvas;
    [SerializeField] CharacterSelectionDialog _CharacterSelectionDialog;
    [SerializeField] HighScoresDialog _HighScoresDialog;


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

        OpenDialog();
    }

    protected override void Dialog_OnEnable()
    {
        if (_TitleDisplayCanvas)
            _TitleDisplayCanvas.SetActive(true);
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
        // We do nothing here, as the player should not be able to cancel out of the main menu.
    }

    public void OnMouseEnterMenuItem(GameObject sender)
    {
        _SelectedMenuItemIndex = GetIndexOfMenuItem(sender.transform);

        SelectMenuItem();
    }
    
    public void OnSelectedStartGame()
    {
        _TitleDisplayCanvas.SetActive(false);
        CloseDialog();
        
        _CharacterSelectionDialog.OpenDialog();
    }

    public void OnSelectedHowToPlay()
    {

    }

    public void OnSelectedControls()
    {

    }

    public void OnSelectedHighScores()
    {
        _TitleDisplayCanvas.SetActive(false);
        CloseDialog();

        _HighScoresDialog.OpenDialog();
    }

    public void OnSelectedExitGame()
    {
        Application.Quit();
    }

    public override void OpenDialog(bool closeOtherOpenDialogs = true)
    {
        if (_TitleDisplayCanvas)
            _TitleDisplayCanvas.SetActive(true);

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
