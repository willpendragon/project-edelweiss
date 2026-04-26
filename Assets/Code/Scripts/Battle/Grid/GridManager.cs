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

        Vector3 tileSize = GetTileWorldSize3D();

        foreach (var tileData in currentMapData.tilePositions)
        {
            Vector3 tilePosition = new Vector3(
                tileData.position.x * (tileSize.x + inBetweenTilesXOffset),
                tileData.position.y * tileSize.y,
                tileData.position.z * (tileSize.z + inBetweenTilesYOffset)
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

            Transform gridBounds = tilePrefabInstance.transform.Find("GridBounds");
            if (gridBounds != null)
            {
                Renderer boundsRenderer = gridBounds.GetComponent<Renderer>();
                if (boundsRenderer != null)
                {
                    boundsRenderer.enabled = false;
                }
            }

            if (tileData.tileType == TileType.Chest || tileData.tileType == TileType.MinibossChest || tileData.tileType == TileType.BossChest)
            {
                GameObject chestPrototype = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chestPrototype.name = "Chest_Prototype";

                BoxCollider prototypeCollider = chestPrototype.GetComponent<BoxCollider>();
                if (prototypeCollider != null) Destroy(prototypeCollider);

                Renderer renderer = chestPrototype.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (tileData.tileType == TileType.MinibossChest) renderer.material.color = Color.yellow;
                    else if (tileData.tileType == TileType.BossChest) renderer.material.color = Color.red;
                    else renderer.material.color = new Color(0.5f, 0.0f, 0.8f, 1f);
                }

                chestPrototype.transform.SetParent(tilePrefabInstance.transform);
                chestPrototype.transform.localScale = Vector3.one * 0.5f; 
                PlaceUnitOnTileSurface(chestPrototype, tileController);

                tileController.detectedUnit = chestPrototype;
                tileController.currentSingleTileCondition = SingleTileCondition.occupied;

                chestPrototype.tag = "Chest";

                if (!chestPrototype.GetComponent<ChestUnit>())
                {
                    var chestUnit = chestPrototype.AddComponent<ChestUnit>();
                    chestUnit.HealthPoints = 10;
                    chestUnit.currentUnitLifeCondition = Unit.UnitLifeCondition.unitAlive;
                    chestUnit.ownedTile = tileController;
                    chestUnit.bossFlag = false;
                    
                    if (!chestPrototype.GetComponent<PrizeReleaseController>())
                    {
                        var releaseController = chestPrototype.AddComponent<PrizeReleaseController>();
                        chestUnit.fieldPrizeController = releaseController;
                    }
                    
                    chestUnit.currentXCoordinate = tileData.position.x;
                    chestUnit.currentYCoordinate = tileData.position.z;

                    if (tileData.tileType == TileType.MinibossChest)
                        chestUnit.unitTemplate = Resources.Load<ChestTemplate>("MinibossChestTemplate");
                    else if (tileData.tileType == TileType.BossChest)
                        chestUnit.unitTemplate = Resources.Load<ChestTemplate>("BossChestTemplate");
                }
            }
        }

        if (currentMapData.decorationPositions != null)
        {
            foreach (var decoData in currentMapData.decorationPositions)
            {
                string targetPrefabName = string.IsNullOrEmpty(decoData.prefabName) ? "DecorationPrefab" : decoData.prefabName;

                GameObject runtimeDecorationPrefab = Resources.Load<GameObject>(targetPrefabName); 
                
                if (runtimeDecorationPrefab == null)
                {
                    Debug.LogWarning($"GridManager: Missing Decoration Prefab '{targetPrefabName}' in Resources folder!");
                    continue; 
                }

                Vector3 decoPosition = new Vector3(
                    decoData.position.x * (tileSize.x + inBetweenTilesXOffset),
                    decoData.position.y * tileSize.y,
                    decoData.position.z * (tileSize.z + inBetweenTilesYOffset)
                );

                // Only this single line changes to preserve the prefab's rotation
                GameObject decoInstance = Instantiate(runtimeDecorationPrefab, decoPosition, runtimeDecorationPrefab.transform.rotation);

                decoInstance.transform.SetParent(this.transform); 

                Transform gridBounds = decoInstance.transform.Find("GridBounds");
                if (gridBounds != null)
                {
                    Renderer boundsRenderer = gridBounds.GetComponent<Renderer>();
                    if (boundsRenderer != null) boundsRenderer.enabled = false;
                }

                TileController occupiedTile = GetTileControllerInstance(decoData.position.x, decoData.position.y - 1, decoData.position.z);
                if (occupiedTile == null) occupiedTile = GetTileControllerInstance(decoData.position.x, decoData.position.y, decoData.position.z);

                if (occupiedTile != null)
                {
                    occupiedTile.currentSingleTileCondition = SingleTileCondition.occupied;
                    occupiedTile.tileType = TileType.Environment; 
                    occupiedTile.detectedUnit = decoInstance; 
                }
            }
        }
        else if (currentMapData.decorationPositions != null && currentMapData.decorationPositions.Count > 0)
        {
            Debug.LogWarning("GridManager: Ti manca un prefab per le decorazioni! Assicurati di posizionare 'DecorationPrefab' nella cartella Resources.");
        }

        // --- NEW: Load Player Spawn Points ---
        if (currentMapData.playerSpawnPositions != null && currentMapData.playerSpawnPositions.Count > 0)
        {
            List<GameObject> dynamicPlayerUnits = new List<GameObject>();

            foreach (var spawnData in currentMapData.playerSpawnPositions)
            {
                if (string.IsNullOrEmpty(spawnData.prefabName)) continue;

                TileController targetTile = GetTileControllerInstance(spawnData.position.x, spawnData.position.y, spawnData.position.z);
                if (targetTile == null) targetTile = GetTileControllerInstance(spawnData.position.x, spawnData.position.y - 1, spawnData.position.z);

                if (targetTile != null)
                {
                    // 1. Load the painted prefab to inspect its true underlying Template Data
                    GameObject runtimeUnitPrefab = Resources.Load<GameObject>(spawnData.prefabName);

                    if (runtimeUnitPrefab == null)
                    {
                        Debug.LogWarning($"GridManager: Missing Painted Unit Prefab '{spawnData.prefabName}' in Resources folder!");
                        continue;
                    }

                    // Extract the core UnitName from the Prefab's template
                    Unit prefabUnitComponent = runtimeUnitPrefab.GetComponent<Unit>();
                    string paintedUnitName = (prefabUnitComponent != null && prefabUnitComponent.unitTemplate != null) 
                        ? prefabUnitComponent.unitTemplate.unitName 
                        : string.Empty;

                    // 2. Try to find a match in the persistent Global Player Party using ONLY the unitTemplate.unitName
                    Unit persistentPlayerMatch = null;
                    if (!string.IsNullOrEmpty(paintedUnitName) && GameManager.Instance != null && GameManager.Instance.playerPartyMembersInstances != null)
                    {
                        persistentPlayerMatch = GameManager.Instance.playerPartyMembersInstances.FirstOrDefault(u => 
                            u != null && u.unitTemplate != null && u.unitTemplate.unitName == paintedUnitName
                        );
                    }

                    if (persistentPlayerMatch != null)
                    {
                        // Match Found! Teleport the persistent player unit to the painted map coordinates
                        GameObject unitInstance = persistentPlayerMatch.gameObject;
                        unitInstance.SetActive(true); 
                        
                        PlaceUnitOnTileSurface(unitInstance, targetTile);

                        targetTile.currentSingleTileCondition = SingleTileCondition.occupied;
                        targetTile.detectedUnit = unitInstance;

                        persistentPlayerMatch.startingXCoordinate = spawnData.position.x;
                        persistentPlayerMatch.startingYCoordinate = spawnData.position.z;
                        persistentPlayerMatch.currentXCoordinate = spawnData.position.x;
                        persistentPlayerMatch.currentYCoordinate = spawnData.position.z;
                        persistentPlayerMatch.ownedTile = targetTile;

                        dynamicPlayerUnits.Add(unitInstance);
                    }
                    else
                    {
                        // No player match found. It's an Enemy or a Deity painted on the map! Instantiating it.
                        GameObject unitInstance = Instantiate(runtimeUnitPrefab, this.transform);
                        Unit paintedEnemy = unitInstance.GetComponent<Unit>();
                        
                        if (paintedEnemy != null)
                        {
                            // Force template load before Enum evaluation to prevent fake Game-Over
                            if (paintedEnemy.unitTemplate != null) paintedEnemy.RetrieveTemplateValues();

                            PlaceUnitOnTileSurface(unitInstance, targetTile);
                            targetTile.currentSingleTileCondition = SingleTileCondition.occupied;
                            targetTile.detectedUnit = unitInstance;

                            paintedEnemy.startingXCoordinate = spawnData.position.x;
                            paintedEnemy.startingYCoordinate = spawnData.position.z;
                            paintedEnemy.currentXCoordinate = spawnData.position.x;
                            paintedEnemy.currentYCoordinate = spawnData.position.z;
                            paintedEnemy.ownedTile = targetTile;
                        }
                    }
                }
            }

            // --- OVERRIDE HARDCODED LOGIC ---
            if (dynamicPlayerUnits.Count > 0)
            {
                // Instantly deactivate any old legacy hardcoded scene units so they don't interfere
                GameObject[] allScenePlayers = GameObject.FindGameObjectsWithTag("Player");
                foreach (var legacyPlayer in allScenePlayers)
                {
                    if (legacyPlayer != null && !dynamicPlayerUnits.Contains(legacyPlayer))
                    {
                        legacyPlayer.SetActive(false); 
                    }
                }

                GameObject partyControllerObj = GameObject.FindGameObjectWithTag("PlayerPartyController");
                if (partyControllerObj != null)
                {
                    // Update PlayerPartyController to use ONLY what the map data dictates
                    var partyController = partyControllerObj.GetComponent<PlayerPartyController>();
                    if (partyController != null)
                    {
                        partyController.playerUnitsOnBattlefield = dynamicPlayerUnits.ToArray();
                    }
                }

                // STABILIZER: Because GameStatsManager relies on Start(), 
                // we must manually invoke it here to pour the saved depleted 
                // HP, Mana, and Faith into these freshly deployed player units.
                GameStatsManager statsManager = FindObjectOfType<GameStatsManager>();
                if (statsManager != null)
                {
                    statsManager.LoadCharacterData();
                }
            }
        } // <--- End of if (currentMapData.playerSpawnPositions != null)

        // --- NEW: Load Painted Enemy Spawn Points ---
        if (currentMapData.enemySpawnPositions != null && currentMapData.enemySpawnPositions.Count > 0)
        {
            foreach (var spawnData in currentMapData.enemySpawnPositions)
            {
                if (string.IsNullOrEmpty(spawnData.prefabName)) continue;

                TileController targetTile = GetTileControllerInstance(spawnData.position.x, spawnData.position.y, spawnData.position.z);
                if (targetTile == null) targetTile = GetTileControllerInstance(spawnData.position.x, spawnData.position.y - 1, spawnData.position.z);

                if (targetTile != null)
                {
                    GameObject runtimeEnemyPrefab = Resources.Load<GameObject>(spawnData.prefabName);
                    if (runtimeEnemyPrefab != null)
                    {
                        GameObject enemyInstance = Instantiate(runtimeEnemyPrefab, this.transform);
                        Unit paintedEnemy = enemyInstance.GetComponent<Unit>();

                        if (paintedEnemy != null)
                        {
                            if (paintedEnemy.unitTemplate != null) paintedEnemy.RetrieveTemplateValues();

                            PlaceUnitOnTileSurface(enemyInstance, targetTile);
                            
                            // CLASH PREVENTION: This unequivocally locks out procedural gen from picking this tile
                            targetTile.currentSingleTileCondition = SingleTileCondition.occupied;
                            targetTile.detectedUnit = enemyInstance;

                            paintedEnemy.startingXCoordinate = spawnData.position.x;
                            paintedEnemy.startingYCoordinate = spawnData.position.z;
                            paintedEnemy.currentXCoordinate = spawnData.position.x;
                            paintedEnemy.currentYCoordinate = spawnData.position.z;
                            paintedEnemy.ownedTile = targetTile;
                        }
                    }
                }
            }
        }

        // --- NEW: Spawn Beacons ---
        //if (currentMapData.beaconPositions != null && currentMapData.beaconPositions.Count > 0)
        //{
        //    GameObject runtimeBeaconPrefab = Resources.Load<GameObject>("Beacon");

        //    if (runtimeBeaconPrefab == null)
        //    {
        //        Debug.LogWarning("GridManager: Missing 'Beacon' Prefab in Resources folder! Beacons could not be spawned.");
        //    }
        //    else
        //    {
        //        foreach (var beaconPos in currentMapData.beaconPositions)
        //        {
        //            TileController targetTile = GetTileControllerInstance(beaconPos.x, beaconPos.y, beaconPos.z);
        //            if (targetTile == null) targetTile = GetTileControllerInstance(beaconPos.x, beaconPos.y - 1, beaconPos.z);

        //            if (targetTile != null)
        //            {
        //                GameObject beaconInstance = Instantiate(runtimeBeaconPrefab, this.transform);
        //                PlaceUnitOnTileSurface(beaconInstance, targetTile);

        //                targetTile.currentSingleTileCondition = SingleTileCondition.occupied;
        //                targetTile.detectedUnit = beaconInstance;

        //                // Se hai uno script agganciato al Beacon che necessita info, puoi inizializzarlo qui.
        //                // Esempio:
        //                // BeaconController beaconScript = beaconInstance.GetComponent<BeaconController>();
        //                // if (beaconScript != null) { beaconScript.InitializeOnTile(targetTile); }
        //            }
        //        }
        //    }
        //}

        // --- NEW: Spawn Interactables ---
        if (currentMapData.interactablePositions != null && currentMapData.interactablePositions.Count > 0)
        {
            foreach (var intData in currentMapData.interactablePositions)
            {
                if (string.IsNullOrEmpty(intData.prefabName)) continue;

                GameObject runtimeInteractablePrefab = Resources.Load<GameObject>(intData.prefabName);

                if (runtimeInteractablePrefab == null)
                {
                    Debug.LogWarning($"GridManager: Missing '{intData.prefabName}' Prefab in Resources folder! Could not be spawned.");
                    continue;
                }

                TileController targetTile = GetTileControllerInstance(intData.position.x, intData.position.y, intData.position.z);
                if (targetTile == null) targetTile = GetTileControllerInstance(intData.position.x, intData.position.y - 1, intData.position.z);

                if (targetTile != null)
                {
                    // --- MAP PAINTED TRAPS ---
                    if (intData.prefabName.Contains("Trap"))
                    {
                        // 1. Find the native TrapController already living on the TileController prefab
                        TrapController tileNativeTrapController = targetTile.GetComponentInChildren<TrapController>();
                        if (tileNativeTrapController != null)
                        {
                            // 2. Activate the native trap logic
                            tileNativeTrapController.currentTrapActivationStatus = TrapController.TrapActivationStatus.active;
                            
                            // 3. Spawn the Trap visuals with your desired offset, parented to the tile to keep hierarchy clean
                            Vector3 offSet = new Vector3(0, 2f, 0);
                            Vector3 spawnPosition = targetTile.transform.position + offSet;
                            
                            Instantiate(runtimeInteractablePrefab, spawnPosition, Quaternion.identity, targetTile.transform);
                        }

                        // We skip setting the tile to 'occupied' so units can still physically path onto the trap tile!
                        continue;
                    }

                    // STANDARD INTERACTABLES
                    GameObject interactableInstance = Instantiate(runtimeInteractablePrefab, this.transform);
                    PlaceUnitOnTileSurface(interactableInstance, targetTile);

                    targetTile.currentSingleTileCondition = SingleTileCondition.occupied;
                    targetTile.detectedUnit = interactableInstance;
                }
            }
        }

        // --- NEW: Load Freeform Environment Props ---
        if (currentMapData.environmentPositions != null && currentMapData.environmentPositions.Count > 0)
        {
            foreach (var envData in currentMapData.environmentPositions)
            {
                if (string.IsNullOrEmpty(envData.prefabName)) continue;

                GameObject runtimeEnvPrefab = Resources.Load<GameObject>(envData.prefabName);

                if (runtimeEnvPrefab == null)
                {
                    Debug.LogWarning($"GridManager: Missing Environment Prefab '{envData.prefabName}' in Resources folder!");
                    continue;
                }

                // Spawn entirely decoupled from the Grid offsets, using true World Space coordinates
                GameObject envInstance = Instantiate(runtimeEnvPrefab, envData.position, Quaternion.Euler(envData.rotation));
                envInstance.transform.localScale = envData.scale;
                
                envInstance.transform.SetParent(this.transform); // Keep hierarchy clean
            }
        }

        // --- NEW: Load Freeform Environment Props ---
        if (currentMapData.environmentPositions != null && currentMapData.environmentPositions.Count > 0)
        {
            foreach (var envData in currentMapData.environmentPositions)
            {
                if (string.IsNullOrEmpty(envData.prefabName)) continue;

                GameObject runtimeEnvPrefab = Resources.Load<GameObject>(envData.prefabName);

                if (runtimeEnvPrefab == null)
                {
                    Debug.LogWarning($"GridManager: Missing Environment Prefab '{envData.prefabName}' in Resources folder!");
                    continue;
                }

                // Spawn entirely decoupled from the Grid offsets, using true World Space coordinates
                GameObject envInstance = Instantiate(runtimeEnvPrefab, envData.position, Quaternion.Euler(envData.rotation));
                envInstance.transform.localScale = envData.scale;
                
                envInstance.transform.SetParent(this.transform); // Keep hierarchy clean
            }
        }

        // --- NEW: Load Programmatic Lights ---
        if (currentMapData.lightSettings != null && currentMapData.lightSettings.Count > 0)
        {
            foreach (var lightData in currentMapData.lightSettings)
            {
                // Create an empty GameObject dynamically
                GameObject lightInstance = new GameObject("RuntimeMapLight");
                lightInstance.transform.position = lightData.position;
                lightInstance.transform.rotation = Quaternion.Euler(lightData.rotation);
                lightInstance.transform.SetParent(this.transform);

                // Add and configure the Light component programmatically
                Light runtimeLight = lightInstance.AddComponent<Light>();
                runtimeLight.type = lightData.type;
                runtimeLight.color = lightData.color;
                runtimeLight.intensity = lightData.intensity;
                runtimeLight.range = lightData.range;
                
                if (lightData.type == LightType.Spot)
                {
                    runtimeLight.spotAngle = lightData.spotAngle;
                }
            }
        }

        // --- NEW: Apply Directional Light Config ---
        if (currentMapData.overrideDirectionalLight)
        {
            // Find the main directional light in the Battle Scene
            Light sceneDirectionalLight = FindObjectsOfType<Light>().FirstOrDefault(l => l.type == LightType.Directional);
            
            if (sceneDirectionalLight != null)
            {
                sceneDirectionalLight.transform.rotation = Quaternion.Euler(currentMapData.directionalLightRotation);
                sceneDirectionalLight.color = currentMapData.directionalLightColor;
                sceneDirectionalLight.intensity = currentMapData.directionalLightIntensity;
            }
            else
            {
                Debug.LogWarning("GridManager: overrideDirectionalLight is true, but no Directional Light exists in the Battle Scene!");
            }
        }

    } // <--- End of GenerateGridMapFromData()

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