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

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    [Tooltip("Add the title of a convo from the Dialogue Database to trigger it at the start of a fight. Double check" +
             "to insert the exact same title, otherwise it won't work")]
    public string BattleDialogue;

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

    [System.Serializable]
    public struct SpawnData
    {
        public Vector3Int position;
        public string prefabName;
        public int linkID;
    }

    [System.Serializable]
    public class EnvironmentData
    {
        public string prefabName;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;
    }

    // --- NEW: Programmatic Light Data ---
    [System.Serializable]
    public class LightData
    {
        public LightType type;
        public Vector3 position;
        public Vector3 rotation;
        public Color color = Color.white;
        public float intensity = 1f;
        public float range = 10f;
        public float spotAngle = 30f;
    }

    public List<TileData> tilePositions = new List<TileData>();
    public List<DecorationData> decorationPositions = new List<DecorationData>();
    public List<SpawnData> playerSpawnPositions = new List<SpawnData>();
    public List<SpawnData> interactablePositions = new List<SpawnData>();
    public List<SpawnData> enemySpawnPositions = new List<SpawnData>();
    public List<SpawnData> deityShardPositions = new List<SpawnData>();
    public List<EnvironmentData> environmentPositions = new List<EnvironmentData>();
    public List<LightData> lightSettings = new List<LightData>();

    public int horizontalSize; // X
    public int verticalSize; // Z
    public int depthSize = 1; // Y

    public GameObject environment;
    public LevelType levelType;
    public Difficulty difficultyLevel = Difficulty.Easy;
    public Vector3 environmentSpawnpoint;

    [Header("Camera Configuration")] public bool overrideCameraSettings = false;
    public Vector3 cameraPosition;
    public Vector3 cameraRotation;
    public float cameraZoom = 60f;
    public bool isOrthographic = false;
    public float orthographicSize = 5f;

    [Header("Directional Light")] public bool overrideDirectionalLight = false;
    public Vector3 directionalLightRotation = new Vector3(50f, -30f, 0f);
    public Color directionalLightColor = Color.white;
    public float directionalLightIntensity = 1f;

    [Header("Global Volume")] public bool overrideGlobalVolume = false;
    public UnityEngine.Rendering.VolumeProfile globalVolumeProfile;

    public GameObject fixedDeity;

    public GameObject RetrieveDeity()
    {
        if (fixedDeity == null)
            return null;
        else
            return fixedDeity;
    }
}