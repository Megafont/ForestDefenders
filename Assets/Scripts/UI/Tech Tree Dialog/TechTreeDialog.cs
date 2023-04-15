using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;


public class TechTreeDialog : Dialog_Base, IDialog
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


    private TMP_Text _AvailableXPText;
    private TMP_Text _BottomBarText;

    private GameObject _ScrollViewContentObject;

    private Dictionary<TechDefinitionIDs, TechTreeTile> _TechTreeTilesLookup;
    private List<TechTreeTileGroup> _TechTreeTileGroups;

    private TechTreeTile _LastTileHighlighted;
    private TechTreeTile _LastTileSelected;



    protected override void Dialog_OnAwake()
    {
        _PauseGameWhileDialogOpen = true;
    }

    protected override void Dialog_OnStart()
    {        
        _InputManager = GameManager.Instance.InputManager;
        _InputManager_UI = _InputManager.UI;

        _AvailableXPText = transform.Find("Panel/XP Count Text (TMP)").GetComponent<TMP_Text>();

        _BottomBarText = transform.Find("Panel/Bottom Text Bar/Bottom Text Bar Text (TMP)").GetComponent<TMP_Text>();
        _BottomBarText.text = _LockedTileDescriptionText;

        _ScrollViewContentObject = transform.Find("Panel/Scroll View/Viewport/Content").gameObject;

        _TechTreeTilesLookup = new Dictionary<TechDefinitionIDs, TechTreeTile>();
        _TechTreeTileGroups = new List<TechTreeTileGroup>();

        TechDefinitions.GetTechDefinitions(this);

        CloseDialog();

    }

    protected override void Dialog_OnUpdate()
    {
        if (_InputManager_UI != null &&
            _InputManager_UI.CloseTechTree)
        {
            CloseDialog();
        }    
    }

    public override void OpenDialog(bool closeOtherOpenDialogs = false)
    {
        UpdateXPCountText();
        RefreshTileUIColors(null);

        base.OpenDialog(closeOtherOpenDialogs);
    }

    public void OnDoneClick()
    {
        CloseDialog();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
            throw new Exception("The amount of XP added must be positive!");

        AvailableXP += amount;
    }

    public void AddResearchGroup(string headerText, List<TechTreeTileData> techTiles)
    {
        if (string.IsNullOrWhiteSpace(headerText))
            throw new Exception("The passed in header text is null, empty, or whitespace!");
        if (techTiles == null)
            throw new Exception("The passed in TileData list is null!");
        if (techTiles.Count == 0)
            throw new Exception("The passed in TileData list is empty!");
        if (GroupExists(headerText))
            throw new Exception($"A tech group named \"headerText\" has already been added into the tech tree!");


        TMP_Text header = Instantiate(_TileGroupHeaderPrefab).GetComponent<TMP_Text>();

        // We set the parent here, because by default the engine keeps the prefab's original world position.
        // So we have to set the parent this way, rather than just passing it into the Instantiate() call above.
        // This false parameter tells it not to keep the original world position.
        header.transform.SetParent(_ScrollViewContentObject.transform);
        header.text = headerText;


        CreateTechTileGroupObject(techTiles, headerText);
    }

    public bool IsTechnologyLocked(TechDefinitionIDs techID)
    {
        return _TechTreeTilesLookup[techID].TileData.IsAvailableToResearch;
    }

    public bool IsTechnologyResearched(TechDefinitionIDs techID)
    {
        return _TechTreeTilesLookup[techID].TileData.IsResearched;
    }

    private void OnTileClicked(TechTreeTile sender)
    {
        // Debug.Log($"Tile \"{sender.TileData.Title}\" clicked!");


        _LastTileSelected = sender;


        if (sender.TileData.IsResearched)
            return;


        if (sender.TileData.IsAvailableToResearch &&
            (AvailableXP >= sender.TileData.XPCost || _GameManager.ResearchIsFree))
        {
            AvailableXP -= sender.TileData.XPCost;
            UpdateXPCountText();

            sender.SetResearchedFlag(true);


            // Unlock the next tile in the row if there is one.
            Vector2Int tileIndices = sender.TileData.TileIndices;
            TechTreeTileGroup tileGroup = _TechTreeTileGroups[tileIndices.y];
            if (tileIndices.x < tileGroup.TechTiles.Count - 1)
            {
                TechTreeTile nextTile = tileGroup.TechTiles[tileIndices.x + 1];
                nextTile.SetIsAvailableToResearchFlag(true);
            }


            // Update the description text if necessary.
            UpdateDescriptionText(sender);


            _GameManager.AddToScore(sender.TileData.XPCost * _GameManager.PlayerResearchScoreMultiplier * _GameManager.MonsterManager.CurrentWaveNumber);


            // Enable the technology referenced by this tile.
            TechEnabler.EnableTech(sender.TileData.Title);
        }

    }

    private void OnTileMouseEnter(TechTreeTile sender)
    {
        UpdateDescriptionText(sender);

        _LastTileHighlighted = sender;
    }

    public void OnTileMouseExit(TechTreeTile sender)
    {
        // Since the mouse left this tile, display the description text for the selected tile if there is one.
        if (_LastTileSelected != null)
            UpdateDescriptionText(_LastTileSelected);
        else
            UpdateDescriptionText(null);      
    }

    private void UpdateDescriptionText(TechTreeTile techTile)
    {
        if (_BottomBarText == null || techTile == null)
            return;


        if (techTile == null || techTile.TileData.IsAvailableToResearch)
            _BottomBarText.text = techTile.TileData.DescriptionText; 
        else
            _BottomBarText.text = _LockedTileDescriptionText;
    }

    private void RefreshTileUIColors(TechTreeTile curTile)
    { 
        foreach (KeyValuePair<TechDefinitionIDs, TechTreeTile> pair in _TechTreeTilesLookup)
        {
            if (pair.Value != curTile)
                pair.Value.UpdateUIColors();
        }
    }

    private void CreateTechTileGroupObject(List<TechTreeTileData> techTilesData, string name) 
    {
        GameObject groupObj = Instantiate(_TileGroupPrefab);

        // We set the parent here, because by default the engine keeps the prefab's original world position.
        // So we have to set the parent this way, rather than just passing it into the Instantiate() call above.
        // This false parameter tells it not to keep the original world position.
        groupObj.transform.SetParent(_ScrollViewContentObject.transform);


        TechTreeTileGroup newGroup = new TechTreeTileGroup() { CategoryName = name,
                                                               TechTiles = new List<TechTreeTile>() };

        int rowIndex = _TechTreeTileGroups.Count;

        //Debug.Log($"Tile Count: {techTilesData.Count}");
        bool startAllUnlocked = GameManager.Instance.StartWithAllTechUnlocked;
        for (int i = 0; i < techTilesData.Count; i++)
        {
            TechTreeTile newTile = Instantiate(_TilePrefab).GetComponent<TechTreeTile>();

            newTile.transform.SetParent(groupObj.transform);

            TechTreeTileData tileData = techTilesData[i];
            tileData.IsAvailableToResearch = !startAllUnlocked ? i == 0 : true; // Set all tiles after the first one in the group to be locked, unless the StartWithAllTechUnlocked option is enabled.
            tileData.IsResearched = startAllUnlocked; // Default to false unless the StartWithAllTechUnlocked option is enabled.

            newTile.SetTileData(this, tileData);
            newTile.OnClick += OnTileClicked;
            newTile.OnMouseOver += OnTileMouseEnter;
            newTile.OnMouseExit += OnTileMouseExit;

            // Store the column and row indices of this tile into the tile data.
            tileData.TileIndices = new Vector2Int(i, rowIndex);


            // Add this research tile to the lookup table.
            if (_TechTreeTilesLookup.ContainsKey(tileData.TechID))
                throw new Exception($"A tech tile has already been created for the tech ID \"{tileData.TechID}\"");
            _TechTreeTilesLookup.Add(tileData.TechID, newTile);

            // Also add research tile into the current group data block.
            newGroup.TechTiles.Add(newTile);

        } // end foreach tileData


        _TechTreeTileGroups.Add(newGroup);
    }

    private bool GroupExists(string headerText)
    {
        foreach (TechTreeTileGroup group in _TechTreeTileGroups)
        {
            if (group.CategoryName == headerText)
                return true;
        }


        return false;
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


    public string LockedTileDescriptionText { get { return _LockedTileDescriptionText; } }

    public Color32 HighlightColor { get { return _HighlightColor; } }
    public Color32 SelectedColor { get { return _SelectedColor; } }
    public Color32 ResearchedColor { get { return _ResearchedColor; } }
    public Color32 LockedColor { get { return _LockedColor; } }
    public Color32 UnlockedColor { get { return _UnlockedColor; } }

    public Color32 TileXPTextColor { get { return _TileNormalXPCostTextColor; } }
    public Color32 TileNotEnoughXPTextColor { get { return _TileNotEnoughXPTextColor; } }

}
