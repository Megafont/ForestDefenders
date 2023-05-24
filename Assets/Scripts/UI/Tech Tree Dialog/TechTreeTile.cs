using System.Collections;
using System.Collections.Generic;


using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;
using System.Diagnostics.Tracing;

public partial class TechTreeTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TechTreeTileData _TileData;

    private Button _Button;
    private TMP_Text _TitleText;
    private TMP_Text _XPCostText;
    private Image _IconImage;

    private GameManager _GameManager;
    private TechTreeDialog _ParentDialog;

    private bool _IsHighlighted;



    public delegate void TechTreeTile_EventHandler(TechTreeTile sender);

    public event TechTreeTile_EventHandler OnClick;
    public event TechTreeTile_EventHandler OnMouseOver;
    public event TechTreeTile_EventHandler OnMouseExit;



    // Start is called before the first frame update
    void Start()
    {
        _GameManager = GameManager.Instance;

        _Button = GetComponent<Button>();
        _TitleText = transform.Find("Title Text (TMP)").GetComponent<TMP_Text>();
        _XPCostText = transform.Find("XP Cost Text (TMP)").GetComponent<TMP_Text>();
        _IconImage = GetComponentInChildren<Image>();

        UpdateGUIColors();

        UpdateGUI();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TileClickedHandler()
    {
        OnClick?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseOver?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData) 
    {
        OnMouseExit?.Invoke(this);
    }

    private void UpdateGUI()
    {
        _TitleText.text = _TileData.IsAvailableToResearch ? _TileData.Title : _ParentDialog.LockedTileDescriptionText;

        _XPCostText.text = !_TileData.IsResearched ? $"Cost: {_TileData.XPCost} XP" : null; // Display the XP only if the tech item has NOT been researched yet.
    }

    public void UpdateGUIColors()
    {
        if (_Button == null)
            return;


        ColorBlock block = _Button.colors;


        Color32 color = _ParentDialog.LockedColor;
        if (_TileData.IsResearched)
        {
            color = _ParentDialog.ResearchedColor;
        }
        else
        {
            if (_GameManager.ResearchIsFree || _TileData.XPCost <= _ParentDialog.AvailableXP)
                _XPCostText.color = _ParentDialog.TileXPTextColor;
            else
                _XPCostText.color = _ParentDialog.TileNotEnoughXPTextColor;


            if (_GameManager.StartWithAllTechUnlocked || _TileData.IsAvailableToResearch)
                color = _ParentDialog.UnlockedColor;
            else
                color = _ParentDialog.LockedColor;
        }


       
        if (IsHighlighted)
            color = _ParentDialog.HighlightColor;
        //if (_SelectionState == TechTreeTileSelectionStates.Selected)
        //    color = _ParentDialog.SelectedColor;

        block.normalColor = color;
        block.disabledColor = color;
        block.selectedColor = color;
        block.highlightedColor = color;
        
        _Button.colors = block;
    }


    public void SetIsAvailableToResearchFlag(bool value)
    {
        _TileData.IsAvailableToResearch = value;

        UpdateGUIColors();
        UpdateGUI();
    }

    public void SetResearchedFlag(bool value)
    {
        _TileData.IsResearched = value;

        UpdateGUIColors();
        UpdateGUI();
    }

    public void SetTileData(TechTreeDialog parentDialog, TechTreeTileData tileData)
    {
        _ParentDialog = parentDialog;
        _TileData = tileData;
    }



    
    public bool IsHighlighted 
    { 
        get { return _IsHighlighted; } 
        set
        {
            _IsHighlighted = value;

            UpdateGUIColors();
        }
    }


    public TechTreeTileData TileData { get { return _TileData; } }
}

