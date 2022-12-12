
using UnityEngine;


public class TechTreeTileData
{
    public TechDefinitionIDs TechID; // The ID of the tech referenced by this tile.
    public Vector2Int TileIndices; // The column and row position of the tile.

    public string Title;
    public string DescriptionText;
    public int XPCost;

    public bool IsLocked; // Whether this tile is still locked, or is unlocked and ready to be researched.
    public bool IsResearched; // Whether this tile has been researched.

}

