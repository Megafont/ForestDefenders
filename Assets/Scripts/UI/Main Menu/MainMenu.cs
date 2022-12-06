using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    [Tooltip("This much time in seconds must elapse before the menu will respond to another input event to keep it from moving too fast.")]
    public float GamepadMenuSelectionDelay = 0.1f;

    [SerializeField] GameObject _HighScoresDialog;


    private GameManager _GameManager;
    private InputManager_UI _InputManager_UI;
    private SceneSwitcher _SceneSwitcher;
    private Transform _MenuItems;


    private float _LastGamepadSelectionChange;
    private int _SelectedMenuItemIndex;


    private void Awake()
    {
        _GameManager = GameManager.Instance;

        _MenuItems = transform.Find("Menu Items").transform;   
    }

    // Start is called before the first frame update
    void Start()
    {
        _InputManager_UI = _GameManager.InputManager.UI;

        _GameManager.InputManager.SwitchToActionMap((int) InputActionMapIDs.UI);
        _SceneSwitcher = _GameManager.SceneSwitcher;

        foreach (Transform t in _MenuItems)
            t.GetComponent<MainMenuItem>().OnMouseUp += OnButtonClicked;

    }

    // Update is called once per frame
    void Update()
    {        
        if (Time.time - _LastGamepadSelectionChange >= GamepadMenuSelectionDelay)
        {
            // If the mouse has caused the selection to be lost by clicking not on a button, then reselect the currently selected button according to this class's stored index.
            if (EventSystem.current.currentSelectedGameObject == null)
                SelectMenuItem();

            //Debug.Log("Selected: " + EventSystem.current.currentSelectedGameObject.name);

            float y = _InputManager_UI.Navigate.y;
            if (y < -0.5f) // User is pressing up
            {
                _SelectedMenuItemIndex++;

                if (_SelectedMenuItemIndex >= _MenuItems.childCount)
                    _SelectedMenuItemIndex = 0;

                SelectMenuItem();

                _LastGamepadSelectionChange = Time.time;
            }
            else if (y > 0.5f) // User is pressing down
            {
                _SelectedMenuItemIndex--;

                if (_SelectedMenuItemIndex < 0)
                    _SelectedMenuItemIndex = _MenuItems.childCount - 1;

                SelectMenuItem();

                _LastGamepadSelectionChange = Time.time;
            }



            if (_InputManager_UI.Confirm && !_SceneSwitcher.IsTransitioningToScene)
            {
                Button pressedBtn = _MenuItems.GetChild(_SelectedMenuItemIndex).GetComponent<Button>();

                PointerEventData eventData = new PointerEventData(EventSystem.current);
                pressedBtn.OnPointerClick(eventData);

                _LastGamepadSelectionChange = Time.time;
            }

        }

    }

    void OnGUI()
    {        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void OnButtonClicked(GameObject sender)
    {
        _SelectedMenuItemIndex = GetIndexOfMenuItem(sender.transform);
    }
    
    public void OnStartGame()
    {
        Debug.LogWarning("Main menu button \"Start\" goes to Test scene. Don't forget to change it to the actual in-game scene!");
        _SceneSwitcher.FadeToScene("Test");
    }

    public void OnHighScores()
    {
        _HighScoresDialog.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnExitGame()
    {
        Application.Quit();
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
