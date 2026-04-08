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
}