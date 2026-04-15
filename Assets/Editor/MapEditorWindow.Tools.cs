using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class MapEditorWindow
{
    // Note the default 'sync = true' parameters here. This fixes the error you were seeing!
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

    private void DeleteTile(Vector3Int position, bool sync = true)
    {
        if (sync) SyncDictionaryFromScene();

        if (spawnedUnits.TryGetValue(position, out GameObject u)) { spawnedUnits.Remove(position); Undo.DestroyObjectImmediate(u); }
        if (decorations.TryGetValue(position, out GameObject d)) { decorations.Remove(position); Undo.DestroyObjectImmediate(d); }
        if (tiles.TryGetValue(position, out GameObject t)) { tiles.Remove(position); Undo.DestroyObjectImmediate(t); }
    }

    private void ApplyBucketFill(Vector3Int startPos)
    {
        if (!IsInsideGrid(startPos)) return;
        bool targetHadTile = tiles.ContainsKey(startPos);
        TileType? targetTileType = targetHadTile ? tiles[startPos].GetComponent<TileController>()?.tileType : null;

        if (isPlacingTile && targetHadTile && targetTileType == selectedTileType) return;

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        List<Vector3Int> pointsToFill = new List<Vector3Int>();

        queue.Enqueue(startPos);
        visited.Add(startPos);

        while (queue.Count > 0 && pointsToFill.Count < 3000)
        {
            Vector3Int curr = queue.Dequeue();
            pointsToFill.Add(curr);

            Vector3Int[] neighbors = { curr + Vector3Int.right, curr + Vector3Int.left, curr + new Vector3Int(0, 0, 1), curr + new Vector3Int(0, 0, -1) };
            foreach (var n in neighbors)
            {
                if (!IsInsideGrid(n) || visited.Contains(n)) continue;

                bool nHasTile = tiles.ContainsKey(n);
                bool match = (!targetHadTile && !nHasTile) || (targetHadTile && nHasTile && tiles[n].GetComponent<TileController>()?.tileType == targetTileType);

                if (match) { visited.Add(n); queue.Enqueue(n); }
            }
        }

        foreach (var p in pointsToFill)
        {
            if (isPlacingTile) { if (targetHadTile) DeleteTile(p, false); PlaceTile(p, selectedTileType, false); }
            else if (isPlacingDecoration) PlaceDecoration(p, false);
            else if (isPlacingUnit) PlaceUnit(p, false);
            else if (isDeletingTile) DeleteTile(p, false);
        }
        SyncDictionaryFromScene();
    }

    private void ApplyDecorativeColor(GameObject tile, TileType type)
    {
        Transform bounds = tile.transform.Find("GridBounds");
        Renderer r = bounds?.GetComponent<Renderer>();
        if (r == null) return;

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        r.GetPropertyBlock(block);

        Color c = Color.white;
        if (type == TileType.Obstacle) c = new Color(0.15f, 0.15f, 0.15f, 1f);
        else if (type == TileType.Chest) c = new Color(0.5f, 0.0f, 0.8f, 1f);
        else if (type == TileType.MinibossChest) c = Color.yellow;
        else if (type == TileType.BossChest) c = Color.red;

        if (c != Color.white)
        {
            block.SetColor("_BaseColor", c);
            block.SetColor("_Color", c);
            r.SetPropertyBlock(block);
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
}