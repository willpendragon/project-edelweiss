using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

public class MapEditorWindow : EditorWindow
{
    private int gridWidth = 10;
    private int gridHeight = 10; // Z
    private int gridDepth = 10;  // Y
    
    public float inBetweenTilesXOffset = 0f;
    public float inBetweenTilesZOffset = 0f; // Rappresenta la tua "YOffset" 2D passata in 3D

    private GameObject tilePrefab;
    private MapData currentMap;
    private TileType selectedTileType = TileType.Basic;

    private Dictionary<Vector3Int, GameObject> tiles = new Dictionary<Vector3Int, GameObject>();

    private bool isPlacingTile = false;
    private bool isDeletingTile = false;
    private bool clearOnClose = false;
    private Vector3Int lastGridPosition = new Vector3Int(-1, -1, -1);

    [MenuItem("Window/Map Editor")]
    public static void ShowWindow() => GetWindow<MapEditorWindow>("Map Editor");

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        
        // 1. Ripristina il Tile Prefab salvato dalle preferenze dell'Editor
        string prefabPath = EditorPrefs.GetString("MapEditor_TilePrefabPath", "");
        if (!string.IsNullOrEmpty(prefabPath))
        {
            tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }
        
        inBetweenTilesXOffset = EditorPrefs.GetFloat("MapEditor_OffsetX", 0f);
        inBetweenTilesZOffset = EditorPrefs.GetFloat("MapEditor_OffsetZ", 0f);
        clearOnClose = EditorPrefs.GetBool("MapEditor_ClearOnClose", false);
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;

        // 2. Pulizia opzionale alla chiusura della finestra
        if (clearOnClose)
        {
            GenerateMap(); // Questo distrugge tutti i tile attuali
            Debug.Log("Map Editor: Scene automatically cleared on close.");
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Map Dimensions", EditorStyles.boldLabel);
        gridWidth = EditorGUILayout.IntField("Grid Width (X)", gridWidth);
        gridDepth = EditorGUILayout.IntField("Grid Depth/Height (Y)", gridDepth);
        gridHeight = EditorGUILayout.IntField("Grid Length (Z)", gridHeight);
        
        EditorGUILayout.Space();
        GUILayout.Label("Map Offsets (Distance Between Tiles)", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        inBetweenTilesXOffset = EditorGUILayout.FloatField("X Offset", inBetweenTilesXOffset);
        inBetweenTilesZOffset = EditorGUILayout.FloatField("Z Offset (Old Y)", inBetweenTilesZOffset);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetFloat("MapEditor_OffsetX", inBetweenTilesXOffset);
            EditorPrefs.SetFloat("MapEditor_OffsetZ", inBetweenTilesZOffset);
            SceneView.RepaintAll(); // Aggiorna la griglia al volo
        }

        EditorGUILayout.Space();
        GUILayout.Label("Assets & Properties", EditorStyles.boldLabel);
        
        // Salvataggio automatico del Prefab se cambia
        EditorGUI.BeginChangeCheck();
        tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck() && tilePrefab != null)
        {
            EditorPrefs.SetString("MapEditor_TilePrefabPath", AssetDatabase.GetAssetPath(tilePrefab));
        }

        currentMap = (MapData)EditorGUILayout.ObjectField("Current Map Asset", currentMap, typeof(MapData), false);
        selectedTileType = (TileType)EditorGUILayout.EnumPopup("Tile Type", selectedTileType);

        EditorGUILayout.Space();
        
        clearOnClose = EditorGUILayout.Toggle("Clear Tiles when Closing Editor", clearOnClose);
        if (GUI.changed) EditorPrefs.SetBool("MapEditor_ClearOnClose", clearOnClose);

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate/Clear Map")) GenerateMap();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Toggle(isPlacingTile, "Paint Mode (ESC to exit)", "Button")) { isPlacingTile = true; isDeletingTile = false; }
        if (GUILayout.Toggle(isDeletingTile, "Delete Mode (ESC to exit)", "Button")) { isPlacingTile = false; isDeletingTile = true; }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Sync & Reload from Scene")) SyncDictionaryFromScene();

