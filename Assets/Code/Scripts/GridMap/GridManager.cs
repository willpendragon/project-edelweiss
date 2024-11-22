using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public GameObject lineRendererPrefab; // Assign this in the Unity Editor
    private static LineRenderer lineRendererInstance;

    public List<GameObject> statusIcons = new List<GameObject>();

    public delegate void MoveUnitDelegate(int targetX, int targetY);
    public event MoveUnitDelegate OnMoveUnit;

    public GameObject tilePrefab;
    public int gridHorizontalSize;
    public int gridVerticalSize;
    public float inBetweenTilesXOffset;
    public float inBetweenTilesYOffset;
    public GameObject currentPlayerUnit;

    //public int moveSelectionLimiter = 1;

    public bool tileSelectionPermitted;

    public TileController[] gridTileControllers;

    private Dictionary<PositionKey, TileController> gridMapDictionary = new Dictionary<PositionKey, TileController>();
    public GridMovementController gridMovementController;

    public delegate void GridMovementModeActivated();
    public static event GridMovementModeActivated OnGridMovementModeActivated;

    public delegate void GridTargetSelectionModeActivated();
    public static event GridTargetSelectionModeActivated OnGridTargetSelectionModeActivated;

    public delegate void SetUnitInitialPositionOnGrid();
    public static event SetUnitInitialPositionOnGrid OnSetUnitInitialPositionOnGrid;

    // Add a reference to the MapData
    public MapData currentMapData;

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
        GenerateGridMapFromData();
    }

    private void Start()
    {
        gridTileControllers = GameObject.FindObjectsOfType<TileController>();
    }

    // Update GenerateGridMap to use MapData
    public void GenerateGridMapFromData()
    {
        ClearGridMap();

        if (currentMapData == null)
        {
            Debug.LogError("No MapData assigned to GridManager.");
            return;
        }

        Debug.Log("Generating Grid Map from MapData");
        foreach (var tileData in currentMapData.tilePositions)
        {
            Vector3 tilePosition = new Vector3(tileData.position.x * (1 + inBetweenTilesXOffset), 0, tileData.position.y * (1 + inBetweenTilesYOffset));
            GameObject tilePrefabInstance = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
            TileController tileController = tilePrefabInstance.GetComponent<TileController>();
            tileController.tileXCoordinate = tileData.position.x;
            tileController.tileYCoordinate = tileData.position.y;
            tileController.tileType = tileData.tileType; // Set the tile type
            PositionKey positionKey = new PositionKey(tileData.position.x, tileData.position.y, tilePrefab);

            if (!gridMapDictionary.ContainsKey(positionKey))
            {
                gridMapDictionary.Add(positionKey, tileController);
            }
            else
            {
                Debug.LogWarning("Duplicate key found when adding GameObject to dictionary!");
            }
        }

        Debug.Log("Dictionary Count: " + gridMapDictionary.Count);
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

    public TileController GetTileControllerInstance(int xCoordinate, int yCoordinate)
    {
        PositionKey positionKeyToFind = new PositionKey(xCoordinate, yCoordinate, null);

        if (gridMapDictionary.TryGetValue(positionKeyToFind, out TileController result))
        {
            if (result != null)
            {
                TileController tileController = result.GetComponent<TileController>();

                if (tileController != null)
                {
                    return tileController;
                }
                else
                {
                    Debug.LogError("TileController component not found");
                    return null;
                }
            }
            else
            {
                Debug.LogError("GameObject is null.");
                return null;
            }
        }
        else
        {
            Debug.Log("Key not found");
            return null;
        }
    }

    public List<Vector2Int> GetExistingTileCoordinates()
    {
        List<Vector2Int> existingTiles = new List<Vector2Int>();
        foreach (var key in gridMapDictionary.Keys)
        {
            existingTiles.Add(new Vector2Int(key.indexTileXPosition, key.indexTileYPosition));
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
        TrapTileUIController trapTileUIController = activePlayerUnit.GetComponent<TrapTileUIController>();
        trapTileUIController.trapTileSelectionIsActive = true;

        foreach (var tile in gridTileControllers)
        {
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            tile.gameObject.GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
        }
    }
}
