using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level", order = 1)]
public class Level : ScriptableObject
{
    public int levelNumber;
    public EnemyPartyData enemyPartyData;
    public MapData map;
}