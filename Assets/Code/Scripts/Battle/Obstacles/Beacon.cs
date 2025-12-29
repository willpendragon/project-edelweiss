using ProjectEdelweiss.Utils;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : MonoBehaviour
{
    // Reordered to make clockwise rotation (0,1,2,3) easy with math
    public enum FacingDirection
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }

    [SerializeField] private int range = 5;
    [SerializeField] private FacingDirection _currentFacingDirection;

    public int tileX;
    public int tileY;

    private void Start()
    {
        // Optional: Sync visual rotation with starting direction.
        UpdateVisualRotation();
    }

    // This is the method your Melee Unit calls when attacking the Beacon.
    public void OnHitByUnit()
    {
        RotateClockwise();
        ActivateBeaconEffect();
    }

    private void RotateClockwise()
    {
        // Cycles: 0 -> 1 -> 2 -> 3 -> 0
        int nextDir = ((int)_currentFacingDirection + 1) % 4;
        _currentFacingDirection = (FacingDirection)nextDir;

        UpdateVisualRotation();
        Debug.Log($"Beacon rotated to: {_currentFacingDirection}");
    }

    private void ActivateBeaconEffect()
    {
        SyncWithGrid();
        List<TileController> targetTiles = GridManager.Instance.gridMovementController.GetTilesInDirection(tileX, tileY, _currentFacingDirection, range);

        foreach (TileController tile in targetTiles)
        {
            Debug.DrawLine(transform.position, tile.transform.position, Color.yellow, 0.5f);
            AttemptStupefyEnemy(tile);
        }
    }

    private void AttemptStupefyEnemy(TileController tile)
    {
        if (tile.detectedUnit != null && tile.detectedUnit.CompareTag(GameTags.Enemy))
        {
            Debug.Log($"{tile} was in the beacon path");

            var hitEnemy = tile.detectedUnit.GetComponent<Unit>();
            if (hitEnemy != null && hitEnemy.unitStatusController != null)
            {
                hitEnemy.unitStatusController.unitCurrentStatus = UnitStatus.stun;
                Debug.Log($"{hitEnemy.name} was stunned by the beacon!");
            }
        }
    }

    private void UpdateVisualRotation()
    {
        // Rotates the actual GameObject so the Player sees where it's pointing
        // Assuming your "Up" sprite/model faces North by default
        float angle = (int)_currentFacingDirection * 90f;
        transform.rotation = Quaternion.Euler(0, 0, -angle);
    }

    public void SyncWithGrid()
    {
        // Find the tile this beacon is currently overlapping
        // Assuming your GridManager has a way to get coordinates from world position
        Vector2Int coords = GridManager.Instance.GetGridCoordinatesFromWorldPosition(transform.position);

        tileX = coords.x;
        tileY = coords.y;

        Debug.Log($"Beacon Initialized at Grid: {tileX}, {tileY}");
    }
}