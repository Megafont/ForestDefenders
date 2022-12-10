using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;


public class TechTreeDialog : MonoBehaviour
{
    [SerializeField] private GameObject _TileGroupHeaderPrefab;
    [SerializeField] private GameObject _TileGroupPrefab;
    [SerializeField] private GameObject _TilePrefab;

    [SerializeField] private string _LockedTileDescriptionText = "???";

    [SerializeField] private Color32 _HighlightColor = new Color32(220, 150, 0, 200);
    [SerializeField] private Color32 _SelectedColor = new Color32(180, 110, 0, 200);
    [SerializeField] private Color32 _LockedColor = new Color32(64, 64, 64, 200);
    [SerializeField] private Color32 _UnlockedColor = new Color32(0, 0, 0, 200);
    [SerializeField] private Color32 _ResearchedColor = new Color32(0, 64, 0, 200);

    [SerializeField] private Color32 _TileNormalXPCostTextColor = Color.white;
    [SerializeField] private Color32 _TileNotEnoughXPTextColor = Color.red;



    private struct TechTreeTileGroup
    {
        public string CategoryName;
        public List<TechTreeTile> TechTiles;
    }


    private InputManager _InputManager;

    private TMP_Text _AvailableXPText;
    private TMP_Text _BottomBarText;

    private GameObject _ScrollViewContentObject;
    private List<TechTreeTileGroup> _TechTileGroups;

    private Dictionary<string, TechTreeTile> _TechTreeTilesLookup;



    // Start is called before the first frame update
    void Start()
    {
        _InputManager = GameManager.Instance.InputManager;

        _AvailableXPText = transform.Find("Panel/XP Count Text (TMP)").GetComponent<TMP_Text>();

        _BottomBarText = transform.Find("Panel/Bottom Text Bar/Bottom Text Bar Text (TMP)").GetComponent<TMP_Text>();
        _BottomBarText.text = _LockedTileDescriptionText;

        _ScrollViewContentObject = transform.Find("Panel/Scroll View/Viewport/Content").gameObject;

        _TechTreeTilesLookup = new Dictionary<string, TechTreeTile>();


        TechDefinitions.GetTechDefinitions(this);

        CloseDialog();

    }

    void OnEnable()
    {
        if (_InputManager == null)
            return;

        _InputManager.GetPlayerInputComponent().actions.FindActionMap(InputManager.ACTION_MAP_PLAYER).Disable();
        _InputManager.GetPlayerInputComponent().actions.FindActionMap(InputManager.ACTION_MAP_UI).Enable();

        UpdateXPCountText();
        RefreshTileUIColors(null);

        IsOpen = true;
    }

    void OnGUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void OnDoneClick()
    {
        CloseDialog();
    }

    public void CloseDialog()
    {
        _InputManager.GetPlayerInputComponent().actions.FindActionMap(InputManager.ACTION_MAP_PLAYER).Enable();
        _InputManager.GetPlayerInputComponent().actions.FindActionMap(InputManager.ACTION_MAP_UI).Disable();

        gameObject.SetActive(false);
        IsOpen = false;
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
            throw new Exception("The amount of XP added must be positive!");

        AvailableXP += amount;
    }

    private void OnTileClicked(TechTreeTile sender)
    {
        // Debug.Log($"Tile \"{sender.TileData.Title}\" clicked!");

        if (sender.TileData.IsResearched)
            return;


        if (!sender.TileData.IsLocked &&
            AvailableXP >= sender.TileData.XPCost)
        {
            AvailableXP -= sender.TileData.XPCost;
            UpdateXPCountText();

            sender.SetResearchedFlag(true);

            RefreshTileUIColors(sender);

            EnableTechs.EnableTech(sender.TileData.Title);
        }

    }

    private void OnTileMouseEnter(TechTreeTile sender)
    {
        _BottomBarText.text = sender.TileData.IsLocked ? _LockedTileDescriptionText : sender.TileData.DescriptionText;
    }

    private void RefreshTileUIColors(TechTreeTile curTile)
    { 
        foreach (KeyValuePair<string, TechTreeTile> pair in _TechTreeTilesLookup)
        {
            if (pair.Value != curTile)
                pair.Value.UpdateUIColors();
        }
    }

