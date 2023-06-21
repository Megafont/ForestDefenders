using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;



public class CharacterSelectionDialog : Dialog_Base, IDialog
{
    public static float ImageSizeIncreaseOnSelection = 100;
    public static Color32 HightlightedColor = Color.white;
    public static Color32 UnhighlightedColor = new Color32(100, 100, 100, 255);



    [Tooltip("This much time in seconds must elapse before the menu will respond to another input event to keep it from moving too fast.")]

    [SerializeField] private MainMenuDialog _MainMenuDialog;


    private SceneSwitcher _SceneSwitcher;
    private Transform _MenuItems;

    private List<Image> _MenuItemImages;
    private TMP_Text _SelectionText;

    private float _LastGamepadSelectionChangeTime;
    private int _SelectedMenuItemIndex;
    private int _PrevSelectedMenuItemIndex;

    private Vector2 _DefaultImageSize;



    protected override void Dialog_OnAwake()
    {
        _MenuItems = transform.Find("Panel/Menu Items").transform;

        _MenuItemImages = new List<Image>();

        _SelectionText = transform.Find("Panel/Selection Text").GetComponent<TMP_Text>();
    }

    protected override void Dialog_OnStart()
    {
        _SceneSwitcher = _GameManager.SceneSwitcher;


        _DefaultImageSize = _MenuItems.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta;

        for (int i = 0; i < _MenuItems.childCount; i++)
        {
            Transform child = _MenuItems.GetChild(i);
            child.GetComponent<MenuDialogsMenuItem>().OnMouseEnter += OnMouseEnterMenuItem;

            _MenuItemImages.Add(child.GetComponent<Image>());
            
            Unhighlight(i);
        }


        // Select the first character.
        Highlight(0);

    }

    protected override void Dialog_OnEnable()
    {

    }

    protected override void Dialog_OnUpdate()
    {        
        if (Time.unscaledTime - _LastGamepadSelectionChangeTime >= _GameManager.GamepadMenuSelectionDelay)
        {
            // If the mouse has caused the selection to be lost by clicking not on a button, then reselect the currently selected button according to this class's stored index.
            if (EventSystem.current.currentSelectedGameObject == null)
                SelectMenuItem();

            //Debug.Log("Selected: " + EventSystem.current.currentSelectedGameObject.name);

            float x = _InputManager_UI.Navigate.x;
            if (x < -0.5f) // User is pressing left
            {                
                while (true)
                {
                    _PrevSelectedMenuItemIndex = _SelectedMenuItemIndex;
                    _SelectedMenuItemIndex--;

                    if (_SelectedMenuItemIndex < 0)
                        _SelectedMenuItemIndex = _MenuItems.childCount - 1;

                    SelectMenuItem();

                    // Skip the next item if it is disabled.
                    Button selected = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
                    if (selected != null && selected.IsInteractable())
                        break;
                }

                _LastGamepadSelectionChangeTime = Time.unscaledTime;
            }
            else if (x > 0.5f) // User is pressing right
            {
                while (true)
                {
                    _PrevSelectedMenuItemIndex = _SelectedMenuItemIndex;
                    _SelectedMenuItemIndex++;

                    if (_SelectedMenuItemIndex >= _MenuItems.childCount)
                        _SelectedMenuItemIndex = 0;

                    SelectMenuItem();

                    // Skip the next item if it is disabled.
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
            Button pressedBtn = _MenuItemImages[_SelectedMenuItemIndex].GetComponent<Button>();

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            pressedBtn.OnPointerClick(eventData);

            _LastGamepadSelectionChangeTime = Time.unscaledTime;
        }
    }

    protected override void Dialog_OnCancel()
    {
        OnReturnToMainMenuConfirmed();
    }

    public void OnMouseEnterMenuItem(GameObject sender)
    {
        _PrevSelectedMenuItemIndex = _SelectedMenuItemIndex;

        _SelectedMenuItemIndex = GetIndexOfMenuItem(sender.transform);

        SelectMenuItem();
    }
    
    public void OnBoyConfirmed()
    {
        GameManager._SpawnBoy = true;

        _SceneSwitcher.FadeToScene("Level 01");

        CloseDialog();
    }

    public void OnGirlConfirmed()
    {
        GameManager._SpawnBoy = false;

        _SceneSwitcher.FadeToScene("Level 01");

        CloseDialog();
    }

    public void OnReturnToMainMenuConfirmed()
    {
        CloseDialog();

        _MainMenuDialog.OpenDialog();
    }


    public override void OpenDialog(bool closeOtherOpenDialogs = true)
    {
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
        if (_MenuItemImages == null || _SelectionText == null)
            return;


        Unhighlight(_PrevSelectedMenuItemIndex);

        Highlight(_SelectedMenuItemIndex);
        _SelectionText.text = _MenuItemImages[_SelectedMenuItemIndex].gameObject.name;


        // Tell the Unity EventSystem to select the correct button.
        EventSystem.current.SetSelectedGameObject(_MenuItems.GetChild(_SelectedMenuItemIndex).gameObject);
    }

    private void Unhighlight(int index)
    {
        _MenuItemImages[index].rectTransform.sizeDelta = _DefaultImageSize;
        _MenuItemImages[index].color = UnhighlightedColor;
    }

    private void Highlight(int index)
    {
        _MenuItemImages[index].rectTransform.sizeDelta = _DefaultImageSize + new Vector2(ImageSizeIncreaseOnSelection, ImageSizeIncreaseOnSelection);
        _MenuItemImages[index].color = HightlightedColor;
    }

}
