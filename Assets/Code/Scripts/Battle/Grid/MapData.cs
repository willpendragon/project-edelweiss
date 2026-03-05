using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMap", menuName = "Map")]
public class MapData : ScriptableObject
{
    public enum LevelType
    {
        Regular,
        Puzzle
    }


    [System.Serializable]
    public struct TileData
    {
        public Vector2Int position;
        public TileType tileType;
    }

    public List<TileData> tilePositions = new List<TileData>();

    public int horizontalSize;
    public int verticalSize;

    public GameObject environment;
    public LevelType levelType;
}
