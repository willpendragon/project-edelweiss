using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class MapEditorWindow
{
    // --- NEW: Directional Light Config ---
    [Header("Directional Light")]
    public bool overrideDirectionalLight = false;
    public Vector3 directionalLightRotation = new Vector3(50f, -30f, 0f);
    public Color directionalLightColor = Color.white;
    public float directionalLightIntensity = 1f;

    private void SaveDecoPrefabsToPrefs() => EditorPrefs.SetString("MapEditor_DecoPrefabs", string.Join(";", decorationPrefabs.Where(p => p != null).Select(AssetDatabase.GetAssetPath)));
    private void SaveUnitPrefabsToPrefs() => EditorPrefs.SetString("MapEditor_UnitPrefabs", string.Join(";", unitPrefabs.Where(p => p != null).Select(AssetDatabase.GetAssetPath)));
    private void SaveInteractablePrefabsToPrefs() => EditorPrefs.SetString("MapEditor_InteractablePrefabs", string.Join(";", interactablePrefabs.Where(p => p != null).Select(AssetDatabase.GetAssetPath)));
    private void SaveEnemyPrefabsToPrefs() => EditorPrefs.SetString("MapEditor_EnemyPrefabs", string.Join(";", enemyPrefabs.Where(p => p != null).Select(AssetDatabase.GetAssetPath)));
    private void SaveEnvironmentPrefabsToPrefs() => EditorPrefs.SetString("MapEditor_EnvPrefabs", string.Join(";", environmentPrefabs.Where(p => p != null).Select(AssetDatabase.GetAssetPath)));

    private void LoadDecoPrefabsFromPrefs()
    {
        decorationPrefabs.Clear();
        foreach (var p in EditorPrefs.GetString("MapEditor_DecoPrefabs", "").Split(';'))
        {
            var f = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (f != null) decorationPrefabs.Add(f);
        }
        if (decorationPrefabs.Count > 0) decorationPrefab = decorationPrefabs[0];

        unitPrefabs.Clear();
        foreach (var p in EditorPrefs.GetString("MapEditor_UnitPrefabs", "").Split(';'))
        {
            var f = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (f != null) unitPrefabs.Add(f);
        }
        if (unitPrefabs.Count > 0) unitPrefab = unitPrefabs[0];

        interactablePrefabs.Clear();
        foreach (var p in EditorPrefs.GetString("MapEditor_InteractablePrefabs", "").Split(';'))
        {
            var f = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (f != null) interactablePrefabs.Add(f);
        }
        if (interactablePrefabs.Count > 0) interactablePrefab = interactablePrefabs[0];

        enemyPrefabs.Clear();
        foreach (var p in EditorPrefs.GetString("MapEditor_EnemyPrefabs", "").Split(';'))
        {
            var f = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (f != null) enemyPrefabs.Add(f);
        }
        if (enemyPrefabs.Count > 0) enemyPrefab = enemyPrefabs[0];

        environmentPrefabs.Clear();
        foreach (var p in EditorPrefs.GetString("MapEditor_EnvPrefabs", "").Split(';'))
        {
            var f = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (f != null) environmentPrefabs.Add(f);
        }
        if (environmentPrefabs.Count > 0) environmentPrefab = environmentPrefabs[0];
    }

    private void SaveMap()
    {
        if (currentMap == null) return;
        SyncDictionaryFromScene();

        currentMap.tilePositions.Clear();
        foreach (var kvp in tiles)
        {
            var ctrl = kvp.Value.GetComponent<TileController>();
            currentMap.tilePositions.Add(new MapData.TileData { position = kvp.Key, tileType = ctrl ? ctrl.tileType : TileType.Basic });
        }

        currentMap.decorationPositions.Clear();
        foreach (var kvp in decorations)
        {
            var src = PrefabUtility.GetCorrespondingObjectFromSource(kvp.Value);
            currentMap.decorationPositions.Add(new MapData.DecorationData { position = kvp.Key, prefabName = src != null ? src.name : kvp.Value.name.Replace("(Clone)", "").Trim() });
        }

        currentMap.playerSpawnPositions.Clear();
        foreach (var kvp in spawnedUnits)
        {
            var src = PrefabUtility.GetCorrespondingObjectFromSource(kvp.Value);
            currentMap.playerSpawnPositions.Add(new MapData.SpawnData { position = kvp.Key, prefabName = src != null ? src.name : kvp.Value.name.Replace("(Clone)", "").Trim() });
        }

        currentMap.enemySpawnPositions.Clear();
        foreach (var kvp in spawnedEnemies)
        {
            var src = PrefabUtility.GetCorrespondingObjectFromSource(kvp.Value);
            currentMap.enemySpawnPositions.Add(new MapData.SpawnData { position = kvp.Key, prefabName = src != null ? src.name : kvp.Value.name.Replace("(Clone)", "").Trim() });
        }

        // INTERACTABLES SAVING
        currentMap.interactablePositions.Clear();
        foreach (var kvp in spawnedInteractables)
        {
            var src = PrefabUtility.GetCorrespondingObjectFromSource(kvp.Value);
            int id = kvp.Value.GetComponent<InteractableLink>()?.linkID ?? 0; // Grab ID
            
            currentMap.interactablePositions.Add(new MapData.SpawnData
            {
                position = kvp.Key,
                prefabName = src != null ? src.name : kvp.Value.name.Replace("(Clone)", "").Replace("SpawnInteractable_", "").Split('_')[0].Trim(),
                linkID = id // Save ID
            });
        }

        currentMap.environmentPositions.Clear();
        foreach (var envObj in spawnedEnvironments)
        {
            if (envObj == null) continue;
            var src = PrefabUtility.GetCorrespondingObjectFromSource(envObj);
            string pName = src != null ? src.name : envObj.name.Replace("(Clone)", "").Split('_')[1].Trim();
            
            currentMap.environmentPositions.Add(new MapData.EnvironmentData 
            { 
                prefabName = pName,
                position = envObj.transform.position,
                rotation = envObj.transform.eulerAngles,
                scale = envObj.transform.localScale
            });
        }

        // --- NEW: Add Light Saving into the main Save button ---
        currentMap.lightSettings.Clear();
        var allLights = GameObject.FindObjectsOfType<Light>();
        
        foreach (var l in allLights)
        {
            // Only capture lights explicitly painted by the Editor tool
            if (l.gameObject.name.StartsWith("MapLight_"))
            {
                currentMap.lightSettings.Add(new MapData.LightData
                {
                    type = l.type,
                    position = l.transform.position,
                    rotation = l.transform.eulerAngles,
                    color = l.color,
                    intensity = l.intensity,
                    range = l.range,
                    spotAngle = l.spotAngle
                });
            }
        }
        // -------------------------------------------------------

        currentMap.horizontalSize = gridWidth;
        currentMap.depthSize = gridDepth;
        currentMap.verticalSize = gridHeight;

        EditorUtility.SetDirty(currentMap);
        AssetDatabase.SaveAssets();
    }

    private void LoadFromAsset()
    {
        if (currentMap == null) return;
        
        // --- NEW: Snap Map Editor Grid bounds to explicitly match the loaded file ---
        gridWidth = currentMap.horizontalSize;
        gridDepth = currentMap.depthSize;
        gridHeight = currentMap.verticalSize;
        // ----------------------------------------------------------------------------
        
        GenerateMap();

        Vector3 tileSize = GetTileWorldSize3D();

        // --- RESTORED TILE LOADING LOGIC ---
        if (tilePrefab != null && currentMap.tilePositions != null)
        {
            foreach (var data in currentMap.tilePositions)
            {
                GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab);
                tile.transform.position = GridToWorld(data.position, tileSize);
                tile.name = $"Tile_{data.position.x}_{data.position.y}_{data.position.z}";

                var ctrl = tile.GetComponent<TileController>();
                if (ctrl != null) { ctrl.tileType = data.tileType; ctrl.gridPosition = data.position; }

                tiles[data.position] = tile;
                HideTileEffects(tile);
                ApplyDecorativeColor(tile, data.tileType);
            }
        }
        // -----------------------------------

        // LOAD DECORATIONS
        if (currentMap.decorationPositions != null)
        {
            foreach (var data in currentMap.decorationPositions)
            {
                GameObject target = decorationPrefabs.FirstOrDefault(p => p != null && p.name == data.prefabName);
                if (target == null && !string.IsNullOrEmpty(data.prefabName)) target = Resources.Load<GameObject>(data.prefabName);
                if (target == null) target = decorationPrefab;

                if (target != null)
                {
                    GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(target);
                    obj.transform.position = GridToWorld(data.position, tileSize);
                    obj.name = $"{target.name}_{data.position.x}_{data.position.y}_{data.position.z}";
                    decorations[data.position] = obj;
                }
            }
        }

        // LOAD UNITS
        if (currentMap.playerSpawnPositions != null)
        {
            foreach (var data in currentMap.playerSpawnPositions)
            {
                GameObject target = unitPrefabs.FirstOrDefault(p => p != null && p.name == data.prefabName);
                if (target == null && !string.IsNullOrEmpty(data.prefabName)) target = Resources.Load<GameObject>(data.prefabName);
                if (target == null) target = unitPrefab;

                if (target != null)
                {
                    GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(target);
                    obj.transform.position = GridToWorld(data.position, tileSize);
                    // Add Required Prefix
                    obj.name = $"SpawnUnit_{target.name}_{data.position.x}_{data.position.y}_{data.position.z}";
                    spawnedUnits[data.position] = obj;
                }
            }
        }

        // LOAD ENEMIES
        if (currentMap.enemySpawnPositions != null)
        {
            foreach (var data in currentMap.enemySpawnPositions)
            {
                GameObject target = enemyPrefabs.FirstOrDefault(p => p != null && p.name == data.prefabName) ?? Resources.Load<GameObject>(data.prefabName) ?? enemyPrefab;
                if (target != null)
                {
                    GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(target);
                    obj.transform.position = GridToWorld(data.position, tileSize);
                    // Add Required Prefix
                    obj.name = $"SpawnEnemy_{target.name}_{data.position.x}_{data.position.y}_{data.position.z}";
                    spawnedEnemies[data.position] = obj;
                }
            }
        }

        // INTERACTABLES LOADING
        if (currentMap.interactablePositions != null)
        {
            foreach (var data in currentMap.interactablePositions)
            {
                // Try finding the prefab in the assigned Editor Pool
                GameObject target = interactablePrefabs.FirstOrDefault(p => p != null && p.name == data.prefabName);

                // Fallback to searching Resources folder
                if (target == null && !string.IsNullOrEmpty(data.prefabName)) target = Resources.Load<GameObject>(data.prefabName);

                // Fallback to active selected prefab in the Editor
                if (target == null) target = interactablePrefab;

                if (target != null)
                {
                    GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(target);
                    obj.transform.position = GridToWorld(data.position, tileSize);
                    obj.name = $"SpawnInteractable_{target.name}_{data.position.x}_{data.position.y}_{data.position.z}";
                    InteractableLink link = obj.GetComponent<InteractableLink>();
                    if (link != null) link.linkID = data.linkID; // Reapply ID
                    spawnedInteractables[data.position] = obj;
                }
            }
        }

        // LOAD ENVIRONMENTS
        if (currentMap.environmentPositions != null)
        {
            foreach (var data in currentMap.environmentPositions)
            {
                GameObject target = environmentPrefabs.FirstOrDefault(p => p != null && p.name == data.prefabName) ?? Resources.Load<GameObject>(data.prefabName) ?? environmentPrefab;
                if (target != null)
                {
                    GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(target);
                    obj.transform.position = data.position;
                    obj.transform.eulerAngles = data.rotation;
                    obj.transform.localScale = data.scale;
                    obj.name = $"EnvProp_{target.name}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
                    spawnedEnvironments.Add(obj);
                }
            }
        }

        // --- NEW: LOAD LIGHTS BACK INTO THE EDITOR ---
        if (currentMap.lightSettings != null)
        {
            foreach (var lightData in currentMap.lightSettings)
            {
                GameObject lightObj = new GameObject($"MapLight_{System.Guid.NewGuid().ToString().Substring(0,5)}");
                lightObj.transform.position = lightData.position;
                lightObj.transform.eulerAngles = lightData.rotation;

                Light lightComp = lightObj.AddComponent<Light>();
                lightComp.type = lightData.type;
                lightComp.color = lightData.color;
                lightComp.intensity = lightData.intensity;
                lightComp.range = lightData.range;
                
                if (lightData.type == LightType.Spot) lightComp.spotAngle = lightData.spotAngle;

                spawnedLights.Add(lightObj);
            }
        }
        // ---------------------------------------------

        // --- NEW: Sync Camera Config after everything is loaded ---
        SyncCameraFromMap();
        SyncDirectionalLightFromMap();
    }

    private void SyncDictionaryFromScene()
    {
        tiles.Clear(); 
        decorations.Clear(); 
        spawnedUnits.Clear(); 
        spawnedInteractables.Clear(); 
        spawnedEnemies.Clear(); 
        spawnedEnvironments.Clear(); 

        foreach (var t in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (t.name.StartsWith("Tile_"))
            {
                string[] p = t.name.Split('_');
                if (p.Length >= 4 && int.TryParse(p[1], out int x) && int.TryParse(p[2], out int y) && int.TryParse(p[3], out int z)) tiles[new Vector3Int(x, y, z)] = t;
                else if (p.Length == 3 && int.TryParse(p[1], out int ox) && int.TryParse(p[2], out int oz)) tiles[new Vector3Int(ox, 0, oz)] = t;
            }
            else if (t.name.StartsWith("SpawnUnit_") || spawnedUnits.ContainsValue(t))
            {
                string[] p = t.name.Split('_');
                if (p.Length >= 4 && int.TryParse(p[p.Length - 3], out int x) && int.TryParse(p[p.Length - 2], out int y) && int.TryParse(p[p.Length - 1], out int z)) spawnedUnits[new Vector3Int(x, y, z)] = t;
            }
            else if (t.name.StartsWith("SpawnInteractable_") || spawnedInteractables.ContainsValue(t))
            {
                string[] p = t.name.Split('_');
                if (p.Length >= 4 && int.TryParse(p[p.Length - 3], out int x) && int.TryParse(p[p.Length - 2], out int y) && int.TryParse(p[p.Length - 1], out int z)) spawnedInteractables[new Vector3Int(x, y, z)] = t;
            }
            else if (t.name.StartsWith("SpawnEnemy_") || spawnedEnemies.ContainsValue(t))
            {
                string[] p = t.name.Split('_');
                if (p.Length >= 4 && int.TryParse(p[p.Length - 3], out int x) && int.TryParse(p[p.Length - 2], out int y) && int.TryParse(p[p.Length - 1], out int z)) spawnedEnemies[new Vector3Int(x, y, z)] = t;
            }
            else if (t.name.StartsWith("EnvProp_") || spawnedEnvironments.Contains(t))
            {
                if (!spawnedEnvironments.Contains(t)) spawnedEnvironments.Add(t);
            }
            // --- NEW: Track manual Light movements ---
            else if (t.name.StartsWith("MapLight_") || spawnedLights.Contains(t))
            {
                if (!spawnedLights.Contains(t)) spawnedLights.Add(t);
            }
            // Fallback for standard decorations
            else if (decorations.ContainsValue(t) || decorationPrefabs.Any(p => p != null && t.name.StartsWith(p.name + "_")))
            {
                string[] p = t.name.Split('_');
                if (p.Length >= 4 && int.TryParse(p[p.Length - 3], out int x) && int.TryParse(p[p.Length - 2], out int y) && int.TryParse(p[p.Length - 1], out int z)) decorations[new Vector3Int(x, y, z)] = t;
            }
        }
    }

    private void GenerateMap()
    {
        SyncDictionaryFromScene();
        foreach (var obj in tiles.Values) Undo.DestroyObjectImmediate(obj);
        foreach (var obj in decorations.Values) Undo.DestroyObjectImmediate(obj);
        foreach (var obj in spawnedUnits.Values) Undo.DestroyObjectImmediate(obj);
        foreach (var obj in spawnedInteractables.Values) Undo.DestroyObjectImmediate(obj);
        foreach (var obj in spawnedEnemies.Values) Undo.DestroyObjectImmediate(obj);
        foreach (var obj in spawnedEnvironments) Undo.DestroyObjectImmediate(obj);
        foreach (var obj in spawnedLights) Undo.DestroyObjectImmediate(obj); // <--- DESTROY LIGHTS ON CLEAR

        tiles.Clear(); 
        decorations.Clear(); 
        spawnedUnits.Clear();
        spawnedInteractables.Clear(); 
        spawnedEnemies.Clear();
        spawnedEnvironments.Clear();
        spawnedLights.Clear(); // <--- RESET LIGHTS
    }

    private Vector3Int GetGridCoordinatesFromWorldPosition(Vector3 worldPos)
    {
        Vector3 size = GetTileWorldSize3D();
        return new Vector3Int(Mathf.RoundToInt(worldPos.x / (size.x + inBetweenTilesXOffset)), Mathf.RoundToInt(worldPos.y / size.y), Mathf.RoundToInt(worldPos.z / (size.z + inBetweenTilesZOffset)));
    }

    private Vector3 GridToWorld(Vector3Int gridPos, Vector3 tileSize) => new Vector3(gridPos.x * (tileSize.x + inBetweenTilesXOffset), gridPos.y * tileSize.y, gridPos.z * (tileSize.z + inBetweenTilesZOffset));

    private void DrawGrid()
    {
        Vector3 size = GetTileWorldSize3D();
        float cellWidth = size.x + inBetweenTilesXOffset, cellDepth = size.z + inBetweenTilesZOffset;
        float offsetX = cellWidth / 2f, offsetZ = cellDepth / 2f;
        Handles.color = new Color(0, 1, 1, 0.2f);

        for (int x = 0; x <= gridWidth; x++) Handles.DrawLine(new Vector3((x * cellWidth) - offsetX, 0, -offsetZ), new Vector3((x * cellWidth) - offsetX, 0, (gridHeight * cellDepth) - offsetZ));
        for (int z = 0; z <= gridHeight; z++) Handles.DrawLine(new Vector3(-offsetX, 0, (z * cellDepth) - offsetZ), new Vector3((gridWidth * cellWidth) - offsetX, 0, (z * cellDepth) - offsetZ));
    }

    private void DrawPreview(Vector3Int gridPos)
    {
        Handles.color = isDeletingTile ? Color.red : (isBucketMode ? Color.cyan : Color.green);
        Handles.DrawWireCube(GridToWorld(gridPos, GetTileWorldSize3D()), GetTileWorldSize3D());
    }

    private Vector3 GetTileWorldSize3D() { if (tilePrefab == null) return Vector3.one; var b = tilePrefab.transform.Find("GridBounds"); return b != null ? Vector3.Scale(b.localScale, tilePrefab.transform.localScale) : Vector3.Scale(tilePrefab.GetComponent<BoxCollider>()?.size ?? Vector3.one, tilePrefab.transform.localScale); }
    private bool IsInsideGrid(Vector3Int pos) => pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridDepth && pos.z >= 0 && pos.z < gridHeight;

    private void SaveEnvironmentsToMap()
    {
        if (currentMap == null)
        {
            Debug.LogWarning("Map Editor: Assign a Current Map Asset first to save environments!");
            return;
        }

        SyncDictionaryFromScene(); // Ensure we have the latest list

        currentMap.environmentPositions.Clear();
        foreach (var envObj in spawnedEnvironments)
        {
            if (envObj == null) continue; // <--- FIXED THIS LINE
            
            // Get original prefab name
            var src = PrefabUtility.GetCorrespondingObjectFromSource(envObj);
            string pName = src != null ? src.name : envObj.name.Replace("(Clone)", "").Split('_')[1].Trim();
            
            // Save current Gizmo-manipulated transform block
            currentMap.environmentPositions.Add(new MapData.EnvironmentData 
            { 
                prefabName = pName,
                position = envObj.transform.position,
                rotation = envObj.transform.eulerAngles,
                scale = envObj.transform.localScale
            });
        }

        EditorUtility.SetDirty(currentMap);
        AssetDatabase.SaveAssets();
        Debug.Log($"Map Editor: Saved {currentMap.environmentPositions.Count} free-form environment props to MapData!");
    }

    private void SaveLightsToMap()
    {
        if (currentMap == null) return;

        // Find all lights created by the editor
        var allLights = GameObject.FindObjectsOfType<Light>();
        currentMap.lightSettings.Clear();
        spawnedLights.Clear();

        foreach (var l in allLights)
        {
            if (l.gameObject.name.StartsWith("MapLight_"))
            {
                spawnedLights.Add(l.gameObject);
                currentMap.lightSettings.Add(new MapData.LightData
                {
                    type = l.type,
                    position = l.transform.position,
                    rotation = l.transform.eulerAngles,
                    color = l.color,
                    intensity = l.intensity,
                    range = l.range,
                    spotAngle = l.spotAngle
                });
            }
        }
        EditorUtility.SetDirty(currentMap);
        AssetDatabase.SaveAssets();
        Debug.Log($"Map Editor: Saved {currentMap.lightSettings.Count} programmatic lights to MapData!");
    }
}