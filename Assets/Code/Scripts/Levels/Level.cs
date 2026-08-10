using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level Design/Level", order = 0)]
public class Level : ScriptableObject
{
    public int levelNumber;
    public EnemyPartyData enemyPartyData;
    public MapData map;
    public string conversationTitle; // Overrides MapData dialogue config. Leave it blank, if you don't want to override/show dialogues.
}