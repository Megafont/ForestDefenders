using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using TMPro;


public class RadialMenuDialog : Dialog_Base, IDialog
{
    [SerializeField] private GameObject _RadialMenuItemPrefab;

    [Min(1)]
    [SerializeField] private float _Radius = 150;



    public delegate void RadialMenuEventHandler(GameObject sender);
    public event RadialMenuEventHandler OnSelectionChanged;



    private List<RadialMenuItem> _MenuItems;
    private int _ActiveMenuItemsCount;

    private bool _IsInitializing;
    private float _MenuItemSizeInDegrees;
    private int _PrevSelectedItemIndex;

    WaitForSecondsRealtime _MenuCloseDelay = new WaitForSecondsRealtime(0.2f);

    private GameObject _RadialMenuPanel;

    private TMP_Text _MenuTitleUI;
    private TMP_Text _MenuBottomBarUI;
    private Color32 _MenuBottomBarTextColor = new Color32(255, 160, 0, 0);
    private Color32 _MenuTitleTextColor = new Color32(255, 160, 0, 0);

    private GameObject _RadialMenuItemsParent;



    protected override void Dialog_OnAwake()
    {
        _MenuTitleUI = GameObject.Find("Radial Menu Dialog/Panel/Title Bar/Title Text (TMP)").GetComponent<TMP_Text>();
        _MenuBottomBarUI = GameObject.Find("Radial Menu Dialog/Panel/Bottom Text Bar/Bottom Text Bar (TMP)").GetComponent<TMP_Text>();
        _RadialMenuPanel = GameObject.Find("Radial Menu Dialog/Panel");
        _RadialMenuItemsParent = GameObject.Find("Radial Menu Dialog/Panel/Menu Items Parent");
    }

    protected override void Dialog_OnStart()
    {
        _MenuItems = new List<RadialMenuItem>();

        // Create one menu item so the list isn't empty (to prevent errors).
        CreateMenuItem();

    }



    public void SetMenuParams(string title, string[] names, int defaultItemIndex = 0)
    {
        if (names.Length == 0)
            throw new Exception("The passed in menu items list is empty!");
        if (defaultItemIndex < 0)
            throw new Exception("The passed in default menu item index must be positive!");
        if (defaultItemIndex >= names.Length) // This is >= instead of > to take into account the "Cancel" item that is added to the end of the menu automatically.
            throw new Exception("The passed in default menu item index must not be larger than the number of menu items!");

        if (_RadialMenuPanel == null)
            throw new Exception("The radial menu panel is null!");


        if (_IsInitializing || IsOpen())
        {
            Debug.LogError("Can't show radial menu because one is already open!");
            return;
        }


        _IsInitializing = true;

        MenuConfirmed = false;
        MenuCancelled = false;

        _MenuTitleUI.text = title;

        InitMenuItemsVisualElements(names, defaultItemIndex);

        _RadialMenuPanel.SetActive(true);
        SelectedMenuItemIndex = 0;

        _IsInitializing = false;
    }

    public override void OpenDialog(bool closeOtherOpenDialogs = false)
    {
        if (IsOpen())
            throw new Exception("Cannot display the radial menu, because one is already displayed!");


        base.OpenDialog(closeOtherOpenDialogs);

        if (IsOpen())
            StartCoroutine(DoRadialMenu());
    }

    private IEnumerator DoRadialMenu()
    {
        if (_MenuItems == null)
            throw new Exception("The menu items list is null!");
        else if (_MenuItems.Count == 0)
            throw new Exception("The menu items list is empty!");


        // Enable the UI controls InputActionMap.
        _InputManager.GetPlayerInputComponent().actions.FindActionMap(InputManager.ACTION_MAP_UI).Enable();



        while (true)
        {
            RefreshUI();

            //Debug.Log($"Confirm: {MenuConfirmed}    Cancel: {MenuCancelled}");
            if (MenuConfirmed || MenuCancelled)
                break;

            yield return null; // Wait one frame.
        }


        /*
        if (MenuConfirmed)
            Debug.Log($"Radial Menu \"{_MenuTitleUI.text}\": Selected Item = \"{SelectedItemName}\"");
        else if (MenuCancelled)
            Debug.Log($"Radial Menu  \"{_MenuTitleUI.text}\": Selection cancelled.");
        */


        // Wait slightly to stop the player character from receiving the same button press that closed the menu, which will make him jump or attack upon closing the menu.
        yield return _MenuCloseDelay;

        // Disable the UI controls InputActionMap.
        CloseDialog();
    }