        EditorGUILayout.Space();
        if (GUILayout.Button("Save Map to Asset", GUILayout.Height(30))) SaveMap();
        if (GUILayout.Button("Load Map from Asset", GUILayout.Height(30))) LoadFromAsset();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // 5. Esci con ESC
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            isPlacingTile = false;
            isDeletingTile = false;
            Repaint();
            sceneView.Repaint();
            e.Use();
            return;
        }

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        if (isPlacingTile || isDeletingTile)
        {
            HandleUtility.AddDefaultControl(controlID);
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        
        bool hasHit = false;
        Vector3Int targetGridPos = Vector3Int.zero;

        // Voxel Raycasting Robusto: Cerca Collider Fisici (Cosi scavalchiamo i Gap dell'Offset!)
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            hasHit = true;
            TileController hitTile = hitInfo.collider.GetComponentInParent<TileController>();
            
            if (hitTile != null)
            {
                Vector3Int basePos = hitTile.gridPosition;
                Vector3 hitNormal = hitInfo.normal;
                Vector3Int normalOffset = new Vector3Int(
                    Mathf.RoundToInt(hitNormal.x),
                    Mathf.RoundToInt(hitNormal.y),
                    Mathf.RoundToInt(hitNormal.z)
                );

                if (isDeletingTile) targetGridPos = basePos;
                else targetGridPos = basePos + normalOffset; // Clic su faccia laterale/superiore = Tile accanto
            }
            else
            {
                Vector3 offsetPos = hitInfo.point + (isDeletingTile ? -hitInfo.normal : hitInfo.normal) * 0.1f;
                targetGridPos = GetGridCoordinatesFromWorldPosition(offsetPos);
            }
        }
        else
        {
            // Floor Raycast
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                hasHit = true;
                Vector3 hitPoint = ray.GetPoint(enter);
                targetGridPos = GetGridCoordinatesFromWorldPosition(hitPoint);
                targetGridPos.y = 0; 
            }
        }

        if (hasHit)
        {
            DrawPreview(targetGridPos);

            if (!IsInsideGrid(targetGridPos) || e.alt) return;

            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                if (targetGridPos != lastGridPosition)
                {
                    if (isPlacingTile) PlaceTile(targetGridPos, selectedTileType);
                    else if (isDeletingTile) DeleteTile(targetGridPos);
                    
                    lastGridPosition = targetGridPos;
                }
                e.Use();
            }
        }

        if (e.type == EventType.MouseUp) lastGridPosition = new Vector3Int(-1, -1, -1);

        DrawGrid();
        sceneView.Repaint();
    }

    private void PlaceTile(Vector3Int position, TileType type)
    {
        if (tilePrefab == null) return;

        SyncDictionaryFromScene(); 

        if (tiles.ContainsKey(position)) return;

        Vector3 tileSize = GetTileWorldSize3D();
        Vector3 worldPos = GridToWorld(position, tileSize);

        GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab);
        tile.transform.position = worldPos;
        tile.name = $"Tile_{position.x}_{position.y}_{position.z}";

        Undo.RegisterCreatedObjectUndo(tile, "Place Tile");

        tiles[position] = tile;
        HideTileEffects(tile);

        var controller = tile.GetComponent<TileController>();
        if (controller != null)
        {
            controller.tileType = type;
            controller.gridPosition = position;
        }

        ApplyDecorativeColor(tile, type);
    }

    // 4. Stile Decorativo! Colorerà di grigio scuro il tile se è Ostacolo
    private void ApplyDecorativeColor(GameObject tile, TileType type)
    {
        if (type == TileType.Obstacle)
        {
            // Cerchiamo in modo specifico il GameObject figlio "GridBounds"
            Transform gridBoundsTransform = tile.transform.Find("GridBounds");
            if (gridBoundsTransform != null)
            {
                Renderer renderer = gridBoundsTransform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block);
                    
                    // Nello standard URP lit shader, la property per il colore base si chiama _BaseColor
                    block.SetColor("_BaseColor", new Color(0.15f, 0.15f, 0.15f, 1f)); 
                    
                    // Come fallback, per shader legacy o custom:
                    block.SetColor("_Color", new Color(0.15f, 0.15f, 0.15f, 1f));
                    
                    renderer.SetPropertyBlock(block);
                }
            }
            else
            {
                Debug.LogWarning($"[MapEditor] Impossibile applicare il colore decorativo: '{tile.name}' non ha un figlio 'GridBounds'.");
            }
        }
    }

    private void DeleteTile(Vector3Int position)
    {
        SyncDictionaryFromScene();

        if (tiles.ContainsKey(position))
        {
            GameObject tile = tiles[position];
            tiles.Remove(position);
            Undo.DestroyObjectImmediate(tile);
        }
    }

    // 3. I calcoli delle coordinate integrano il gap inBetweenTiles!
    private Vector3Int GetGridCoordinatesFromWorldPosition(Vector3 worldPos)
    {
        Vector3 size = GetTileWorldSize3D();
        float spacingX = size.x + inBetweenTilesXOffset;
        float spacingZ = size.z + inBetweenTilesZOffset;

        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x / spacingX), 
            Mathf.FloorToInt(worldPos.y / size.y), 
            Mathf.FloorToInt(worldPos.z / spacingZ)
        );
    }

    private Vector3 GridToWorld(Vector3Int gridPos, Vector3 tileSize)
    {
        float spacingX = tileSize.x + inBetweenTilesXOffset;
        float spacingZ = tileSize.z + inBetweenTilesZOffset;

        float x = gridPos.x * spacingX + (tileSize.x * 0.5f);
        float y = gridPos.y * tileSize.y; // Assumendo base-pivot
        float z = gridPos.z * spacingZ + (tileSize.z * 0.5f);
        return new Vector3(x, y, z);
    }

    private void DrawGrid()
    {
        Vector3 size = GetTileWorldSize3D();
        float spacingX = size.x + inBetweenTilesXOffset;
        float spacingZ = size.z + inBetweenTilesZOffset;

        Handles.color = new Color(0, 1, 1, 0.2f);
        
        for (int x = 0; x <= gridWidth; x++)
            Handles.DrawLine(new Vector3(x * spacingX, 0, 0), new Vector3(x * spacingX, 0, gridHeight * spacingZ));
        for (int z = 0; z <= gridHeight; z++)
            Handles.DrawLine(new Vector3(0, 0, z * spacingZ), new Vector3(gridWidth * spacingX, 0, z * spacingZ));
    }

    private void DrawPreview(Vector3Int gridPos)
    {
        Vector3 size = GetTileWorldSize3D();
        float spacingX = size.x + inBetweenTilesXOffset;
        float spacingZ = size.z + inBetweenTilesZOffset;

        float centerX = gridPos.x * spacingX + (size.x * 0.5f);
        float centerY = gridPos.y * size.y + (size.y * 0.5f); 
        float centerZ = gridPos.z * spacingZ + (size.z * 0.5f);

        Vector3 center = new Vector3(centerX, centerY, centerZ);
        
        Handles.color = isDeletingTile ? Color.red : Color.green;
        Handles.DrawWireCube(center, size);
    }

    private Vector3 GetTileWorldSize3D()
    {
        if (tilePrefab == null) return Vector3.one;
        Transform bounds = tilePrefab.transform.Find("GridBounds");
        if (bounds == null) 
        {
            BoxCollider col = tilePrefab.GetComponent<BoxCollider>();
            if (col != null) return col.size;
            return Vector3.one;
        }
        return bounds.localScale;
    }

    private bool IsInsideGrid(Vector3Int pos) 
        => pos.x >= 0 && pos.x < gridWidth && 
           pos.y >= 0 && pos.y < gridDepth && 
           pos.z >= 0 && pos.z < gridHeight;

    private void HideTileEffects(GameObject tile)
    {
        Transform effects = tile.transform.Find("TileEffects");
        if (effects != null) SceneVisibilityManager.instance.Hide(effects.gameObject, true);
    }

    private void SaveMap()
    {
        if (currentMap == null) return;
        SyncDictionaryFromScene(); 
        currentMap.tilePositions.Clear();
        foreach (var kvp in tiles)
        {
            var controller = kvp.Value.GetComponent<TileController>();
            currentMap.tilePositions.Add(new MapData.TileData
            {
                position = kvp.Key,
                tileType = controller ? controller.tileType : TileType.Basic
            });
        }
        currentMap.horizontalSize = gridWidth;
        currentMap.depthSize = gridDepth;
        currentMap.verticalSize = gridHeight;

        EditorUtility.SetDirty(currentMap);
        AssetDatabase.SaveAssets(); 
    }

    private void LoadFromAsset()
    {
        if (currentMap == null || tilePrefab == null) return;

        SyncDictionaryFromScene();
        foreach (var obj in tiles.Values) DestroyImmediate(obj);
        tiles.Clear();

        Vector3 tileSize = GetTileWorldSize3D();
        foreach (var data in currentMap.tilePositions)
        {
            Vector3 worldPos = GridToWorld(data.position, tileSize);
            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab);
            tile.transform.position = worldPos;
            tile.name = $"Tile_{data.position.x}_{data.position.y}_{data.position.z}";

            var controller = tile.GetComponent<TileController>();
            if (controller != null) 
            {
                controller.tileType = data.tileType;
                controller.gridPosition = data.position;
            }

            tiles[data.position] = tile;
            HideTileEffects(tile);
            
            // Applica il look decorativo caricando i file vecchi
            ApplyDecorativeColor(tile, data.tileType);
        }
    }

    private void SyncDictionaryFromScene()
    {
        tiles.Clear();
        GameObject[] allTiles = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var t in allTiles)
        {
            if (t.name.StartsWith("Tile_"))
            {
                string[] parts = t.name.Split('_');
                if (parts.Length == 4 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y) && int.TryParse(parts[3], out int z))
                {
                    tiles[new Vector3Int(x, y, z)] = t;
                }
                else if (parts.Length == 3 && int.TryParse(parts[1], out int oldX) && int.TryParse(parts[2], out int oldZ))
                {
                    tiles[new Vector3Int(oldX, 0, oldZ)] = t;
                }
            }
        }
    }

    private void GenerateMap()
    {
        SyncDictionaryFromScene();
        foreach (var obj in tiles.Values) Undo.DestroyObjectImmediate(obj);
        tiles.Clear();
    }
}