    public void AddResearchGroup(string headerText, List<TechTreeTileData> techTiles)
    {
        if (techTiles == null)
            throw new Exception("The passed in TileData list is null!");
        if (techTiles.Count == 0)
            throw new Exception("The passed in TileData list is empty!");


        TMP_Text header = Instantiate(_TileGroupHeaderPrefab).GetComponent<TMP_Text>();
        
        // We set the parent here, because by default the engine keeps the prefab's original world position.
        // So we have to set the parent this way, rather than just passing it into the Instantiate() call above.
        // This false parameter tells it not to keep the original world position.
        header.transform.SetParent(_ScrollViewContentObject.transform);
        header.text = headerText;


        CreateTechTileGroupObject(techTiles);
    }

    private void CreateTechTileGroupObject(List<TechTreeTileData> techTilesData) 
    {
        GameObject groupObj = Instantiate(_TileGroupPrefab);

        // We set the parent here, because by default the engine keeps the prefab's original world position.
        // So we have to set the parent this way, rather than just passing it into the Instantiate() call above.
        // This false parameter tells it not to keep the original world position.
        groupObj.transform.SetParent(_ScrollViewContentObject.transform);


        //Debug.Log($"Tile Count: {techTilesData.Count}");
        for (int i = 0; i < techTilesData.Count; i++)
        {
            TechTreeTile newTile = Instantiate(_TilePrefab).GetComponent<TechTreeTile>();

            newTile.transform.SetParent(groupObj.transform);

            TechTreeTileData tileData = techTilesData[i];            
            tileData.IsLocked = i > 0; // Set all tiles after the first one in the group to be locked.            
            tileData.IsResearched = false;

            newTile.SetTileData(this, tileData);
            newTile.OnClick += OnTileClicked;
            newTile.OnMouseOver += OnTileMouseEnter;


            // Add this research tile to the lookup table.
            _TechTreeTilesLookup.Add(tileData.Title, newTile);

        } // end foreach tileData

    }

    private void UpdateXPCountText()
    {
        _AvailableXPText.text = $"Available XP: {AvailableXP}";
    }
      


    private void DEBUG_GenerateTestTileGroups()
    {
        List<TechTreeTileData> tilesData = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { Title = "T1", DescriptionText = "D1", XPCost = 3 },
            new TechTreeTileData() { Title = "T2", DescriptionText = "D2", XPCost = 3 },
            new TechTreeTileData() { Title = "T3", DescriptionText = "D3", XPCost = 3 },
            new TechTreeTileData() { Title = "T4", DescriptionText = "D4", XPCost = 3 },
            new TechTreeTileData() { Title = "T5", DescriptionText = "D5", XPCost = 3 },
            new TechTreeTileData() { Title = "T6", DescriptionText = "D6", XPCost = 3 },
            new TechTreeTileData() { Title = "T7", DescriptionText = "D7", XPCost = 3 },
            new TechTreeTileData() { Title = "T8", DescriptionText = "D8", XPCost = 3 },
        };

        List<TechTreeTileData> tilesData2 = new List<TechTreeTileData>()
        {
            new TechTreeTileData() { Title = "U1", DescriptionText = "E1", XPCost = 6 },
            new TechTreeTileData() { Title = "U2", DescriptionText = "E2", XPCost = 6 },
            new TechTreeTileData() { Title = "U3", DescriptionText = "E3", XPCost = 6 },
            new TechTreeTileData() { Title = "U4", DescriptionText = "E4", XPCost = 6 },
            new TechTreeTileData() { Title = "U5", DescriptionText = "E5", XPCost = 6 },
            new TechTreeTileData() { Title = "U6", DescriptionText = "E6", XPCost = 6 },
            new TechTreeTileData() { Title = "U7", DescriptionText = "E7", XPCost = 6 },
            new TechTreeTileData() { Title = "U8", DescriptionText = "E8", XPCost = 6 },
        };


        AddResearchGroup("Test Group", tilesData);
        AddResearchGroup("Test Group 2", tilesData2);
    }



    public int AvailableXP { get; private set; }
    public bool IsOpen { get; private set; }


    public string LockedTileDescriptionText { get { return _LockedTileDescriptionText; } }

    public Color32 HighlightColor { get { return _HighlightColor; } }
    public Color32 SelectedColor { get { return _SelectedColor; } }
    public Color32 ResearchedColor { get { return _ResearchedColor; } }
    public Color32 LockedColor { get { return _LockedColor; } }
    public Color32 UnlockedColor { get { return _UnlockedColor; } }

    public Color32 TileXPTextColor { get { return _TileNormalXPCostTextColor; } }
    public Color32 TileNotEnoughXPTextColor { get { return _TileNotEnoughXPTextColor; } }

}
