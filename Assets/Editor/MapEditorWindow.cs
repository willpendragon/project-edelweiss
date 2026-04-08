using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

public class MapEditorWindow : EditorWindow
{
    private int gridWidth = 10;
    private int gridHeight = 10; // Z
    private int gridDepth = 10;  // Y
    
    public float inBetweenTilesXOffset = 0f;
    public float inBetweenTilesZOffset = 0f;

    private GameObject tilePrefab;
    private MapData currentMap;
    private TileType selectedTileType = TileType.Basic;

    private Dictionary<Vector3Int, GameObject> tiles = new Dictionary<Vector3Int, GameObject>();

    private bool isPlacingTile = false;
    private bool isDeletingTile = false;
    private bool clearOnClose = false;
    private Vector3Int lastGridPosition = new Vector3Int(-1, -1, -1);

    // --- NUOVE VARIABILI PER IL COOLDOWN ---
    public float paintDelay = 0.15f; 
    private double lastPaintTime = 0.0;

    // --- IMMAGINE DEL BANNER ---
    private Texture2D headerImage;

    [MenuItem("Window/Map Editor")]
    public static void ShowWindow() => GetWindow<MapEditorWindow>("Map Editor");

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;

        // Trova e carica l'immagine del Banner. Assicurati di mettere un'immagine chiamata 
        // "MapEditorBanner" nella cartella: Assets/Editor/Resources
        headerImage = Resources.Load<Texture2D>("MapEditorBanner");

        string prefabPath = EditorPrefs.GetString("MapEditor_TilePrefabPath", "");
        if (!string.IsNullOrEmpty(prefabPath))
        {
            tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }
        
        inBetweenTilesXOffset = EditorPrefs.GetFloat("MapEditor_OffsetX", 0f);
        inBetweenTilesZOffset = EditorPrefs.GetFloat("MapEditor_OffsetZ", 0f);
        clearOnClose = EditorPrefs.GetBool("MapEditor_ClearOnClose", false);
        paintDelay = EditorPrefs.GetFloat("MapEditor_PaintDelay", 0.15f);
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;

