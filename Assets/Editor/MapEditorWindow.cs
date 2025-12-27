using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MapEditorWindow : EditorWindow
{
    private int gridWidth = 10;
    private int gridHeight = 10;
    private GameObject tilePrefab;
    private MapData currentMap;
    private TileType selectedTileType = TileType.Basic;

    // Use a dictionary for speed, but we will "Sync" it to ensure it's never empty
    private Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();

    private bool isPlacingTile = false;
    private bool isDeletingTile = false;
    private Vector2Int lastGridPosition = new Vector2Int(-1, -1);

    [MenuItem("Window/Map Editor")]
    public static void ShowWindow() => GetWindow<MapEditorWindow>("Map Editor");

    private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    private void OnGUI()
    {
        GUILayout.Label("Map Settings", EditorStyles.boldLabel);
        gridWidth = EditorGUILayout.IntField("Grid Width", gridWidth);
        gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight);
        tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(GameObject), false);
        currentMap = (MapData)EditorGUILayout.ObjectField("Current Map Asset", currentMap, typeof(MapData), false);
        selectedTileType = (TileType)EditorGUILayout.EnumPopup("Tile Type", selectedTileType);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate/Clear Map")) GenerateMap();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Toggle(isPlacingTile, "Paint Mode", "Button")) { isPlacingTile = true; isDeletingTile = false; }
        if (GUILayout.Toggle(isDeletingTile, "Delete Mode", "Button")) { isPlacingTile = false; isDeletingTile = true; }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Sync & Reload from Scene")) SyncDictionaryFromScene();

        // --- ADD THESE TO YOUR OnGUI METHOD ---
        if (GUILayout.Button("Save Map to Asset"))
        {
            SaveMap();
        }

        if (GUILayout.Button("Load Map from Asset"))
        {
            LoadFromAsset();
        }
        // --------------------------------------
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        // 1. Prevent "Click-Through" selection in the scene
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        if (isPlacingTile || isDeletingTile)
        {
            HandleUtility.AddDefaultControl(controlID);
        }

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector2Int gridPos = GetGridCoordinatesFromWorldPosition(hitPoint);

            // Draw Brush Preview
            DrawPreview(gridPos);

            if (!IsInsideGrid(gridPos) || e.alt) return;

            // 2. Handle Input
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                if (gridPos != lastGridPosition)
                {
                    if (isPlacingTile) PlaceTile(gridPos, selectedTileType);
                    else if (isDeletingTile) DeleteTile(gridPos);
                    lastGridPosition = gridPos;
                }
                e.Use();
            }
        }

        if (e.type == EventType.MouseUp) lastGridPosition = new Vector2Int(-1, -1);

        DrawGrid();
        sceneView.Repaint();
    }

    private void PlaceTile(Vector2Int position, TileType type)
    {
        if (tilePrefab == null) return;

        SyncDictionaryFromScene(); // Ensure we don't double-place if dictionary was wiped

        if (tiles.ContainsKey(position)) return;

        Vector2 tileSize = GetTileWorldSize();
        Vector3 worldPos = GridToWorld(position, tileSize);

        GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab);
        tile.transform.position = worldPos;
        tile.name = $"Tile_{position.x}_{position.y}";

        // Support Undo
        Undo.RegisterCreatedObjectUndo(tile, "Place Tile");

        tiles[position] = tile;
        HideTileEffects(tile);

        var controller = tile.GetComponent<TileController>();
        if (controller != null) controller.tileType = type;

        //UpdateMapData();
    }

    private void DeleteTile(Vector2Int position)
    {
        SyncDictionaryFromScene();

        if (tiles.ContainsKey(position))
        {
            GameObject tile = tiles[position];
            tiles.Remove(position);
            Undo.DestroyObjectImmediate(tile);
            //UpdateMapData();
        }
    }

    private Vector2Int GetGridCoordinatesFromWorldPosition(Vector3 worldPos)
    {
        Vector2 size = GetTileWorldSize();
        // FloorToInt is the industry standard for stable grids
        return new Vector2Int(Mathf.FloorToInt(worldPos.x / size.x), Mathf.FloorToInt(worldPos.z / size.y));
    }

    private Vector3 GridToWorld(Vector2Int gridPos, Vector2 tileSize)
    {
        // Align the center of the tile to the center of the grid cell
        return new Vector3(gridPos.x * tileSize.x + (tileSize.x * 0.5f), 0, gridPos.y * tileSize.y + (tileSize.y * 0.5f));
    }

    private void SyncDictionaryFromScene()
    {
        // Rebuild dictionary by finding existing objects in scene using their names
        tiles.Clear();
        GameObject[] allTiles = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var t in allTiles)
        {
            if (t.name.StartsWith("Tile_"))
            {
                string[] parts = t.name.Split('_');
                if (parts.Length == 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
                {
                    tiles[new Vector2Int(x, y)] = t;
                }
            }
        }
    }

    private void DrawGrid()
    {
        Vector2 size = GetTileWorldSize();
        Handles.color = new Color(0, 1, 1, 0.2f);
        for (int x = 0; x <= gridWidth; x++)
            Handles.DrawLine(new Vector3(x * size.x, 0, 0), new Vector3(x * size.x, 0, gridHeight * size.y));
        for (int y = 0; y <= gridHeight; y++)
            Handles.DrawLine(new Vector3(0, 0, y * size.y), new Vector3(gridWidth * size.x, 0, y * size.y));
    }

    private void DrawPreview(Vector2Int gridPos)
    {
        Vector2 size = GetTileWorldSize();
        Vector3 center = GridToWorld(gridPos, size);
        Handles.color = isDeletingTile ? Color.red : Color.green;
        Handles.DrawWireCube(center, new Vector3(size.x, 0.1f, size.y));
    }

    private Vector2 GetTileWorldSize()
    {
        if (tilePrefab == null) return Vector2.one;
        Transform bounds = tilePrefab.transform.Find("GridBounds");
        if (bounds == null) return Vector2.one;
        return new Vector2(bounds.localScale.x, bounds.localScale.z);
    }

    private bool IsInsideGrid(Vector2Int pos) => pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;

    private void UpdateMapData()
    {
        if (currentMap == null) return;
        currentMap.tilePositions.Clear();
        foreach (var kvp in tiles)
        {
            var controller = kvp.Value.GetComponent<TileController>();
            currentMap.tilePositions.Add(new MapData.TileData { position = kvp.Key, tileType = controller ? controller.tileType : TileType.Basic });
        }
        EditorUtility.SetDirty(currentMap);
    }

    private void GenerateMap()
    {
        SyncDictionaryFromScene();
        foreach (var obj in tiles.Values) Undo.DestroyObjectImmediate(obj);
        tiles.Clear();
        //UpdateMapData();
    }

    private void HideTileEffects(GameObject tile)
    {
        Transform effects = tile.transform.Find("TileEffects");
        if (effects != null) SceneVisibilityManager.instance.Hide(effects.gameObject, true);
    }

    private void SaveMap()
    {
        if (currentMap == null)
        {
            Debug.LogError("Assign a MapData asset first!");
            return;
        }

        SyncDictionaryFromScene(); // Make sure our dictionary matches the scene

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

        EditorUtility.SetDirty(currentMap);
        AssetDatabase.SaveAssets(); // This writes the file to disk!
        Debug.Log($"Saved {tiles.Count} tiles to {currentMap.name}");
    }

    private void LoadFromAsset()
    {
        if (currentMap == null || tilePrefab == null)
        {
            Debug.LogError("Assign MapData and Tile Prefab first!");
            return;
        }

        // 1. Clear the scene
        SyncDictionaryFromScene();
        foreach (var obj in tiles.Values) DestroyImmediate(obj);
        tiles.Clear();

        // 2. Spawn from Data
        Vector2 tileSize = GetTileWorldSize();
        foreach (var data in currentMap.tilePositions)
        {
            Vector3 worldPos = GridToWorld(data.position, tileSize);
            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab);
            tile.transform.position = worldPos;
            tile.name = $"Tile_{data.position.x}_{data.position.y}";

            var controller = tile.GetComponent<TileController>();
            if (controller != null) controller.tileType = data.tileType;

            tiles[data.position] = tile;
            HideTileEffects(tile);
        }

        Debug.Log($"Loaded {currentMap.tilePositions.Count} tiles.");
    }
}