    private void RefreshUI()
    {
        if (_IsInitializing)
            return;


        GetSelectedItemIndexFromGamepadInput();
    }


    public void OnMouseClickItem(GameObject sender)
    {
        Dialog_OnConfirm();
    }

    public void OnMouseEnterMenuItem(GameObject sender)
    {
        if (!sender)
            return;


        int index = _MenuItems.IndexOf(sender.GetComponent<RadialMenuItem>());
        SelectItem(index);
    }

    protected override void Dialog_OnConfirm()
    {
        if (!MenuCancelled)
        {
            if (SelectedMenuItemName != "Cancel")
                MenuConfirmed = true;
            else
                MenuCancelled = true;
        }
    }

    protected override void Dialog_OnCancel()
    {
        if (!MenuConfirmed)
        {
            MenuCancelled = true;
        }
    }

    private void GetSelectedItemIndexFromGamepadInput()
    {
        Vector2 dir = _InputManager.UI.Navigate;
        dir.y *= -1;


        // Filter out inputs that are zero. Otherwise it will reselect the top item
        // in the menu as soon as you let go of the joystick.
        if (dir == Vector2.zero)
            return;


        float angle = Vector3.Angle(Vector3.down, dir);

        if (dir.x < 0)
            angle = 360 - angle;


        int index = (int)Mathf.Round((float)(angle / _MenuItemSizeInDegrees));

        // If the index is equal to the number of items, it means angle was 360, which is the same as 0. So reset index to 0 so it points to the first item as it should.
        if (index >= _ActiveMenuItemsCount)
            index = 0;


        /*
        if (SelectedIndex < 0)
            Debug.Log($"Angle: {angle}    Index: {index}    Item Menu Slice Size: {_MenuItemSizeInDegrees}    Selected: NONE");
        else
            Debug.Log($"Angle: {angle}    Index: {index}    Item Menu Slice Size: {_MenuItemSizeInDegrees}    Selected: {_MenuItems[SelectedIndex].Name}");
        */


        if (index != SelectedMenuItemIndex)
            SelectItem(index);
    }

    private void SelectItem(int index)
    {
        // If the menu item is already selected, then return so we don't set _PrevSelectedItemIndex below.
        if (index == SelectedMenuItemIndex)
            return;


        _PrevSelectedItemIndex = SelectedMenuItemIndex;

        SelectedMenuItemIndex = index;

        UpdateSelection();
    }

    private void UpdateSelection()
    {        
        if (SelectedMenuItemIndex >= 0)
            _MenuItems[SelectedMenuItemIndex].Highlight();

        if (_PrevSelectedItemIndex >= 0)
            _MenuItems[_PrevSelectedItemIndex].Unhighlight();


        OnSelectionChanged?.Invoke(gameObject);
    }