        if (clearOnClose)
        {
            GenerateMap(); 
            Debug.Log("Map Editor: Scene automatically cleared on close.");
        }
    }

    private void OnGUI()
    {
        // 1. BANNER IMMAGINE
        if (headerImage != null)
        {
            float imageWidth = EditorGUIUtility.currentViewWidth;
            float imageHeight = imageWidth * ((float)headerImage.height / headerImage.width); // Aspetto ratio
            GUILayout.Label(headerImage, GUILayout.Width(imageWidth), GUILayout.Height(Mathf.Clamp(imageHeight, 50, 150)));
        }
        else
        {
            EditorGUILayout.HelpBox("Per mostrare un Banner in cima all'Editor, posizionare l'immagine 'MapEditorBanner.png' dentro 'Assets/Editor/Resources'.", MessageType.Info);
        }

        GUILayout.Label("Map Dimensions", EditorStyles.boldLabel);
        gridWidth = EditorGUILayout.IntField("Grid Width (X)", gridWidth);
        gridDepth = EditorGUILayout.IntField("Grid Depth/Height (Y)", gridDepth);
        gridHeight = EditorGUILayout.IntField("Grid Length (Z)", gridHeight);
        
        EditorGUILayout.Space();
        GUILayout.Label("Map Offsets & Experience", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        
        inBetweenTilesXOffset = EditorGUILayout.FloatField("X Offset", inBetweenTilesXOffset);
        inBetweenTilesZOffset = EditorGUILayout.FloatField("Z Offset (Old Y)", inBetweenTilesZOffset);

        // 2. SLIDER PER IL PAINT DELAY
        paintDelay = EditorGUILayout.Slider("Paint Cooldown (sec)", paintDelay, 0.05f, 1.0f);
        
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetFloat("MapEditor_OffsetX", inBetweenTilesXOffset);
            EditorPrefs.SetFloat("MapEditor_OffsetZ", inBetweenTilesZOffset);
            EditorPrefs.SetFloat("MapEditor_PaintDelay", paintDelay);
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Assets & Properties", EditorStyles.boldLabel);
        
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
                else targetGridPos = basePos + normalOffset; 
            }
            else
            {
                Vector3 offsetPos = hitInfo.point + (isDeletingTile ? -hitInfo.normal : hitInfo.normal) * 0.1f;
                targetGridPos = GetGridCoordinatesFromWorldPosition(offsetPos);
            }
        }
        else
        {
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
            if (!IsInsideGrid(targetGridPos) || e.alt) return;

            DrawPreview(targetGridPos);

            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                double currentTime = EditorApplication.timeSinceStartup;

                // È passato abbastanza tempo dall'ultimo piazzamento?
                bool cooldownPassed = (currentTime - lastPaintTime) > paintDelay;
                
                // Stiamo puntando la STESSA cella su cui abbiamo appena interagito?
                bool isSameCellAsLast = (targetGridPos == lastGridPosition);

                // LOGICA:
                // Se stiamo trascinando il mouse, e siamo ancora fermi sulla stessa cella, ignoriamo totalmente(evita torri accidentali).
                // Altrimenti, se è scattato il cooldown, procedi con l'azione!
                if (e.type == EventType.MouseDrag && isSameCellAsLast)
                {
                    e.Use();
                    return;
                }

                if (cooldownPassed)
                {
                    if (isPlacingTile) PlaceTile(targetGridPos, selectedTileType);
                    else if (isDeletingTile) DeleteTile(targetGridPos);
                    
                    lastGridPosition = targetGridPos;
                    lastPaintTime = currentTime;
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

    private void ApplyDecorativeColor(GameObject tile, TileType type)
    {
        if (type == TileType.Obstacle)
        {
            Transform gridBoundsTransform = tile.transform.Find("GridBounds");
            if (gridBoundsTransform != null)
            {
                Renderer renderer = gridBoundsTransform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block);
                    
                    block.SetColor("_BaseColor", new Color(0.15f, 0.15f, 0.15f, 1f)); 
                    block.SetColor("_Color", new Color(0.15f, 0.15f, 0.15f, 1f));
                    
                    renderer.SetPropertyBlock(block);
                }
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

    private Vector3Int GetGridCoordinatesFromWorldPosition(Vector3 worldPos)
    {
        Vector3 size = GetTileWorldSize3D();
        float cellWidth = size.x + inBetweenTilesXOffset;
        float cellDepth = size.z + inBetweenTilesZOffset;

        return new Vector3Int(
            Mathf.RoundToInt(worldPos.x / cellWidth), 
            Mathf.RoundToInt(worldPos.y / size.y), 
            Mathf.RoundToInt(worldPos.z / cellDepth)
        );
    }

    private Vector3 GridToWorld(Vector3Int gridPos, Vector3 tileSize)
    {
        // Esatta formula del GridManager
        float x = gridPos.x * (tileSize.x + inBetweenTilesXOffset);
        float y = gridPos.y * tileSize.y; 
        float z = gridPos.z * (tileSize.z + inBetweenTilesZOffset);

        // Non aggiungiamo la metà del tileSize se il pivot del tuo cubo prefab non è nell'angolo!
        // Visto che in GridManager spawni senza "offset di mezzeria", qui facciamo uguale.
        return new Vector3(x, y, z);
    }

    private void DrawGrid()
    {
        Vector3 size = GetTileWorldSize3D();
        float cellWidth = size.x + inBetweenTilesXOffset;
        float cellDepth = size.z + inBetweenTilesZOffset;

        // Calcoliamo l'offset necessario per spostare le linee sui bordi del tile
        float offsetX = cellWidth / 2f;
        float offsetZ = cellDepth / 2f;

        Handles.color = new Color(0, 1, 1, 0.2f);
        
        for (int x = 0; x <= gridWidth; x++)
        {
            float lineX = (x * cellWidth) - offsetX;
            Handles.DrawLine(
                new Vector3(lineX, 0, -offsetZ), 
                new Vector3(lineX, 0, (gridHeight * cellDepth) - offsetZ)
            );
        }
            
        for (int z = 0; z <= gridHeight; z++)
        {
            float lineZ = (z * cellDepth) - offsetZ;
            Handles.DrawLine(
                new Vector3(-offsetX, 0, lineZ), 
                new Vector3((gridWidth * cellWidth) - offsetX, 0, lineZ)
            );
        }
    }

    private void DrawPreview(Vector3Int gridPos)
    {
        Vector3 size = GetTileWorldSize3D();
        
        // Disegno il preview square richiamando direttamente la funzione maestra
        Vector3 center = GridToWorld(gridPos, size);
        
        Handles.color = isDeletingTile ? Color.red : Color.green;
        Handles.DrawWireCube(center, size);
    }

    private Vector3 GetTileWorldSize3D()
    {
        if (tilePrefab == null) return Vector3.one;

        Vector3 scale = tilePrefab.transform.localScale;

        Transform bounds = tilePrefab.transform.Find("GridBounds");
        if (bounds != null) 
        {
            return Vector3.Scale(bounds.localScale, scale);
        }

        BoxCollider col = tilePrefab.GetComponent<BoxCollider>();
        if (col != null)
        {
            return Vector3.Scale(col.size, scale);
        }

        return scale;
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