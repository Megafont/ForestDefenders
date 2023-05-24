using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MainMenuDialog : Dialog_Base, IDialog
{
    [Tooltip("This much time in seconds must elapse before the menu will respond to another input event to keep it from moving too fast.")]

    [SerializeField] GameObject _TitleDisplayCanvas;
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

    protected override void Dialog_OnUpdate()
    {        
        if (Time.unscaledTime - _LastGamepadSelectionChangeTime >= _GameManager.GamepadMenuSelectionDelay)
        {
            // If the mouse has caused the selection to be lost by clicking not on a button, then reselect the currently selected button according to this class's stored index.
            if (EventSystem.current.currentSelectedGameObject == null)
                SelectMenuItem();

            //Debug.Log("Selected: " + EventSystem.current.currentSelectedGameObject.name);

            float y = _InputManager_UI.Navigate.y;
            if (y < -0.5f) // User is pressing down
            {
                // Skip the next item if it is disabled.
                while (true)
                {
                    _SelectedMenuItemIndex++;

                    if (_SelectedMenuItemIndex >= _MenuItems.childCount)
                        _SelectedMenuItemIndex = 0;

                    SelectMenuItem();

                    Button selected = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
                    if (selected != null && selected.IsInteractable())
                        break;
                }

                _LastGamepadSelectionChangeTime = Time.unscaledTime;
            }
            else if (y > 0.5f) // User is pressing up
            {
                // Skip the next item if it is disabled.
                while (true)
                {
                    _SelectedMenuItemIndex--;

                    if (_SelectedMenuItemIndex < 0)
                        _SelectedMenuItemIndex = _MenuItems.childCount - 1;

                    SelectMenuItem();

                    Button selected = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
                    if (selected != null && selected.IsInteractable())
                        break;

                }

                _LastGamepadSelectionChangeTime = Time.unscaledTime;
            }


        }

    }

    protected override void Dialog_OnConfirm()
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
    
    public void OnStartGame()
    {
        _SceneSwitcher.FadeToScene("Level");
    }

    public void OnHighScores()
    {
        _TitleDisplayCanvas.SetActive(false);
        CloseDialog();

        _HighScoresDialog.OpenDialog();
    }

    public void OnExitGame()
    {
        Application.Quit();
    }

    public override void OpenDialog(bool closeOtherOpenDialogs = true)
    {
        if (_TitleDisplayCanvas)
            _TitleDisplayCanvas.SetActive(true);

        // Select the first menu item.
        EventSystem.current.SetSelectedGameObject(_MenuItems.GetChild(0).gameObject);

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
