using UnityEngine;

[CreateAssetMenu(fileName = "NewMapGenerationConfig", menuName = "Overworld/Map Generation Config")]
public class MapGenerationConfig : ScriptableObject
{
    [Header("Map Scatter Generation")]
    public float mapWidth = 60f;
    public float mapDepth = 20f;
    public float minDistanceApart = 7f;

    [Header("Seed Settings")]
    public int randomSeed = 12345;

    [Header("Game Rules")]
    [Tooltip("If true, players can replay Regular Battles that they have already cleared to prevent softlocks.")]
    public bool allowRepeatableRegularBattles = true;

    [Header("Node Distribution Weights")]
    [Range(0, 100)] public float regularBattleWeight = 70f;
    [Range(0, 100)] public float puzzleBattleWeight = 20f;
    [Range(0, 100)] public float minibossBattleWeight = 10f;
    [Range(0, 100)] public float bossBattleWeight = 0f; // Typically placed deliberately

    [Header("Node Spawn Thresholds")]
    [Tooltip("Node index after which Puzzle Battles can spawn")]
    public int puzzleBattleThreshold = 3;
    [Tooltip("Node index after which Miniboss Battles can spawn")]
    public int minibossBattleThreshold = 5;
}