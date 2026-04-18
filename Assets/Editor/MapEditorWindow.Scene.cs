using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class MapEditorWindow
{
    // Variables to remember what tool we were using before holding Right-Click
    private bool _storedTile, _storedDeco, _storedUnit, _storedInteractable, _storedBucket;
    private bool _isRightClickDeleting = false;

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Escape)
            {
                SetMode();
                isBucketMode = false;
                Repaint(); sceneView.Repaint(); e.Use(); return;
            }
            if (e.keyCode >= KeyCode.Alpha1 && e.keyCode <= KeyCode.Alpha6)
            {
                brushSize = (e.keyCode - KeyCode.Alpha1) + 1;
                Repaint(); sceneView.Repaint(); e.Use();
            }
            if (e.keyCode == KeyCode.B)
            {
                SetMode(bucket: !isBucketMode);
                Repaint(); sceneView.Repaint(); e.Use();
            }
        }

        // --- TEMPORARY RIGHT-CLICK DELETE OVERRIDE ---
        if (e.type == EventType.MouseDown && e.button == 1 && !e.alt)
        {
            // 1. Save current states
            _storedTile = isPlacingTile;
            _storedDeco = isPlacingDecoration;
            _storedUnit = isPlacingUnit;
            _storedInteractable = isPlacingInteractable;
            _storedBucket = isBucketMode;

            // 2. Force to Delete Mode
            SetMode(delete: true);
            isBucketMode = false;
            _isRightClickDeleting = true;

            Repaint(); sceneView.Repaint(); 
        }
        else if (e.type == EventType.MouseUp && e.button == 1 && _isRightClickDeleting)
        {
            // 3. Restore previous states on release
            SetMode(tile: _storedTile, deco: _storedDeco, unit: _storedUnit, interactable: _storedInteractable, delete: false, bucket: _storedBucket);
            _isRightClickDeleting = false;

            Repaint(); sceneView.Repaint(); 
        }
        // ---------------------------------------------

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        if (isPlacingTile || isPlacingDecoration || isPlacingInteractable || isPlacingUnit || isDeletingTile || isBucketMode)
            HandleUtility.AddDefaultControl(controlID);

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        bool hasHit = false;
        Vector3Int targetGridPos = Vector3Int.zero;
        Vector3 tileSize = GetTileWorldSize3D();
        float closestDist = float.MaxValue;

        // 1. Raycast over physics tiles first to establish the baseline hit distance
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            hasHit = true;
            closestDist = hitInfo.distance;
            TileController hitTile = hitInfo.collider.GetComponentInParent<TileController>();

            if (hitTile != null)
            {
                Vector3Int basePos = hitTile.gridPosition;
                Vector3Int normalOff = new Vector3Int(Mathf.RoundToInt(hitInfo.normal.x), Mathf.RoundToInt(hitInfo.normal.y), Mathf.RoundToInt(hitInfo.normal.z));
                
                // If Painting: target the face you clicked (allows stacking sideways and vertically!)
                targetGridPos = isDeletingTile ? basePos : basePos + normalOff;
            }
            else
            {
                Vector3 offsetPos = hitInfo.point + (isDeletingTile ? -hitInfo.normal : hitInfo.normal) * 0.1f;
                targetGridPos = GetGridCoordinatesFromWorldPosition(offsetPos);
            }
        }

        // 2. Raycast over custom lists (Decos, Units, Interactables) to see if they are CLOSER to the camera
        void CheckHits(Dictionary<Vector3Int, GameObject> dict)
        {
            foreach (var kvp in dict)
            {
                Bounds bounds = new Bounds(GridToWorld(kvp.Key, tileSize) + tileSize / 2f, tileSize);
                if (bounds.IntersectRay(ray, out float dist) && dist < closestDist)
                {
                    closestDist = dist;
                    targetGridPos = kvp.Key;
                    
                    if (!isDeletingTile) targetGridPos.y += 1;
                    
                    hasHit = true;
                }
            }
        }
        
        CheckHits(decorations);
        CheckHits(spawnedUnits);
        CheckHits(spawnedInteractables);

        // 3. Fallback back to the empty Ground Plane if absolutely nothing was hit natively
        if (!hasHit)
        {
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                hasHit = true;
                targetGridPos = GetGridCoordinatesFromWorldPosition(ray.GetPoint(enter));
                targetGridPos.y = 0;
            }
        }

        if (hasHit && !e.alt)
        {
            List<Vector3Int> pointsToAffect = new List<Vector3Int>();

            if (isBucketMode) DrawPreview(targetGridPos); 
            else
            {
                if (e.shift && lastPaintedPosition.x != -1)
                {
                    foreach (var center in GetLinePoints(lastPaintedPosition, targetGridPos)) 
                        pointsToAffect.AddRange(GetBrushPoints(center, brushSize));
                }
                else pointsToAffect.AddRange(GetBrushPoints(targetGridPos, brushSize));

                pointsToAffect = pointsToAffect.Distinct().ToList();
                foreach (var p in pointsToAffect.Where(IsInsideGrid)) DrawPreview(p);
            }

            // Ensure BOTH button 0 (Left) and button 1 (Right) are allowed to process actions
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && (e.button == 0 || e.button == 1))
            {
                double currentTime = EditorApplication.timeSinceStartup;
                if (e.type == EventType.MouseDrag && targetGridPos == lastGridPosition) { e.Use(); return; }

                if ((currentTime - lastPaintTime) > paintDelay)
                {
                    if (isBucketMode && e.type == EventType.MouseDown) ApplyBucketFill(targetGridPos);
                    else if (!isBucketMode)
                    {
                        foreach (var p in pointsToAffect.Where(IsInsideGrid))
                        {
                            if (isPlacingTile) PlaceTile(p, selectedTileType, false);
                            else if (isPlacingInteractable) PlaceInteractable(p, false);
                            else if (isPlacingDecoration) PlaceDecoration(p, false);
                            else if (isPlacingUnit) PlaceUnit(p, false);
                            else if (isDeletingTile) DeleteTile(p, false); // <--- This effortlessly handles the right click now
                        }
                        SyncDictionaryFromScene();
                    }

                    lastGridPosition = targetGridPos;
                    lastPaintedPosition = targetGridPos; 
                    lastPaintTime = currentTime;
                }
                e.Use();
            }
        }

        if (e.type == EventType.MouseUp) lastGridPosition = new Vector3Int(-1, -1, -1);
        DrawGrid(); sceneView.Repaint();
    }
}