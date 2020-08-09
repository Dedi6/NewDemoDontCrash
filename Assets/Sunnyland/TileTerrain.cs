using UnityEngine.Tilemaps;
using UnityEngine;


[CreateAssetMenu(fileName = "New TerrainTypeTile", menuName = "Tiles/TerrainTypeTile")]
public class TileTerrain : RuleTile  // or TileBase or RuleTile or other
{
    public enum TerrainList
    {
        Grass,
        Wood,
        Stone,
    }
    // will be able to plug in value you want in Inspector for asset
    public TerrainList TerrainType;
    public int testInt = 1;
}