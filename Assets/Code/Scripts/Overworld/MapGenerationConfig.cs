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
    
    // --- UPDATED TOOLTIP ---
    [Tooltip("If true, generates a new random map layout each run while strictly obeying the sliders below. (Note: ignored if 'Fully Randomize Rules' is checked below).")]
    public bool randomizeSeedOnGeneration = false;

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

    [Header("Total Randomization (Roguelike Mode)")]
    // --- UPDATED TOOLTIP ---
    [Tooltip("If true, creates a new random map layout AND randomizes all weights, scatter, and thresholds within the ranges below. Overrides Seed Settings and exact sliders.")]
    public bool fullyRandomizeRules = false;
    
    // Bounds for randomization
    public Vector2 mapWidthRange = new Vector2(40f, 80f);
    public Vector2 minDistanceRange = new Vector2(5f, 9f);
    
    public Vector2 regularWeightRange = new Vector2(50f, 80f);
    public Vector2 puzzleWeightRange = new Vector2(10f, 40f);
    public Vector2 minibossWeightRange = new Vector2(5f, 25f);

    public Vector2Int puzzleThresholdRange = new Vector2Int(1, 4);
    public Vector2Int minibossThresholdRange = new Vector2Int(4, 7);
}