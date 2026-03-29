using ProjectEdelweiss.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] ObstacleController _obstacleController;

    public GameObject lineRendererPrefab;
    private static LineRenderer lineRendererInstance;

    public List<GameObject> statusIcons = new List<GameObject>();

    public delegate void MoveUnitDelegate(int targetX, int targetY);
    public event MoveUnitDelegate OnMoveUnit;

    public GameObject tilePrefab;
    public int gridHorizontalSize;
    public int gridVerticalSize;
    public float inBetweenTilesXOffset;
    public float inBetweenTilesYOffset;
    public float tileVerticalOffset = 0.56f;
    public GameObject currentPlayerUnit;

    public bool tileSelectionPermitted;
    public bool AOESelectionPermitted = true;

    public TileController[] gridTileControllers;
    public List<TileShaderController> _tileShaderControllers;
    [SerializeField] EnvironmentController _environmentController;

    public Dictionary<PositionKey, TileController> gridMapDictionary = new Dictionary<PositionKey, TileController>();
    public GridMovementController gridMovementController;

    public delegate void GridMovementModeActivated();
    public static event GridMovementModeActivated OnGridMovementModeActivated;

    public delegate void GridTargetSelectionModeActivated();
    public static event GridTargetSelectionModeActivated OnGridTargetSelectionModeActivated;

    public delegate void SetUnitInitialPositionOnGrid();
    public static event SetUnitInitialPositionOnGrid OnSetUnitInitialPositionOnGrid;

    public delegate void SpawnActivationPlatforms();
    public static event SpawnActivationPlatforms OnSpawnActivationPlatforms;

    public static bool IsUnitMoving = false;

    [Header("Map Layouts")]
    public MapData currentMapData;
    public MapData puzzleMapData;

    [SerializeField] UnitSetupController unitSetupController;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        GenerateStandardMap();
    }

    private void GenerateStandardMap()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == GameTags.BATTLE_SCENE)
        {
            currentMapData = GameManager.Instance.CurrentMap;
            GenerateGridMapFromData(currentMapData);
            _obstacleController.SpawnObstacles();
        }
    }

    private void Start()
    {
        gridTileControllers = GameObject.FindObjectsOfType<TileController>();
        if (OnSpawnActivationPlatforms != null)
        {
            OnSpawnActivationPlatforms.Invoke();
        }
        // Cache Tile Shaders controllers
        SetTileShaderControllers();
    }

    private void SetTileShaderControllers()
    {
        foreach (var tile in gridTileControllers)
        {
            var tileShader = tile.tileShaderController;
            _tileShaderControllers.Add(tileShader);
        }
    }

    public void GenerateGridMapFromData(MapData currentMapData)
    {
        ClearGridMap();

        if (currentMapData == null)
        {
            Debug.LogError("No MapData assigned to GridManager.");
            return;
        }

        GridManager.Instance.gridHorizontalSize = currentMapData.horizontalSize;
        GridManager.Instance.gridVerticalSize = currentMapData.verticalSize;

        foreach (var tileData in currentMapData.tilePositions)
        {
            // ORA CALCOLIAMO LA Y IN BASE ALL'ELEVAZIONE: y = tileData.position.y * tileVerticalSpacing
            float tileVerticalSpacing = 1f; // Regola questo valore in base all'altezza visiva (es. spessore) del tuo cubo Voxel
            
            Vector3 tilePosition = new Vector3(
                tileData.position.x * (1 + inBetweenTilesXOffset), 
                tileData.position.y * tileVerticalSpacing + tileVerticalOffset, 
                tileData.position.z * (1 + inBetweenTilesYOffset)
            );

            GameObject tilePrefabInstance = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
            TileController tileController = tilePrefabInstance.GetComponent<TileController>();
            
            // Assegniamo il nuovo gridPosition completo
            tileController.gridPosition = tileData.position;
            tileController.tileType = tileData.tileType; 
            
            PositionKey positionKey = new PositionKey(tileData.position, tilePrefab);

            if (!gridMapDictionary.ContainsKey(positionKey))
            {
                gridMapDictionary.Add(positionKey, tileController);
            }
            else
            {
                Debug.LogWarning($"Duplicate key found when adding GameObject to dictionary at {tileData.position}");
            }
        }
    }

    private void ClearGridMap()
    {
        foreach (var tile in gridMapDictionary.Values)
        {
            Destroy(tile.gameObject);
        }
        gridMapDictionary.Clear();
    }

    public LineRenderer GetLineRenderer()
    {
        if (lineRendererInstance == null)
        {
            GameObject lineRendererObj = Instantiate(lineRendererPrefab);
            lineRendererInstance = lineRendererObj.GetComponentInChildren<LineRenderer>();
            lineRendererObj.transform.SetParent(transform);
        }
        return lineRendererInstance;
    }

    public TileController GetTileControllerInstance(int xCoordinate, int elevationY, int zCoordinate)
    {
        PositionKey positionKeyToFind = new PositionKey(xCoordinate, elevationY, zCoordinate, null);

        if (gridMapDictionary.TryGetValue(positionKeyToFind, out TileController result))
        {
            return result;
        }
        return null;
    }
    
    // VERSIONE "SOVRACCARICATA" EXTRA 2D PER NON ROMPERE TUTTO SUBITO
    // Molti tuoi script passano ancora (X, Y) credendo sia la vista dall'alto (dove la loro vecchia Y è ora la nostra Z)
    // Usiamo questa per cercare i tile al "Piano Terra" (Elevazione 0).
    public TileController GetTileControllerInstance(int xCoordinate, int zOrOldYCoordinate)
    {
        return GetTileControllerInstance(xCoordinate, 0, zOrOldYCoordinate);
    }
    public List<Vector2Int> GetExistingTileCoordinates()
    {
        List<Vector2Int> existingTiles = new List<Vector2Int>();
        foreach (var key in gridMapDictionary.Keys)
        {
            existingTiles.Add(new Vector2Int(key.indexTileXPosition, key.indexTileYPosition));
            //// Test: Exclude tiles marked as obstacles.
            //if (key.tileController.GetComponent<TileController>().tileType != TileType.Obstacle)
            //{
            //}
        }
        return existingTiles;
    }

    public void MoveCurrentPlayerUnit(int targetX, int targetY)
    {
        if (currentPlayerUnit.GetComponent<UnitStatusController>().unitCurrentStatus != UnitStatus.stun)
        {
            currentPlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
            currentPlayerUnit.GetComponent<Unit>().MoveUnit(targetX, targetY, false);
            TileController finalDestinationTile = GetTileControllerInstance(targetX, targetY);
            {
                if (finalDestinationTile.detectedUnit == null)
                {
                    currentPlayerUnit.GetComponent<Unit>().ownedTile.detectedUnit = null;
                    currentPlayerUnit.GetComponent<Unit>().ownedTile.currentSingleTileCondition = SingleTileCondition.free;
                    currentPlayerUnit.GetComponent<Unit>().ownedTile = finalDestinationTile;
                    currentPlayerUnit.GetComponent<Unit>().ownedTile.detectedUnit = currentPlayerUnit;
                    currentPlayerUnit.GetComponent<Unit>().ownedTile.currentSingleTileCondition = SingleTileCondition.occupied;
                    currentPlayerUnit.GetComponent<Unit>().unitOpportunityPoints--;
                    GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

                    foreach (var tile in tiles)
                    {
                        tile.GetComponent<TileController>().currentSingleTileStatus = SingleTileStatus.characterSelectionModeActive;
                    }
                    currentPlayerUnit.GetComponent<Unit>().ownedTile.GetComponent<TileController>().currentSingleTileStatus = SingleTileStatus.selectedPlayerUnitOccupiedTile;
                    currentPlayerUnit.GetComponent<Unit>().ownedTile.GetComponent<TileController>().detectedUnit = currentPlayerUnit;
                }
            }
            Debug.Log("Moving Player Unit to (" + targetX + ", " + targetY + ")");
        }
    }
    public Vector3 GetWorldPositionFromGridCoordinates(int x, int y)
    {
        float worldX = x * (1 + inBetweenTilesXOffset);
        float worldZ = y * (1 + inBetweenTilesYOffset);
        return new Vector3(worldX, 0, worldZ);
    }
    public Vector2Int GetGridCoordinatesFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / (1 + inBetweenTilesXOffset));
        int y = Mathf.RoundToInt(worldPosition.z / (1 + inBetweenTilesYOffset));
        return new Vector2Int(x, y);
    }
    public void RemoveTrapSelection()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        foreach (var tile in gridTileControllers)
        {
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
        }
    }
    public void RefreshGridTileControllers()
    {
        gridTileControllers = gridMapDictionary
            .Values
            .Where(tile => tile != null)
            .ToArray();
    }

    public void ClearPath()
    {
        if (lineRendererInstance != null)
        {
            lineRendererInstance.positionCount = 0;
        }
    }

    public List<Vector2Int> GetSpecialTiles()
    {
        List<Vector2Int> specialTiles = new List<Vector2Int>();
        foreach (var tile in gridTileControllers)
        {
            if (tile.tileType == TileType.Obstacle)
            {
                Vector2Int coords = new Vector2Int(tile.tileXCoordinate, tile.tileYCoordinate);
                specialTiles.Add(coords);
            }
        }
        return specialTiles;
    }

    public void ClearTileColors()
    {
        // Not ideal performance-wise, consider refactor.
        foreach (var tileShader in _tileShaderControllers)
        {
            tileShader.SetTileGlowIntensity(0f);
        }
    }
}