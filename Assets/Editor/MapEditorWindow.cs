using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MapEditorWindow : EditorWindow
{
    private int gridWidth = 10;
    private int gridHeight = 10; // Z
    private int gridDepth = 10;  // Y,

    public float inBetweenTilesXOffset = 0f;
    public float inBetweenTilesZOffset = 0f;

    private GameObject tilePrefab;
    private GameObject decorationPrefab; // Currently selected active decoration

    // --- NEW: Multiple Decorations List ---
    public List<GameObject> decorationPrefabs = new List<GameObject>();
    private SerializedObject _so;
    private SerializedProperty _decorationsProp;
    private int _selectedDecorationIndex = 0;
    private Vector2 _decorScrollPos;
    private Vector2 _mainScrollPos; // <-- ADD THIS LINE

    private MapData currentMap;
    private TileType selectedTileType = TileType.Basic;

    private Dictionary<Vector3Int, GameObject> tiles = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, GameObject> decorations = new Dictionary<Vector3Int, GameObject>();

    private bool isPlacingTile = false;
    private bool isPlacingDecoration = false;
    private bool isDeletingTile = false;
    private bool clearOnClose = false;
    private Vector3Int lastGridPosition = new Vector3Int(-1, -1, -1);

    public float paintDelay = 0.15f;
    private double lastPaintTime = 0.0;
    private Texture2D headerImage;

    [MenuItem("Window/Map Editor")]
    public static void ShowWindow() => GetWindow<MapEditorWindow>("Map Editor");

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;

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

        // --- NEW: Load Prefabs ---
        _so = new SerializedObject(this);
        _decorationsProp = _so.FindProperty("decorationPrefabs");
        LoadDecoPrefabsFromPrefs();
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
        // START OF MAIN SCROLL VIEW
        _mainScrollPos = EditorGUILayout.BeginScrollView(_mainScrollPos);

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

        // 2. DIMENSIONI DELLA MAPPA
        GUILayout.Label("Map Dimensions", EditorStyles.boldLabel);
        gridWidth = EditorGUILayout.IntField("Grid Width (X)", gridWidth);
        gridDepth = EditorGUILayout.IntField("Grid Depth/Height (Y)", gridDepth);
        gridHeight = EditorGUILayout.IntField("Grid Length (Z)", gridHeight);

        EditorGUILayout.Space();
        // 3. OFFSET E TEMPI DI ATTESA
        GUILayout.Label("Map Offsets & Experience", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();

        inBetweenTilesXOffset = EditorGUILayout.FloatField("X Offset", inBetweenTilesXOffset);
        inBetweenTilesZOffset = EditorGUILayout.FloatField("Z Offset (Old Y)", inBetweenTilesZOffset);

        paintDelay = EditorGUILayout.Slider("Paint Cooldown (sec)", paintDelay, 0.05f, 1.0f);

        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetFloat("MapEditor_OffsetX", inBetweenTilesXOffset);
            EditorPrefs.SetFloat("MapEditor_OffsetZ", inBetweenTilesZOffset);
            EditorPrefs.SetFloat("MapEditor_PaintDelay", paintDelay);
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();
        // 4. ASSET E PROPRIETA'
        GUILayout.Label("Assets & Properties", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck() && tilePrefab != null)
        {
            EditorPrefs.SetString("MapEditor_TilePrefabPath", AssetDatabase.GetAssetPath(tilePrefab));
        }

        // --- NEW: Multiple Decorations Panel ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Decoration Prefabs Box", EditorStyles.boldLabel);

        _so.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_decorationsProp, new GUIContent("Decoration Pool"), true);
        if (EditorGUI.EndChangeCheck())
        {
            _so.ApplyModifiedProperties();
            SaveDecoPrefabsToPrefs();

            if (_selectedDecorationIndex >= decorationPrefabs.Count)
                _selectedDecorationIndex = 0;

            if (decorationPrefabs.Count > 0)
                decorationPrefab = decorationPrefabs[_selectedDecorationIndex];
            else
                decorationPrefab = null;
        }

        // Visual Preview Grid
        if (decorationPrefabs.Count > 0)
        {
            EditorGUILayout.LabelField("Select Active Decoration:", EditorStyles.label);
            _decorScrollPos = EditorGUILayout.BeginScrollView(_decorScrollPos, GUILayout.Height(85));
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < decorationPrefabs.Count; i++)
            {
                var prefab = decorationPrefabs[i];
                if (prefab == null) continue;

                Texture2D preview = AssetPreview.GetAssetPreview(prefab);

                // Highlight the selected element
                GUI.backgroundColor = (i == _selectedDecorationIndex) ? Color.green : Color.white;

                GUIContent content = preview != null ? new GUIContent(preview, prefab.name) : new GUIContent(prefab.name);

                if (GUILayout.Button(content, GUILayout.Width(64), GUILayout.Height(64)))
                {
                    _selectedDecorationIndex = i;
                    decorationPrefab = prefab;
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space();

        currentMap = (MapData)EditorGUILayout.ObjectField("Current Map Asset", currentMap, typeof(MapData), false);
        selectedTileType = (TileType)EditorGUILayout.EnumPopup("Tile Type", selectedTileType);

        EditorGUILayout.Space();

        clearOnClose = EditorGUILayout.Toggle("Clear Tiles when Closing Editor", clearOnClose);
        if (GUI.changed) EditorPrefs.SetBool("MapEditor_ClearOnClose", clearOnClose);

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate/Clear Map")) GenerateMap();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Toggle(isPlacingTile, "Paint Tile", "Button")) { isPlacingTile = true; isPlacingDecoration = false; isDeletingTile = false; }
        if (GUILayout.Toggle(isPlacingDecoration, "Paint Decoration", "Button")) { isPlacingTile = false; isPlacingDecoration = true; isDeletingTile = false; }
        if (GUILayout.Toggle(isDeletingTile, "Delete Mode", "Button")) { isPlacingTile = false; isPlacingDecoration = false; isDeletingTile = true; }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Sync & Reload from Scene")) SyncDictionaryFromScene();

        EditorGUILayout.Space();

        if (decorationPrefabs.Count > 1)
        {
            EditorGUILayout.HelpBox("Warning: Currently 'Save Map' only saves decoration positions. Multi-Prefabs won't be saved in MapData correctly until MapData logic is upgraded!", MessageType.Warning);
        }

        if (GUILayout.Button("Save Map to Asset", GUILayout.Height(30))) SaveMap();
        if (GUILayout.Button("Load Map from Asset", GUILayout.Height(30))) LoadFromAsset();

        // END OF MAIN SCROLL VIEW
        EditorGUILayout.EndScrollView();
    }

    private void SaveDecoPrefabsToPrefs()
    {
        var paths = decorationPrefabs.Where(p => p != null).Select(p => AssetDatabase.GetAssetPath(p));
        EditorPrefs.SetString("MapEditor_DecoPrefabs", string.Join(";", paths));
    }

    private void LoadDecoPrefabsFromPrefs()
    {
        string saved = EditorPrefs.GetString("MapEditor_DecoPrefabs", "");
        decorationPrefabs.Clear();
        if (!string.IsNullOrEmpty(saved))
        {
            foreach (var path in saved.Split(';'))
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (p != null) decorationPrefabs.Add(p);
            }
        }
        if (decorationPrefabs.Count > 0)
            decorationPrefab = decorationPrefabs[0];
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            isPlacingTile = false;
            isPlacingDecoration = false;
            isDeletingTile = false;
            Repaint();
            sceneView.Repaint();
            e.Use();
            return;
        }

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        if (isPlacingTile || isPlacingDecoration || isDeletingTile)
        {
            HandleUtility.AddDefaultControl(controlID);
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        bool hasHit = false;
        Vector3Int targetGridPos = Vector3Int.zero;
        Vector3 tileSize = GetTileWorldSize3D();

        float closestDist = float.MaxValue;

        foreach (var kvp in decorations)
        {
            Vector3 worldPos = GridToWorld(kvp.Key, tileSize);
            Bounds decoBounds = new Bounds(worldPos + tileSize / 2f, tileSize);

            if (decoBounds.IntersectRay(ray, out float dist))
            {
                if (dist < closestDist)
                {
                    closestDist = dist;
                    targetGridPos = kvp.Key;

                    if (isPlacingDecoration || isPlacingTile) targetGridPos.y += 1;
                    hasHit = true;
                }
            }
        }

        if (!hasHit)
        {
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
        }


        if (hasHit)
        {
            if (!IsInsideGrid(targetGridPos) || e.alt) return;

            DrawPreview(targetGridPos);

            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                double currentTime = EditorApplication.timeSinceStartup;

                bool cooldownPassed = (currentTime - lastPaintTime) > paintDelay;
                bool isSameCellAsLast = (targetGridPos == lastGridPosition);

                if (e.type == EventType.MouseDrag && isSameCellAsLast)
                {
                    e.Use();
                    return;
                }

                if (cooldownPassed)
                {
                    if (isPlacingTile) PlaceTile(targetGridPos, selectedTileType);
                    else if (isPlacingDecoration) PlaceDecoration(targetGridPos);
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
        else if (type == TileType.Chest || type == TileType.MinibossChest || type == TileType.BossChest)
        {
            Transform gridBoundsTransform = tile.transform.Find("GridBounds");
            if (gridBoundsTransform != null)
            {
                Renderer renderer = gridBoundsTransform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block);

                    Color chestColor = new Color(0.5f, 0.0f, 0.8f, 1f);

                    if (type == TileType.MinibossChest)
                        chestColor = Color.yellow;
                    else if (type == TileType.BossChest)
                        chestColor = Color.red;

                    block.SetColor("_BaseColor", chestColor);
                    block.SetColor("_Color", chestColor);

                    renderer.SetPropertyBlock(block);
                }
            }
        }
    }

    private void DeleteTile(Vector3Int position)
    {
        SyncDictionaryFromScene();

        if (decorations.ContainsKey(position))
        {
            GameObject deco = decorations[position];
            decorations.Remove(position);
            Undo.DestroyObjectImmediate(deco);
        }

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
        float x = gridPos.x * (tileSize.x + inBetweenTilesXOffset);
        float y = gridPos.y * tileSize.y;
        float z = gridPos.z * (tileSize.z + inBetweenTilesZOffset);

        return new Vector3(x, y, z);
    }

    private void DrawGrid()
    {
        Vector3 size = GetTileWorldSize3D();
        float cellWidth = size.x + inBetweenTilesXOffset;
        float cellDepth = size.z + inBetweenTilesZOffset;

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

        // UPDATED: Save Decoration Prefab Names!
        currentMap.decorationPositions.Clear();
        foreach (var kvp in decorations)
        {
            string cleanName = kvp.Value.name.Split('_')[0]; // Strip coordinates from name, e.g. "Deco_1_2_3" to "Deco" or "Pillar"

            // Hack to get actual prefab name if instantiated via Editor
            var sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(kvp.Value);
            string pName = sourcePrefab != null ? sourcePrefab.name : kvp.Value.name.Replace("(Clone)", "").Trim();

            currentMap.decorationPositions.Add(new MapData.DecorationData
            {
                position = kvp.Key,
                prefabName = pName // Save the prefab name!
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
        if (currentMap == null) return;

        SyncDictionaryFromScene();

        foreach (var obj in tiles.Values) DestroyImmediate(obj);
        tiles.Clear();

        foreach (var obj in decorations.Values) DestroyImmediate(obj);
        decorations.Clear();

        Vector3 tileSize = GetTileWorldSize3D();

        if (tilePrefab != null)
        {
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

        // UPDATED: Load specific decoration prefab by name!
        foreach (var data in currentMap.decorationPositions)
        {
            // Attempt to find the matching prefab from our list
            GameObject targetPrefab = decorationPrefabs.FirstOrDefault(p => p != null && p.name == data.prefabName);

            // Fallback to Resources loading (just like runtime will)
            if (targetPrefab == null && !string.IsNullOrEmpty(data.prefabName))
                targetPrefab = Resources.Load<GameObject>(data.prefabName);

            // Absolute fallback
            if (targetPrefab == null)
                targetPrefab = decorationPrefab;

            if (targetPrefab != null)
            {
                Vector3 worldPos = GridToWorld(data.position, tileSize);
                GameObject deco = (GameObject)PrefabUtility.InstantiatePrefab(targetPrefab);
                deco.transform.position = worldPos;

                // Keep naming format identical so SyncDictionaryFromScene finds it on next refresh!
                deco.name = $"{targetPrefab.name}_{data.position.x}_{data.position.y}_{data.position.z}";

                decorations[data.position] = deco;
            }
        }
    }

    private void SyncDictionaryFromScene()
    {
        tiles.Clear();
        decorations.Clear();

        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var t in allObjects)
        {
            if (t.name.StartsWith("Tile_"))
            {
                string[] parts = t.name.Split('_');
                if (parts.Length >= 4 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y) && int.TryParse(parts[3], out int z))
                {
                    tiles[new Vector3Int(x, y, z)] = t;
                }
                else if (parts.Length == 3 && int.TryParse(parts[1], out int oldX) && int.TryParse(parts[2], out int oldZ))
                {
                    tiles[new Vector3Int(oldX, 0, oldZ)] = t;
                }
            }
            // CHANGED logic slightly to allow dynamic names like "Pillar_1_2_3" instead of just "Deco_1_2_3"
            else if (decorations.ContainsValue(t) || decorationPrefabs.Any(p => p != null && t.name.StartsWith(p.name + "_")))
            {
                string[] parts = t.name.Split('_');
                // The last 3 parts should always be the coordinates X, Y, Z
                int len = parts.Length;
                if (len >= 4 && int.TryParse(parts[len - 3], out int dx) && int.TryParse(parts[len - 2], out int dy) && int.TryParse(parts[len - 1], out int dz))
                {
                    decorations[new Vector3Int(dx, dy, dz)] = t;
                }
            }
        }
    }

    private void PlaceDecoration(Vector3Int position)
    {
        if (decorationPrefab == null) return;

        SyncDictionaryFromScene();

        if (decorations.ContainsKey(position)) return;

        Vector3 tileSize = GetTileWorldSize3D();
        Vector3 worldPos = GridToWorld(position, tileSize);

        GameObject deco = (GameObject)PrefabUtility.InstantiatePrefab(decorationPrefab);
        deco.transform.position = worldPos;
        // Name it by its actual Prefab Name so SaveMap can harvest the ID!
        deco.name = $"{decorationPrefab.name}_{position.x}_{position.y}_{position.z}";

        Undo.RegisterCreatedObjectUndo(deco, "Place Decoration");

        decorations[position] = deco;
    }

    private void GenerateMap()
    {
        SyncDictionaryFromScene();

        foreach (var obj in tiles.Values) Undo.DestroyObjectImmediate(obj);
        tiles.Clear();

        foreach (var obj in decorations.Values) Undo.DestroyObjectImmediate(obj);
        decorations.Clear();
    }
}