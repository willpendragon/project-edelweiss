using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMap", menuName = "Map")]
public class MapData : ScriptableObject
{
    [System.Serializable]
    public struct TileData
    {
        public Vector2Int position;
        public TileType tileType;
    }

    public List<TileData> tilePositions = new List<TileData>();
}
