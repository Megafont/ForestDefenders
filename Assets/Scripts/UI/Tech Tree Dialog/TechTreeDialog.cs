using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class TechTreeDialog : Dialog_Base, IDialog
{
    const float GAMEPAD_MIN_THRESHOLD = 0.5f; // The minimum amount the left stick must be pressed to move the selection.


    [Header("UI Prefabs")]
    [SerializeField] private GameObject _TileGroupHeaderPrefab;
    [SerializeField] private GameObject _TileGroupPrefab;
    [SerializeField] private GameObject _TilePrefab;

    [Header("Colors")]
    [SerializeField] private Color32 _HighlightColor = new Color32(220, 150, 0, 200);
    [SerializeField] private Color32 _LockedColor = new Color32(64, 64, 64, 200);
    [SerializeField] private Color32 _UnlockedColor = new Color32(0, 0, 0, 200);
    [SerializeField] private Color32 _ResearchedColor = new Color32(0, 64, 0, 200);

    [SerializeField] private Color32 _TileNormalXPCostTextColor = Color.white;
    [SerializeField] private Color32 _TileNotEnoughXPTextColor = Color.red;

    [Header("Display")]
    [SerializeField] private string _LockedTileNameText = "???";
    [SerializeField] private string _LockedTileDescriptionText = "-";
    [SerializeField] private Sprite _LockedTileThumbnail;

    [Header("Audio")]
    [SerializeField] private AudioClip _UnlockTechSound;
    [Range(0,1)]
    [SerializeField] private float _UnlockTechSoundVolume = 1.0f;

    private struct TechTreeTileGroup
    {
        public string CategoryName;
        public List<TechTreeTile> TechTiles;
    }


    private TMP_Text _TechNameText;
    private TMP_Text _TechDescriptionText;
    private TMP_Text _AvailableXPText;


    private ScrollRect _ScrollViewScrollRect;
    private GameObject _ScrollViewContentObject;

    private Dictionary<TechDefinitionIDs, TechTreeTile> _TechTreeTilesLookup;
    private List<TechTreeTileGroup> _TechTreeTileGroups;

    private Vector2Int _SelectedTileIndices;

    private ScrollRect _ScrollRect;

    private float _LastGamepadSelectionChangeTime;

    AudioSource _UnlockTechAudioSource;



    protected override void Dialog_OnAwake()
    {
        _PauseGameWhileDialogOpen = true;

        _UnlockTechAudioSource = gameObject.AddComponent<AudioSource>();
    }

    protected override void Dialog_OnStart()
    {        
        _ScrollRect = transform.GetComponentInChildren<ScrollRect>();
        if (_ScrollRect == null)
            Debug.LogError("The ScrollView's ScrollRect component was not found!");


        _AvailableXPText = transform.Find("Panel/XP Count Text (TMP)").GetComponent<TMP_Text>();

        _TechNameText = transform.Find("Panel/Tech Name Text (TMP)").GetComponent<TMP_Text>();
        _TechNameText.text = _LockedTileNameText;

        _TechDescriptionText = transform.Find("Panel/Tech Description Text (TMP)").GetComponent<TMP_Text>();
        _TechDescriptionText.text = _LockedTileDescriptionText;

        _ScrollViewScrollRect = transform.Find("Panel/Scroll View").GetComponent<ScrollRect>();
        _ScrollViewContentObject = transform.Find("Panel/Scroll View/Viewport/Content").gameObject;


        _TechTreeTilesLookup = new Dictionary<TechDefinitionIDs, TechTreeTile>();
        _TechTreeTileGroups = new List<TechTreeTileGroup>();

        TechDefinitions.GetTechDefinitions(this);
        if (_TechTreeTileGroups.Count > 0)
        {
            _SelectedTileIndices = Vector2Int.zero;
            SelectedTile.IsHighlighted = true;
        }
        else
        {
            _SelectedTileIndices = new Vector2Int(-1, -1);
        }


        gameObject.SetActive(false);

    }

    protected override void Dialog_OnUpdate()
    {
        if (_InputManager_UI != null &&
            _InputManager_UI.CloseTechTree)
        {
            CloseDialog();
        }    
    }

    protected override void Dialog_OnNavigate()
    {
        if (Time.unscaledTime - _LastGamepadSelectionChangeTime < _GameManager.GamepadMenuSelectionDelay)
            return;



        float scrollMagnitudeY = _InputManager_UI.Navigate.y;
        float scrollMagnitudeX = _InputManager_UI.Navigate.x;

        TechTreeTile curTile = SelectedTile;

        bool playerScrolled = false;


        // Did the player scroll up?
        if (scrollMagnitudeY >= GAMEPAD_MIN_THRESHOLD)
        {
            _SelectedTileIndices.y -= 1;
            if (_SelectedTileIndices.y < 0)
                _SelectedTileIndices.y = _TechTreeTileGroups.Count - 1;

            playerScrolled = true;
        }

        // Did the player scroll down?
        if (scrollMagnitudeY <= -GAMEPAD_MIN_THRESHOLD)
        {
            _SelectedTileIndices.y += 1;
            if (_SelectedTileIndices.y >= _TechTreeTileGroups.Count)
                _SelectedTileIndices.y = 0;

            playerScrolled = true;
        }


        // If we selected a different row, make sure the x index is still valid. If not, snap it to the end of the row.
        TechTreeTileGroup selectedTileGroup = _TechTreeTileGroups[_SelectedTileIndices.y];
        if (_SelectedTileIndices.x >= selectedTileGroup.TechTiles.Count)
            _SelectedTileIndices.x = selectedTileGroup.TechTiles.Count - 1;


        // Did the player scroll left?
        if (scrollMagnitudeX <= -GAMEPAD_MIN_THRESHOLD)
        {
            _SelectedTileIndices.x -= 1;
            if (_SelectedTileIndices.x < 0)
                _SelectedTileIndices.x = selectedTileGroup.TechTiles.Count - 1;

            playerScrolled = true;
        }


        // Did the player scroll right?
        if (scrollMagnitudeX >= GAMEPAD_MIN_THRESHOLD)
        {
            _SelectedTileIndices.x += 1;
            if (_SelectedTileIndices.x >= selectedTileGroup.TechTiles.Count)
                _SelectedTileIndices.x = 0;

            playerScrolled = true;
        }


        if (playerScrolled)
        {
            _LastGamepadSelectionChangeTime = Time.unscaledTime;

            curTile.IsHighlighted = false;

            TechTreeTile selected = SelectedTile;
            
            selected.IsHighlighted = true;
            _ScrollViewScrollRect.FocusOnItem(selected.GetComponent<RectTransform>());

            UpdateDescriptionText(selected);
        }
        


        /* THIS IS OLD CODE THAT SCROLLS THE SCROLLVIEW WITHOUT FOCUSING IT ON THE SELECTED ITEM.
        // Check if the user is pressing up or down. If so, scrolls the high scores table.        
        if (scrollMagnitudeY != 0)
        {
            float scrollableHeight = _ScrollRect.content.sizeDelta.y - _ScrollRect.viewport.rect.height;
            float scrollAmount = KEYBOARD_SCROLL_SPEED * scrollMagnitudeY;// * Time.unscaledDeltaTime;
            _ScrollRect.verticalNormalizedPosition += scrollAmount / scrollableHeight;
        }
        */


    }

    protected override void Dialog_OnSubmit()
    {
        // Attempt to unlock the selected tile.
        UnlockTile(SelectedTile);
    }

    protected override void Dialog_OnCancel()
    {
        CloseDialog();
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

    private Vector2Int GetTileIndices(TechTreeTile tile)
    {
        if (!tile || _TechTreeTileGroups.Count < 1)
            return new Vector2Int(-1, -1);


        Vector2Int indices = Vector2Int.zero;

        for (int i = 0; i < _TechTreeTileGroups.Count; i++)
        {
            if (_TechTreeTileGroups[i].TechTiles.Contains(tile))
            {
                indices.x = _TechTreeTileGroups[i].TechTiles.IndexOf(tile);
                indices.y = i;

                break;
            }

        } // end for

        return indices;
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
            throw new Exception("The amount of XP added must be positive!");

        AvailableXP += amount;
    }

    public void AddTechGroup(string headerText, List<TechTreeTileData> techTiles)
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
        if (!sender)
            return;


        // Debug.Log($"Tile \"{sender.TileData.Title}\" clicked!");


        // Attempt to unlock the clicked tile.
        UnlockTile(sender);

    }

    private void OnTileMouseEnter(TechTreeTile sender)
    {
        // Deselect the current tile.
        SelectedTile.IsHighlighted = false;


        // Select the new tile.
        _SelectedTileIndices = GetTileIndices(sender);

        UpdateDescriptionText(sender);

        sender.IsHighlighted = true;
    }

    public void OnTileMouseExit(TechTreeTile sender)
    {
        
        TechTreeTile selected = SelectedTile;
        
        // If the tile the mouse left is no longer the selected tile, then unhighlight it.
        if (sender != selected)
            sender.IsHighlighted = false;

        // Since the mouse left this tile, display the description text for the selected tile if there is one.
        if (selected != null)
        {
            UpdateDescriptionText(selected);
        }
        else
        {
            UpdateDescriptionText(null);
        }

    }

    private void UnlockTile(TechTreeTile tileToUnlock)
    {
        _SelectedTileIndices = GetTileIndices(tileToUnlock);

        if (tileToUnlock.TileData.IsResearched)
            return;


        if (tileToUnlock.TileData.IsAvailableToResearch &&
            (AvailableXP >= tileToUnlock.TileData.XPCost || _GameManager.ResearchIsFree))
        {

            if (!_GameManager.ResearchIsFree)
                AvailableXP -= tileToUnlock.TileData.XPCost;


            // Play sound effect.
            _UnlockTechAudioSource.volume = _UnlockTechSoundVolume;
            _UnlockTechAudioSource.PlayOneShot(_UnlockTechSound);


            UpdateXPCountText();

            tileToUnlock.SetResearchedFlag(true);


            // Unlock the next tile in the row if there is one.
            Vector2Int tileIndices = tileToUnlock.TileData.TileIndices;
            TechTreeTileGroup tileGroup = _TechTreeTileGroups[tileIndices.y];
            if (tileIndices.x < tileGroup.TechTiles.Count - 1)
            {
                TechTreeTile nextTile = tileGroup.TechTiles[tileIndices.x + 1];
                nextTile.SetIsAvailableToResearchFlag(true);
            }


            // Update the description text if necessary.
            UpdateDescriptionText(tileToUnlock);


            int monsterWaveMultiplier = (int)Mathf.Max(_GameManager.MonsterManager.CurrentWaveNumber, 1.0f); // This line ensures this value is at least 1.0f since CurrentWaveNumber is 0 at the very start of the game.
            _GameManager.AddToScore(tileToUnlock.TileData.XPCost * _GameManager.PlayerResearchScoreMultiplier * monsterWaveMultiplier);


            // Enable the technology referenced by this tile.
            TechEnabler.EnableTech(tileToUnlock.TileData.Title);
        } // end if

    }

    private void UpdateDescriptionText(TechTreeTile techTile)
    {
        if (techTile == null || techTile.TileData.IsAvailableToResearch)
        {
            _TechNameText.text = $"{techTile.TileData.Title}";
            _TechDescriptionText.text = $"{techTile.TileData.DescriptionText}";
        }
        else
        {
            _TechNameText.text = $"{_LockedTileNameText}";
            _TechDescriptionText.text = $"{_LockedTileDescriptionText}";
        }
    }

    private void RefreshTileUIColors(TechTreeTile curTile)
    { 
        foreach (KeyValuePair<TechDefinitionIDs, TechTreeTile> pair in _TechTreeTilesLookup)
        {
            if (pair.Value != curTile)
                pair.Value.UpdateGUIColors();
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


        AddTechGroup("Test Group", tilesData);
        AddTechGroup("Test Group 2", tilesData2);
    }



    public int AvailableXP { get; private set; }


    public string LockedTileNameText { get { return _LockedTileNameText; } }
    public string LockedTileDescriptionText { get { return _LockedTileDescriptionText; } }
    public Sprite LockedTileThumbnail { get { return _LockedTileThumbnail; } }

    public TechTreeTile SelectedTile 
    { 
        get 
        {
            if (_TechTreeTileGroups.Count < 1)
                return null;

            return _TechTreeTileGroups[_SelectedTileIndices.y].TechTiles[_SelectedTileIndices.x]; 
        } 
    }

    public Color32 HighlightColor { get { return _HighlightColor; } }
    public Color32 ResearchedColor { get { return _ResearchedColor; } }
    public Color32 LockedColor { get { return _LockedColor; } }
    public Color32 UnlockedColor { get { return _UnlockedColor; } }

    public Color32 TileXPTextColor { get { return _TileNormalXPCostTextColor; } }
    public Color32 TileNotEnoughXPTextColor { get { return _TileNotEnoughXPTextColor; } }

}
