using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMap", menuName = "Level Design/Map")]
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
    public Vector3 environmentSpawnpoint;

    public GameObject fixedDeity; // Use this only for cases where you need to spawn a Deity in a specific level without requiring achievement-based unlock logic.

    public GameObject RetrieveDeity()
    {
        if (fixedDeity == null)
            return null;
        else
            return fixedDeity;
    }
}