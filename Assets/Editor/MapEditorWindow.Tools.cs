using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class MapEditorWindow
{
    private void PlaceTile(Vector3Int position, TileType type, bool sync = true)
    {
        if (tilePrefab == null) return;
        if (sync) SyncDictionaryFromScene();
        if (tiles.ContainsKey(position)) return;

        Vector3 worldPos = GridToWorld(position, GetTileWorldSize3D());
        GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab);
        tile.transform.position = worldPos;
        tile.name = $"Tile_{position.x}_{position.y}_{position.z}";

        Undo.RegisterCreatedObjectUndo(tile, "Place Tile");
        tiles[position] = tile;

        var ctrl = tile.GetComponent<TileController>();
        if (ctrl) { ctrl.tileType = type; ctrl.gridPosition = position; }

        HideTileEffects(tile);
        ApplyDecorativeColor(tile, type);
    }

    private void PlaceDecoration(Vector3Int position, bool sync = true)
    {
        if (decorationPrefab == null) return;
        if (sync) SyncDictionaryFromScene();
        if (decorations.ContainsKey(position)) return;

        Vector3 worldPos = GridToWorld(position, GetTileWorldSize3D());
        GameObject deco = (GameObject)PrefabUtility.InstantiatePrefab(decorationPrefab);
        deco.transform.position = worldPos;
        deco.name = $"{decorationPrefab.name}_{position.x}_{position.y}_{position.z}";

        Undo.RegisterCreatedObjectUndo(deco, "Place Decoration");
        decorations[position] = deco;
    }

    private void PlaceUnit(Vector3Int position, bool sync = true)
    {
        if (unitPrefab == null) return;
        if (sync) SyncDictionaryFromScene();
        if (spawnedUnits.ContainsKey(position)) return;

        Vector3 worldPos = GridToWorld(position, GetTileWorldSize3D());
        GameObject unit = (GameObject)PrefabUtility.InstantiatePrefab(unitPrefab);
        unit.transform.position = worldPos;
        unit.name = $"SpawnUnit_{unitPrefab.name}_{position.x}_{position.y}_{position.z}";

        Undo.RegisterCreatedObjectUndo(unit, "Place Unit");
        spawnedUnits[position] = unit;
    }

    private void PlaceInteractable(Vector3Int position, bool sync = true)
    {
        if (interactablePrefab == null) return;
        if (sync) SyncDictionaryFromScene();
        if (spawnedInteractables.ContainsKey(position)) return;

        Vector3 worldPos = GridToWorld(position, GetTileWorldSize3D());
        GameObject interactable = (GameObject)PrefabUtility.InstantiatePrefab(interactablePrefab);
        interactable.transform.position = worldPos;
        interactable.name = $"SpawnInteractable_{interactablePrefab.name}_{position.x}_{position.y}_{position.z}";

        // Assign the ID!
        InteractableLink link = interactable.GetComponent<InteractableLink>();
        if (link != null) link.linkID = currentLinkID;

        Undo.RegisterCreatedObjectUndo(interactable, "Place Interactable");
        spawnedInteractables[position] = interactable;
    }

    // Add the PlaceEnemy method
    private void PlaceEnemy(Vector3Int position, bool sync = true)
    {
        if (enemyPrefab == null) return;
        if (sync) SyncDictionaryFromScene();
        if (spawnedEnemies.ContainsKey(position)) return;

        Vector3 worldPos = GridToWorld(position, GetTileWorldSize3D());
        GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
        enemy.transform.position = worldPos;
        enemy.name = $"SpawnEnemy_{enemyPrefab.name}_{position.x}_{position.y}_{position.z}";

        Undo.RegisterCreatedObjectUndo(enemy, "Place Enemy");
        spawnedEnemies[position] = enemy;
    }

    private void DeleteTile(Vector3Int position, bool sync = true)
    {
        if (sync) SyncDictionaryFromScene();

        // Use spawnedInteractables instead of spawnedBeacons
        if (spawnedInteractables.TryGetValue(position, out GameObject i) && i != null) { spawnedInteractables.Remove(position); Undo.DestroyObjectImmediate(i); }
        if (spawnedUnits.TryGetValue(position, out GameObject u) && u != null) { spawnedUnits.Remove(position); Undo.DestroyObjectImmediate(u); }
        if (decorations.TryGetValue(position, out GameObject d) && d != null) { decorations.Remove(position); Undo.DestroyObjectImmediate(d); }
        if (tiles.TryGetValue(position, out GameObject t) && t != null) { tiles.Remove(position); Undo.DestroyObjectImmediate(t); }
        if (spawnedEnemies.TryGetValue(position, out GameObject e) && e != null) { spawnedEnemies.Remove(position); Undo.DestroyObjectImmediate(e); }
    }

    // HELPER: Universally checks if ANY block (Tile, Deco, Unit) exists at this specific coordinate
    private bool HasBlockAt(Vector3Int pos)
    {
        return (tiles.TryGetValue(pos, out GameObject t) && t != null) ||
               (spawnedInteractables.TryGetValue(pos, out GameObject i) && i != null) ||
               (decorations.TryGetValue(pos, out GameObject d) && d != null) ||
               (spawnedUnits.TryGetValue(pos, out GameObject u) && u != null) ||
               (spawnedEnemies.TryGetValue(pos, out GameObject e) && e != null);
    }

    private void ApplyBucketFill(Vector3Int startPos)
    {
        if (!IsInsideGrid(startPos)) return;
        
        // Check if we clicked on an existing physics object
        tiles.TryGetValue(startPos, out GameObject startTile);
        bool targetHadTile = startTile != null;
        TileType? targetTileType = targetHadTile ? startTile.GetComponent<TileController>()?.tileType : null;

        // Prevent infinite loops when substituting same exact types
        if (isPlacingTile && targetHadTile && targetTileType == selectedTileType) return;

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        List<Vector3Int> pointsToFill = new List<Vector3Int>();

        queue.Enqueue(startPos);
        visited.Add(startPos);

        while (queue.Count > 0 && pointsToFill.Count < 5000) 
        {
            Vector3Int curr = queue.Dequeue();
            pointsToFill.Add(curr);

            if (targetHadTile)
            {
                // REPLACE MODE: 3D Flood Fill. Replace all contiguous blocks of returning type.
                Vector3Int[] neighbors3D = {
                    curr + Vector3Int.right, curr + Vector3Int.left,
                    curr + new Vector3Int(0, 0, 1), curr + new Vector3Int(0, 0, -1),
                    curr + Vector3Int.up, curr + Vector3Int.down
                };

                foreach (var n in neighbors3D)
                {
                    if (IsInsideGrid(n) && !visited.Contains(n))
                    {
                        if (tiles.TryGetValue(n, out GameObject neighborTile) && neighborTile != null)
                        {
                            if (neighborTile.GetComponent<TileController>()?.tileType == targetTileType)
                            {
                                visited.Add(n);
                                queue.Enqueue(n);
                            }
                        }
                    }
                }
            }
            else
            {
                // EMPTY SPACE MODE: 2D Flood Fill strictly confined to physics blockers and floors.
                Vector3Int[] neighborsHorizontal = {
                    curr + Vector3Int.right, curr + Vector3Int.left,
                    curr + new Vector3Int(0, 0, 1), curr + new Vector3Int(0, 0, -1)
                };

                foreach (var n in neighborsHorizontal)
                {
                    if (IsInsideGrid(n) && !visited.Contains(n))
                    {
                        // Safely check if there are absolutely zero blockers (Tiles, Decos, Units) in the way
                        bool isNeighborEmpty = !HasBlockAt(n);
                        
                        if (isNeighborEmpty)
                        {
                            // THE FLOOR RULE: Does this exact column have ANY block exactly 1 step beneath it?
                            bool hasFloorBeneath = n.y == 0 || HasBlockAt(new Vector3Int(n.x, n.y - 1, n.z));

                            if (hasFloorBeneath)
                            {
                                visited.Add(n);
                                queue.Enqueue(n);
                            }
                        }
                    }
                }
            }
        }

        // Apply bulk modifications quickly
        foreach (var p in pointsToFill)
        {
            if (isPlacingTile) { if(targetHadTile) DeleteTile(p, false); PlaceTile(p, selectedTileType, false); }
            else if (isPlacingDecoration) PlaceDecoration(p, false);
            else if (isPlacingUnit) PlaceUnit(p, false);
            else if (isDeletingTile) DeleteTile(p, false);
            else if (isPlacingEnemy) PlaceEnemy(p, false);
        }
        
        SyncDictionaryFromScene();
    }

    private void ApplyDecorativeColor(GameObject tile, TileType type)
    {
        Transform bounds = tile.transform.Find("GridBounds");
        Renderer r = bounds?.GetComponent<Renderer>();
        
        // Clean up any existing visual previews in case the tile type was changed
        for (int i = tile.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = tile.transform.GetChild(i);
            if (child.name == "EditorPreview") 
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }

        // --- NEW: Load Custom Resources Prefab for Editor Visualization ---
        string customPrefabName = null;
        if (type == TileType.MinibossChest) customPrefabName = "MiniBossChest";
        else if (type == TileType.BossChest) customPrefabName = "BossChest";
        else if (type == TileType.Chest) customPrefabName = "Chest";
        else if (type == TileType.DeityTile) customPrefabName = "DeityTile";
        else if (type == TileType.Beacon) customPrefabName = "Beacon";

        if (!string.IsNullOrEmpty(customPrefabName))
        {
            GameObject loadedPrefab = Resources.Load<GameObject>(customPrefabName);
            if (loadedPrefab != null)
            {
                GameObject preview = (GameObject)PrefabUtility.InstantiatePrefab(loadedPrefab);
                preview.name = "EditorPreview";
                preview.transform.SetParent(tile.transform);
                preview.transform.localPosition = new Vector3(0, 0f, 0); 
            }
        }
        // ------------------------------------------------------------------

        if (r == null) return;

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        r.GetPropertyBlock(block);

        Color c = Color.white;
        
        // Base coloring fallbacks if the prefabs don't exist yet
        if (type == TileType.Obstacle) c = new Color(0.15f, 0.15f, 0.15f, 1f);
        else if (type == TileType.Chest) c = new Color(0.5f, 0.0f, 0.8f, 1f);
        else if (type == TileType.MinibossChest) c = Color.yellow;
        else if (type == TileType.BossChest) c = Color.red;
        else if (type == TileType.DeityTile) c = Color.cyan;

        if (c != Color.white)
        {
            block.SetColor("_BaseColor", c);
            block.SetColor("_Color", c);
            r.SetPropertyBlock(block);
        }
        else
        {
            r.SetPropertyBlock(null); // Reset color to default for Basic tiles
        }
    }

    private List<Vector3Int> GetLinePoints(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> pts = new List<Vector3Int>();
        int steps = Mathf.Max(Mathf.Abs(start.x - end.x), Mathf.Max(Mathf.Abs(start.y - end.y), Mathf.Abs(start.z - end.z)));
        if (steps == 0) { pts.Add(start); return pts; }

        for (int i = 0; i <= steps; i++)
        {
            Vector3 l = Vector3.Lerp(start, end, (float)i / steps);
            pts.Add(new Vector3Int(Mathf.RoundToInt(l.x), Mathf.RoundToInt(l.y), Mathf.RoundToInt(l.z)));
        }
        return pts.Distinct().ToList();
    }

    private List<Vector3Int> GetBrushPoints(Vector3Int center, int size)
    {
        List<Vector3Int> pts = new List<Vector3Int>();
        int offStart = -(size - 1) / 2, offEnd = size / 2;
        for (int x = offStart; x <= offEnd; x++)
            for (int z = offStart; z <= offEnd; z++)
                pts.Add(new Vector3Int(center.x + x, center.y, center.z + z));
        return pts;
    }

    private void HideTileEffects(GameObject tile)
    {
        Transform effects = tile.transform.Find("TileEffects");
        if (effects != null) SceneVisibilityManager.instance.Hide(effects.gameObject, true);
    }

    // Free-form Environment Spawning
    private void SpawnEnvironmentProp()
    {
        if (environmentPrefab == null)
        {
            Debug.LogWarning("Map Editor: Select an Environment Prefab from the pool first!");
            return;
        }

        SyncDictionaryFromScene();

        GameObject envObj = (GameObject)PrefabUtility.InstantiatePrefab(environmentPrefab);
        
        if (SceneView.lastActiveSceneView != null)
        {
            envObj.transform.position = SceneView.lastActiveSceneView.pivot;
        }
        else
        {
            envObj.transform.position = Vector3.zero;
        }

        envObj.name = $"EnvProp_{environmentPrefab.name}_{System.Guid.NewGuid().ToString().Substring(0, 5)}";
        
        Undo.RegisterCreatedObjectUndo(envObj, "Spawn Environment Prop");
        spawnedEnvironments.Add(envObj);
        
        Selection.activeGameObject = envObj;
    }
}