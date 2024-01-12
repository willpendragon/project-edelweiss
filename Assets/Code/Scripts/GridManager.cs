using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public delegate void MoveUnitDelegate(int targetX, int targetY);
    public event MoveUnitDelegate OnMoveUnit;

    public GameObject tilePrefab;
    public int gridHorizontalSize;
    public int gridVerticalSize;
    public float inBetweenTilesXOffset;
    public float inBetweenTilesYOffset;
    public GameObject currentPlayerUnit;

    private Dictionary<PositionKey, TileController> gridMapDictionary = new Dictionary<PositionKey, TileController>();
    public GridMovementController gridMovementController;

    public delegate void GridMovementModeActivated();
    public static event GridMovementModeActivated OnGridMovementModeActivated;

    public delegate void GridTargetSelectionModeActivated();
    public static event GridTargetSelectionModeActivated OnGridTargetSelectionModeActivated;

    public delegate void SetUnitInitialPositionOnGrid();
    public static event SetUnitInitialPositionOnGrid OnSetUnitInitialPositionOnGrid;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        GenerateGridMap();
    }
    private void Start()
    {
        OnSetUnitInitialPositionOnGrid();
    }
    private void OnEnable()
    {
        TileController.OnTileClicked += MoveCurrentPlayerUnit;
    }
    private void OnDisable()
    {
        TileController.OnTileClicked -= MoveCurrentPlayerUnit;
    }

    public void GenerateGridMap()
    {
        Debug.Log("Generating Grid Map");
        for (int x = 0; x < gridHorizontalSize; x++)
        {
            for (int y = 0; y < gridVerticalSize; y++)
            {
                Vector3 tilePosition = new Vector3(x * (1 + inBetweenTilesXOffset), 0, y * (1 + inBetweenTilesYOffset));
                GameObject tilePrefabInstance = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(0, 0, 0));
                TileController tileController = tilePrefabInstance.GetComponent<TileController>();
                tileController.tileXCoordinate = x;
                tileController.tileYCoordinate = y;
                PositionKey positionKey = new PositionKey(x, y, tilePrefab);
                Debug.Log(gridMapDictionary);

                //Adds the Tile GameObject to the dictionary
                if (!gridMapDictionary.ContainsKey(positionKey))
                {
                    gridMapDictionary.Add(positionKey, tilePrefabInstance.GetComponent<TileController>());
                }
                else
                {
                    Debug.LogWarning("Duplicate key found when adding GameObject to dictionary!");
                }
                Debug.Log("Dictionary Count: " + gridMapDictionary.Count);
                foreach (var entry in gridMapDictionary)
                {
                    Debug.Log($"PositionKey: {entry.Key.indexTileXPosition}, {entry.Key.indexTileYPosition}, GameObject: {entry.Value.name}");
                }
            }
        }
    }

    // This method retrieves a TileController Instance based on X and Y coordinates

    public TileController GetTileControllerInstance(int xCoordinate, int yCoordinate)
    {
        // Create a PositionKey with the given X and Y coordinates
        PositionKey positionKeyToFind = new PositionKey(xCoordinate, yCoordinate, null);

        // Use TryGetValue to check if the key exists in the Grid Map Dictionary
        if (gridMapDictionary.TryGetValue(positionKeyToFind, out TileController result))
        {
            if (result != null)
            {
                // Gives back the indexed Tile Controller instance as a result
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
                // Result GameObject is null
                Debug.LogError("GameObject is null.");
                return null;
            }
        }
        else
        {
            // Key not found, return null
            Debug.Log("Key not found");
            return null;
        }
    }
    public void MoveCurrentPlayerUnit(int targetX, int targetY)
    {
        currentPlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        currentPlayerUnit.GetComponent<Unit>().MoveUnit(targetX, targetY);
        TileController finalDestinationTile = GetTileControllerInstance(targetX, targetY);
        if (currentPlayerUnit.GetComponent<Unit>().ownedTile == null)
        {
            currentPlayerUnit.GetComponent<Unit>().ownedTile = finalDestinationTile;
            currentPlayerUnit.GetComponent<Unit>().ownedTile.detectedUnit = currentPlayerUnit;
            currentPlayerUnit.GetComponent<Unit>().ownedTile.currentSingleTileCondition = SingleTileCondition.occupied;
        }
        else
        {
            currentPlayerUnit.GetComponent<Unit>().ownedTile.currentSingleTileCondition = SingleTileCondition.free;
            currentPlayerUnit.GetComponent<Unit>().ownedTile.detectedUnit = null;
            currentPlayerUnit.GetComponent<Unit>().ownedTile = finalDestinationTile;
            currentPlayerUnit.GetComponent<Unit>().ownedTile.detectedUnit = currentPlayerUnit;
            currentPlayerUnit.GetComponent<Unit>().ownedTile.currentSingleTileCondition = SingleTileCondition.occupied;
        }
        Debug.Log("Moving Player Unit to (" + targetX + ", " + targetY + ")");
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
}
