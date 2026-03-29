using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

public class MapEditorWindow : EditorWindow
{
    private int gridWidth = 10;
    private int gridHeight = 10;
    private int gridDepth = 10; // Nuova: Limite massimo di altezza Y
    private GameObject tilePrefab;
    private MapData currentMap;
    private TileType selectedTileType = TileType.Basic;

    // SCAMBIO: Da Vector2Int a Vector3Int
    private Dictionary<Vector3Int, GameObject> tiles = new Dictionary<Vector3Int, GameObject>();

    private bool isPlacingTile = false;
    private bool isDeletingTile = false;
    private Vector3Int lastGridPosition = new Vector3Int(-1, -1, -1);

    [MenuItem("Window/Map Editor")]
    public static void ShowWindow() => GetWindow<MapEditorWindow>("Map Editor");

    private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    private void OnGUI()
    {
        GUILayout.Label("Map Settings", EditorStyles.boldLabel);
        gridWidth = EditorGUILayout.IntField("Grid Width (X)", gridWidth);
        gridDepth = EditorGUILayout.IntField("Grid Depth/Height (Y)", gridDepth);
        gridHeight = EditorGUILayout.IntField("Grid Length (Z)", gridHeight);
        
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

        if (GUILayout.Button("Save Map to Asset")) SaveMap();
        if (GUILayout.Button("Load Map from Asset")) LoadFromAsset();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        if (isPlacingTile || isDeletingTile)
        {
            HandleUtility.AddDefaultControl(controlID);
        }

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        
        bool hasHit = false;
        Vector3Int targetGridPos = Vector3Int.zero;

        // Voxel Raycasting Robusto: Cerca Collider Fisici
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            hasHit = true;
            
            // Cerchiamo di capire se abbiamo colpito un Tile
            TileController hitTile = hitInfo.collider.GetComponentInParent<TileController>();
            
            if (hitTile != null)
            {
                // Se colpiamo un Tile, leggiamo la SUA esatta posizione dalla sua memoria
                Vector3Int basePos = hitTile.gridPosition;
                
                // La "Normale" ci dice quale faccia del tile abbiamo colpito (es. Su=0,1,0 ; Destra=1,0,0)
                Vector3 hitNormal = hitInfo.normal;
                Vector3Int offset = new Vector3Int(
                    Mathf.RoundToInt(hitNormal.x),
                    Mathf.RoundToInt(hitNormal.y),
                    Mathf.RoundToInt(hitNormal.z)
                );

                if (isDeletingTile)
                {
                    // Se stiamo cancellando, vogliamo l'esatta cella del tile colpito
                    targetGridPos = basePos;
                }
                else
                {
                    // Se stiamo piazzando, vogliamo la cella "accanto" alla faccia colpita
                    targetGridPos = basePos + offset;
                }
            }
            else
            {
                // Abbiamo colpito qualcos'altro (non un Tile)
                Vector3 offsetPos = hitInfo.point + (isDeletingTile ? -hitInfo.normal : hitInfo.normal) * 0.1f;
                targetGridPos = GetGridCoordinatesFromWorldPosition(offsetPos);
            }
        }
        else
        {
            // Se non colpiamo nulla, ricadiamo sul "Piano Terra" (Y = 0)
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                hasHit = true;
                Vector3 hitPoint = ray.GetPoint(enter);
                targetGridPos = GetGridCoordinatesFromWorldPosition(hitPoint);
                targetGridPos.y = 0; // Forza al pavimento
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
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x / size.x), 
            Mathf.FloorToInt(worldPos.y / size.y), 
            Mathf.FloorToInt(worldPos.z / size.z)
        );
    }

    private Vector3 GridToWorld(Vector3Int gridPos, Vector3 tileSize)
    {
        // Y è moltiplicata linearmente (senza offset metà tile) assumendo che i pivot siano alla base o al centro, adatta se hai un pivot diverso.
        // Assumiamo che il Pivot del Tile sia Cetrato, altrimenti togli '+ (tileSize... * 0.5f)'
        float x = gridPos.x * tileSize.x + (tileSize.x * 0.5f);
        float y = gridPos.y * tileSize.y; // Se il pivot è al centro cambia in: gridPos.y * tileSize.y + (tileSize.y * 0.5f)
        float z = gridPos.z * tileSize.z + (tileSize.z * 0.5f);
        return new Vector3(x, y, z);
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
                // Tile_X_Y_Z
                if (parts.Length == 4 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y) && int.TryParse(parts[3], out int z))
                {
                    tiles[new Vector3Int(x, y, z)] = t;
                }
                // Retrocompatibilità
                else if (parts.Length == 3 && int.TryParse(parts[1], out int oldX) && int.TryParse(parts[2], out int oldZ))
                {
                    tiles[new Vector3Int(oldX, 0, oldZ)] = t;
                }
            }
        }
    }

    private void DrawGrid()
    {
        Vector3 size = GetTileWorldSize3D();
        Handles.color = new Color(0, 1, 1, 0.2f);
        
        // Disegno solo la griglia base (y = 0) per non intasare l'inquadratura
        for (int x = 0; x <= gridWidth; x++)
            Handles.DrawLine(new Vector3(x * size.x, 0, 0), new Vector3(x * size.x, 0, gridHeight * size.z));
        for (int z = 0; z <= gridHeight; z++)
            Handles.DrawLine(new Vector3(0, 0, z * size.z), new Vector3(gridWidth * size.x, 0, z * size.z));
    }

    private void DrawPreview(Vector3Int gridPos)
    {
        Vector3 size = GetTileWorldSize3D();
        
        // Adatta il pivot Y come in GridToWorld()
        float centerX = gridPos.x * size.x + (size.x * 0.5f);
        float centerY = gridPos.y * size.y + (size.y * 0.5f); // Se il pivot Y è alla base. Adjust if Center Pivot.
        float centerZ = gridPos.z * size.z + (size.z * 0.5f);

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
            // Proviamo a estrarre la grandezza dal Collider se c'è
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
                position = kvp.Key, // E' già un Vector3Int!
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
        }
    }

    private void GenerateMap()
    {
        SyncDictionaryFromScene();
        foreach (var obj in tiles.Values) Undo.DestroyObjectImmediate(obj);
        tiles.Clear();
    }
}