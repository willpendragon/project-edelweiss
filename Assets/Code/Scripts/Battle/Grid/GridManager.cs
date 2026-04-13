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

        Vector3 tileSize = GetTileWorldSize3D(); // USA L'HELPER!

        foreach (var tileData in currentMapData.tilePositions)
        {
            Vector3 tilePosition = new Vector3(
                tileData.position.x * (tileSize.x + inBetweenTilesXOffset),
                tileData.position.y * tileSize.y,
                tileData.position.z * (tileSize.z + inBetweenTilesYOffset) // Uso di Z per la profondità
            );

            GameObject tilePrefabInstance = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
            TileController tileController = tilePrefabInstance.GetComponent<TileController>();

            tileController.gridPosition = tileData.position;
            tileController.tileType = tileData.tileType;

            PositionKey positionKey = new PositionKey(tileData.position, tilePrefab);

            if (!gridMapDictionary.ContainsKey(positionKey))
            {
                gridMapDictionary.Add(positionKey, tileController);
            }

            // --- NEW: Hide GridBounds during gameplay for ALL physical Map tiles ---
            Transform gridBounds = tilePrefabInstance.transform.Find("GridBounds");
            if (gridBounds != null)
            {
                Renderer boundsRenderer = gridBounds.GetComponent<Renderer>();
                if (boundsRenderer != null)
                {
                    boundsRenderer.enabled = false;
                }
            }

            // [PROTOTYPE] Spawn a purple Cube to represent the chest directly on the generated tile
            if (tileData.tileType == TileType.Chest || tileData.tileType == TileType.MinibossChest || tileData.tileType == TileType.BossChest)
            {
                GameObject chestPrototype = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chestPrototype.name = "Chest_Prototype";

                // Remove the collider so it doesn't block tile raycasts
                BoxCollider prototypeCollider = chestPrototype.GetComponent<BoxCollider>();
                if (prototypeCollider != null)
                {
                    Destroy(prototypeCollider);
                }

                // Set the color to distinguish it based on type
                Renderer renderer = chestPrototype.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (tileData.tileType == TileType.MinibossChest)
                        renderer.material.color = Color.yellow;
                    else if (tileData.tileType == TileType.BossChest)
                        renderer.material.color = Color.red;
                    else
                        renderer.material.color = new Color(0.5f, 0.0f, 0.8f, 1f); // Default Purple
                }

                // Parent it to the tile and position it firmly on the surface
                chestPrototype.transform.SetParent(tilePrefabInstance.transform);
                chestPrototype.transform.localScale = Vector3.one * 0.5f; // Small size cube
                PlaceUnitOnTileSurface(chestPrototype, tileController);

                // Make the tile mathematically occupied by the chest so units can't walk on it
                tileController.detectedUnit = chestPrototype;
                tileController.currentSingleTileCondition = SingleTileCondition.occupied;

                // Tag it properly as a Chest
                chestPrototype.tag = "Chest";

                // Add a custom ChestUnit component to handle specific chest logic
                if (!chestPrototype.GetComponent<ChestUnit>())
                {
                    var chestUnit = chestPrototype.AddComponent<ChestUnit>();
                    chestUnit.HealthPoints = 10;
                    chestUnit.currentUnitLifeCondition = Unit.UnitLifeCondition.unitAlive;
                    chestUnit.ownedTile = tileController;
                    chestUnit.bossFlag = false;
                    
                    // Construct local PrizeReleaseController purely to handle the immediate drop sequence
                    if (!chestPrototype.GetComponent<PrizeReleaseController>())
                    {
                        var releaseController = chestPrototype.AddComponent<PrizeReleaseController>();
                        chestUnit.fieldPrizeController = releaseController;
                    }
                    

                    // Sync logical coordinates so battle initialization scripts don't reset it to 0,0
                    chestUnit.currentXCoordinate = tileData.position.x;
                    chestUnit.currentYCoordinate = tileData.position.z;

                    // TODO: Assign the specific Scriptable Object Template here based on tileData.tileType!
                    if (tileData.tileType == TileType.MinibossChest)
                        chestUnit.unitTemplate = Resources.Load<ChestTemplate>("MinibossChestTemplate");
                    else if (tileData.tileType == TileType.BossChest)
                        chestUnit.unitTemplate = Resources.Load<ChestTemplate>("BossChestTemplate");
                }
            }
        }

        // --- NEW: Load Decorations at runtime ---
        GameObject runtimeDecorationPrefab = Resources.Load<GameObject>("DecorationPrefab"); 
        
        if (runtimeDecorationPrefab != null && currentMapData.decorationPositions != null)
        {
            foreach (var decoData in currentMapData.decorationPositions)
            {
                Vector3 decoPosition = new Vector3(
                    decoData.position.x * (tileSize.x + inBetweenTilesXOffset),
                    decoData.position.y * tileSize.y,
                    decoData.position.z * (tileSize.z + inBetweenTilesYOffset)
                );

                GameObject decoInstance = Instantiate(runtimeDecorationPrefab, decoPosition, Quaternion.identity);
                decoInstance.transform.SetParent(this.transform); // Raggruppa sotto al GridManager per pulizia

                // --- NEW: Hide GridBounds during gameplay ---
                Transform gridBounds = decoInstance.transform.Find("GridBounds");
                if (gridBounds != null)
                {
                    Renderer boundsRenderer = gridBounds.GetComponent<Renderer>();
                    if (boundsRenderer != null)
                        boundsRenderer.enabled = false; // Makes it invisible but keeps colliders intact!
                }

                // --- Block movement on the tile ---
                // Decoration is visually "on top" of the tile, so it exists at Y + 1 according to the save data
                TileController occupiedTile = GetTileControllerInstance(decoData.position.x, decoData.position.y - 1, decoData.position.z);
                
                // Fallback: Just in case the decoration was saved at the exact same Y coordinate as the base tile
                if (occupiedTile == null)
                {
                    occupiedTile = GetTileControllerInstance(decoData.position.x, decoData.position.y, decoData.position.z);
                }

                // If we found the base walkable tile under the decoration, mark it as a physical obstacle
                if (occupiedTile != null)
                {
                    occupiedTile.currentSingleTileCondition = SingleTileCondition.occupied;
                    occupiedTile.tileType = TileType.Environment; // Set explicitly to Environment
                    occupiedTile.detectedUnit = decoInstance; 
                }
            }
        }
        else if (currentMapData.decorationPositions != null && currentMapData.decorationPositions.Count > 0)
        {
            Debug.LogWarning("GridManager: Ti manca un prefab per le decorazioni! Assicurati di posizionare 'DecorationPrefab' nella cartella Resources.");
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
        // Partiamo da un'elevazione massima (es. 20 blocchi di altezza) e scendiamo
        // finché non troviamo il primo blocco fisico esistente.
        for (int y = 20; y >= 0; y--)
        {
            TileController tile = GetTileControllerInstance(xCoordinate, y, zOrOldYCoordinate);
            if (tile != null)
            {
                return tile; // Trovato! Restituiamo il Voxel sulla cima della collina!
            }
        }

        return null; // Nessun tile in questa colonna
    }
    public List<Vector2Int> GetExistingTileCoordinates()
    {
        List<Vector2Int> existingTiles = new List<Vector2Int>();
        foreach (var key in gridMapDictionary.Keys)
        {
            // indexTileZPosition corresponds to the logical Y coordinate in your 2D top-down grid scripts
            existingTiles.Add(new Vector2Int(key.indexTileXPosition, key.indexTileZPosition));
        }

        // Remove duplicates if multiple Voxel tiles are stacked on the same (X, Z) coordinate column
        return existingTiles.Distinct().ToList();
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
    public Vector3 GetWorldPositionFromGridCoordinates(int x, int z)
    {
        for (int y = 20; y >= 0; y--)
        {
            TileController tile = GetTileControllerInstance(x, y, z);
            if (tile != null)
            {
                float surfaceY = tile.transform.position.y;
                BoxCollider col = tile.GetComponent<BoxCollider>();
                if (col != null)
                {
                    surfaceY += col.bounds.extents.y;
                }

                return new Vector3(tile.transform.position.x, surfaceY, tile.transform.position.z);
            }
        }

        // CORREZIONE AL FALLBACK
        Vector3 tileSize = GetTileWorldSize3D();
        float worldX = x * (tileSize.x + inBetweenTilesXOffset);
        float worldZ = z * (tileSize.z + inBetweenTilesYOffset);
        return new Vector3(worldX, 0, worldZ);
    }
    public Vector2Int GetGridCoordinatesFromWorldPosition(Vector3 worldPosition)
    {
        // CORREZIONE DELLA VETTORIZZZAZIONE INVERSA
        Vector3 tileSize = GetTileWorldSize3D();
        int x = Mathf.RoundToInt(worldPosition.x / (tileSize.x + inBetweenTilesXOffset));
        int y = Mathf.RoundToInt(worldPosition.z / (tileSize.z + inBetweenTilesYOffset));

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

    /// <summary>
    /// Posiziona un GameObject (es. Unità) esattamente sopra la superficie calpestabile di un Tile,
    /// compensando in automatico qualsiasi offset errato o pivot strano dei figli (SpriteRenderer).
    /// </summary>
    public void PlaceUnitOnTileSurface(GameObject unitToPlace, TileController targetTile)
    {
        if (unitToPlace == null || targetTile == null) return;

        // Anziché usare posizioni relative o bounds, usiamo la cima esatta del Renderer o del Collider!
        float finalY = targetTile.transform.position.y;

        Collider col = targetTile.GetComponent<Collider>();
        if (col != null)
        {
            // bounds.max.y ti dà il punto ASSOLUTO in altezza top nello spazio mondo di QUEL cubo!
            finalY = col.bounds.max.y;
        }

        Debug.Log($"[VOXEL MATH] Sposto {unitToPlace.name} ad altezza reale Y = {finalY}");

        unitToPlace.transform.position = new Vector3(
            targetTile.transform.position.x,
            finalY,
            targetTile.transform.position.z
        );
    }

    public Vector3 GetTileWorldSize3D()
    {
        if (tilePrefab == null) return Vector3.one;
        BoxCollider col = tilePrefab.GetComponent<BoxCollider>();
        if (col != null)
        {
            // Moltiplica la grandezza pura per la scala del Prefab! (Es: 1 * 1.5 = 1.5)
            return Vector3.Scale(col.size, tilePrefab.transform.localScale);
        }
        return tilePrefab.transform.localScale;
    }
}