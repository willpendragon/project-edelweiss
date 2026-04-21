using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMap", menuName = "Level Design/Map")]
public class MapData : ScriptableObject
{
    public enum LevelType
    {
        Regular,
        Puzzle,
        Miniboss,
        Boss
    }

    // --- NEW: Difficulty Enum ---
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    [System.Serializable]
    public struct TileData
    {
        public Vector3Int position;
        public TileType tileType;
    }

    [System.Serializable]
    public struct DecorationData
    {
        public Vector3Int position;
        public string prefabName; 
    }

    // --- NEW: Player Spawn Data Storage ---
    [System.Serializable]
    public struct SpawnData
    {
        public Vector3Int position;
        public string prefabName; 
        public int linkID; // <--- ADD THIS
    }

    public List<TileData> tilePositions = new List<TileData>();
    public List<DecorationData> decorationPositions = new List<DecorationData>();
    public List<SpawnData> playerSpawnPositions = new List<SpawnData>();
    public List<SpawnData> interactablePositions = new List<SpawnData>(); 

    public int horizontalSize; // X
    public int verticalSize;   // Z
    public int depthSize = 1;  // Y

    public GameObject environment;
    public LevelType levelType;
    public Difficulty difficultyLevel = Difficulty.Easy; // --- NEW: Difficulty Assignment ---
    public Vector3 environmentSpawnpoint;

    // --- NEW: Camera Config ---
    [Header("Camera Configuration")]
    public bool overrideCameraSettings = false;
    public Vector3 cameraPosition;
    public Vector3 cameraRotation;
    public float cameraZoom = 60f;
    public bool isOrthographic = false;
    public float orthographicSize = 5f;

    public GameObject fixedDeity; 

    public GameObject RetrieveDeity()
    {
        if (fixedDeity == null)
            return null;
        else
            return fixedDeity;
    }
}