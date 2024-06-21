using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMap", menuName = "Map")]
public class MapData : ScriptableObject
{
    public List<Vector2Int> tilePositions = new List<Vector2Int>();
}