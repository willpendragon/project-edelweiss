using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MapEditorWindow : EditorWindow
{
    private int gridWidth = 10;
    private int gridHeight = 10;
    private float tileOffsetX = 1.0f;
    private float tileOffsetY = 1.0f;
    private GameObject tilePrefab;
    private Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();

    private MapData currentMap;
    private bool isPlacingTile = false;
    private bool isDeletingTile = false;

    // Tile type selector
    private TileType selectedTileType = TileType.Basic;

    [MenuItem("Window/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditorWindow>("Map Editor");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Map Settings", EditorStyles.boldLabel);

        gridWidth = EditorGUILayout.IntField("Grid Width", gridWidth);
        gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight);
        tileOffsetX = EditorGUILayout.FloatField("Tile Offset X", tileOffsetX);
        tileOffsetY = EditorGUILayout.FloatField("Tile Offset Y", tileOffsetY);
        tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(GameObject), false);

        // Tile type selection
        selectedTileType = (TileType)EditorGUILayout.EnumPopup("Tile Type", selectedTileType);

        if (GUILayout.Button("Generate Map"))
        {
            GenerateMap();
        }

        if (GUILayout.Button("Save Map"))
        {
            SaveMap();
        }

        if (GUILayout.Button("Load Map"))
        {
            LoadMap();
        }

        currentMap = (MapData)EditorGUILayout.ObjectField("Current Map", currentMap, typeof(MapData), false);

        if (GUILayout.Button("Enable Tile Placement"))
        {
            isPlacingTile = !isPlacingTile;
            isDeletingTile = false;
        }

        if (GUILayout.Button("Enable Tile Deletion"))
        {
            isDeletingTile = !isDeletingTile;
            isPlacingTile = false;
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Handles.color = Color.green;

        foreach (var tile in tiles.Keys)
        {
            Vector3 tilePosition = new Vector3(tile.x * tileOffsetX, 0, tile.y * tileOffsetY);
            Handles.DrawWireCube(tilePosition, new Vector3(tileOffsetX, 0.1f, tileOffsetY));
        }

        if (Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 1))
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector2Int gridPos = GetGridCoordinatesFromWorldPosition(hitPoint);

                if (isPlacingTile && Event.current.button == 0)
                {
                    PlaceTile(gridPos, selectedTileType);
                }
                else if (isDeletingTile && Event.current.button == 1)
                {
                    DeleteTile(gridPos);
                }
            }
            Event.current.Use();
        }

        // Add a visual guide for the grid
        Handles.color = Color.cyan;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y <= gridHeight; y++)  // Fixed: should be y <= gridHeight
            {
                Vector3 tilePosition = new Vector3(x * tileOffsetX, 0, y * tileOffsetY);
                Handles.DrawWireCube(tilePosition, new Vector3(tileOffsetX, 0.1f, tileOffsetY));
            }
        }

        SceneView.RepaintAll();
    }

    private Vector2Int GetGridCoordinatesFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / tileOffsetX);
        int y = Mathf.FloorToInt(worldPosition.z / tileOffsetY);
        return new Vector2Int(x, y);
    }

    private void PlaceTile(Vector2Int position, TileType tileType)
    {
        if (tiles.ContainsKey(position))
        {
            Debug.LogWarning("Tile already exists at this position.");
            return;
        }

        Vector3 tilePosition = new Vector3(position.x * tileOffsetX, 0, position.y * tileOffsetY);
        GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
        tile.name = $"Tile_{position.x}_{position.y}";
        tiles[position] = tile;

        // Set the tile type
        TileController tileController = tile.GetComponent<TileController>();
        if (tileController != null)
        {
            tileController.tileType = tileType;
        }

        if (currentMap != null)
        {
            MapData.TileData tileData = new MapData.TileData
            {
                position = position,
                tileType = tileType
            };
            currentMap.tilePositions.Add(tileData);
            EditorUtility.SetDirty(currentMap);
        }
    }

    private void DeleteTile(Vector2Int position)
    {
        if (!tiles.ContainsKey(position))
        {
            Debug.LogWarning("Tile does not exist at this position.");
            return;
        }

        GameObject tile = tiles[position];
        DestroyImmediate(tile);
        tiles.Remove(position);

        if (currentMap != null)
        {
            currentMap.tilePositions.RemoveAll(t => t.position == position);
            EditorUtility.SetDirty(currentMap);
        }
    }

    private void GenerateMap()
    {
        ClearMap();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 tilePosition = new Vector3(x * tileOffsetX, 0, y * tileOffsetY);
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tile.name = $"Tile_{x}_{y}";
                tiles[new Vector2Int(x, y)] = tile;

                // Set the tile type
                TileController tileController = tile.GetComponent<TileController>();
                if (tileController != null)
                {
                    tileController.tileType = selectedTileType;
                }

                if (currentMap != null)
                {
                    MapData.TileData tileData = new MapData.TileData
                    {
                        position = new Vector2Int(x, y),
                        tileType = selectedTileType
                    };
                    currentMap.tilePositions.Add(tileData);
                }
            }
        }

        if (currentMap != null)
        {
            EditorUtility.SetDirty(currentMap);
        }
    }

    private void ClearMap()
    {
        foreach (var tile in tiles.Values)
        {
            DestroyImmediate(tile);
        }
        tiles.Clear();
    }

    private void SaveMap()
    {
        if (currentMap != null)
        {
            currentMap.tilePositions.Clear();
            foreach (var tile in tiles)
            {
                TileController tileController = tile.Value.GetComponent<TileController>();
                if (tileController != null)
                {
                    MapData.TileData tileData = new MapData.TileData
                    {
                        position = tile.Key,
                        tileType = tileController.tileType
                    };
                    currentMap.tilePositions.Add(tileData);
                }
            }
            EditorUtility.SetDirty(currentMap);
            AssetDatabase.SaveAssets();
        }
        else
        {
            Debug.LogError("No map asset selected to save.");
        }
    }

    private void LoadMap()
    {
        if (currentMap != null)
        {
            ClearMap();
            foreach (var tileData in currentMap.tilePositions)
            {
                Vector3 tilePosition = new Vector3(tileData.position.x * tileOffsetX, 0, tileData.position.y * tileOffsetY);
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tile.name = $"Tile_{tileData.position.x}_{tileData.position.y}";
                tiles[tileData.position] = tile;

                // Load the tile type
                TileController tileController = tile.GetComponent<TileController>();
                if (tileController != null)
                {
                    tileController.tileType = tileData.tileType;
                }
            }
        }
        else
        {
            Debug.LogError("No map asset selected to load.");
        }
    }
}