    private void InitMenuItemsVisualElements(string[] names, int defaultItemIndex)
    {
        _ActiveMenuItemsCount = names.Length + 1;


        // Calculate the size of the pie slice each menu item occupies in the radial menu.
        // The plus one takes into account the Cancel item that is added to the end of the menu automatically below.
        _MenuItemSizeInDegrees = 360 / _ActiveMenuItemsCount;

        Quaternion q = Quaternion.identity;
        float angle = 0;

        float menuItemVerticalOffset = CalculateMenuItemsVerticalOffset();

        // Get the number of items we need to iterate through (whichever one is higher is what we need).
        // The plus one takes into account the Cancel item that is added to the end of the menu automatically below.
        int length = Mathf.Max(_MenuItems.Count, _ActiveMenuItemsCount);


        // Iterate through the list of menu items.
        for (int i = 0; i < length; i++)
        {
            // If there isn't a next menu item, create it.
            if (i >= _MenuItems.Count)
                CreateMenuItem();

            // If the menu item is within the length of the passed in list of items, then set it up.
            // This is >= instead of > to take into account the "Cancel" menu item that is added automatically.
            if (i < _ActiveMenuItemsCount)
            {
                angle = _MenuItemSizeInDegrees * i;
                q.eulerAngles = new Vector3(0, 0, -angle); // We negate the angle so the menu items are placed around the center going clockwise rather than the inverse.
                Vector3 offset = q * (Vector3.up * _Radius);

                // Position the menu item.
                // The last part of this line in ()s shifts the menu items down by half the height of the title bar so they appear centered in the area beneath it.
                _MenuItems[i].transform.position = _RadialMenuPanel.transform.position + offset + new Vector3(0, menuItemVerticalOffset, 0);

                // Debug.Log($"Menu Pos: {RadialMenuPanel.transform.position}    Offset: {offset}    Menu Item Pos: {_MenuItems[i].UI.transform.position}");


                // Remove highlighting if this menu item is highlighted.
                _MenuItems[i].Unhighlight();

                // Set the name of the menu item.
                if (i < names.Length)
                    _MenuItems[i].Name = names[i];
                else if (i == names.Length)
                    _MenuItems[i].Name = "Cancel";


                // If this is the first menu item, set it as the default.
                if (i == defaultItemIndex)
                {
                    _MenuItems[i].IsDefaultMenuItem = true;
                    _MenuItems[i].Highlight();
                }
                else
                {
                    _MenuItems[i].IsDefaultMenuItem = false;
                }


                // Make sure this menu item's UI is enabled.
                _MenuItems[i].Show(true);
            }
            else
            {
                _MenuItems[i].Name = "Unused";

                // This is an extra menu item that won't be used this time around, so disable it.
                _MenuItems[i].Show(false);
            }

        } // end for i

    }

    /// <summary>
    /// Offsets the menu items vertically so they are centered between the title bar and the bottom bar.
    /// </summary>
    /// <returns></returns>
    private float CalculateMenuItemsVerticalOffset()
    {
        float titleBarHeight = _MenuTitleUI.rectTransform.rect.height;
        float bottomBarHeight = _MenuBottomBarUI.rectTransform.rect.height;

        return ((-titleBarHeight) + (bottomBarHeight)) / 2;
    }

    private void CreateMenuItem()
    {
        GameObject newMenuItem = Instantiate(_RadialMenuItemPrefab,
                                             Vector3.zero,
                                             Quaternion.identity,
                                             _RadialMenuItemsParent.transform);


        RadialMenuItem item = newMenuItem.GetComponent<RadialMenuItem>();

        item.OnMouseClick += OnMouseClickItem;
        item.OnMouseEnter += OnMouseEnterMenuItem;
        
        _MenuItems.Add(item);

    }



    public bool MenuConfirmed { get; private set; }
    public bool MenuCancelled { get; private set; }

    public Color32 MenuItemTextColor
    {
        get { return _MenuItems[0].TextNormalColor; }
        set
        {
            foreach (RadialMenuItem item in _MenuItems)
                item.TextNormalColor = value;
        }
    }

    public Color32 MenuItemHighlightTextColor
    {
        get { return _MenuItems[0].TextHighlightColor; }
        set
        {
            foreach (RadialMenuItem item in _MenuItems)
                item.TextHighlightColor = value;

            _MenuItems[SelectedMenuItemIndex].TextHighlightColor = value;
        }
    }
    
    public int SelectedMenuItemIndex { get; private set; }
    public string SelectedMenuItemName { get { return _MenuItems[SelectedMenuItemIndex].Name; } }

    public string BottomBarText
    {
        get { return _MenuBottomBarUI.text; }
        set { _MenuBottomBarUI.text = value; }        
    }

    public Color BottomBarTextColor
    {
        get
        {
            return _MenuBottomBarTextColor;
        }
        set
        {
            if (_MenuBottomBarUI)
            {
                _MenuBottomBarTextColor = value;
                _MenuBottomBarUI.color = _MenuBottomBarTextColor;
            }
        }
    }

    public Color TitleTextColor
    {
        get
        {
            return _MenuTitleTextColor;
        }
        set
        {
            if (_MenuTitleUI)
            {
                _MenuTitleTextColor = value;
                _MenuTitleUI.color = _MenuTitleTextColor;
            }
        }
    }


}
