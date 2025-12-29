using DG.Tweening;
using ProjectEdelweiss.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
    }

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
        SyncWithGrid();
        UpdateVisualRotation();
        UpdateBeamVisuals();
    }

    private void UpdateBeamVisuals()
    {
        // 1. Always start the beam at the Beacon's center
        _lineRenderer.SetPosition(0, transform.position);

        // 2. Get the path to find where the light ends
        List<TileController> targetTiles = GridManager.Instance.gridMovementController.GetTilesInDirection(tileX, tileY, _currentFacingDirection, range);

        if (targetTiles.Count > 0)
        {
            // Set end point to the center of the furthest tile in the path
            Vector3 endPos = targetTiles[targetTiles.Count - 1].transform.position;
            _lineRenderer.SetPosition(1, endPos);
        }
        else
        {
            // Fallback: If no tiles (edge of map), set end to beacon position (invisible beam)
            _lineRenderer.SetPosition(1, transform.position);
        }
    }

    // This is the method your Melee Unit calls when attacking the Beacon.
    public void OnHitByUnit()
    {
        RotateClockwise();
        ActivateBeaconEffect();
    }

    private void RotateClockwise()
    {
        int nextDir = ((int)_currentFacingDirection + 1) % 4;
        _currentFacingDirection = (FacingDirection)nextDir;

        UpdateVisualRotation();
        UpdateBeamVisuals(); // Add this line
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
                // WARNING, DRY Principle violation: Move into a dedicated helper class on Units.
                PlayStunFeedback(hitEnemy);
                Debug.Log($"{hitEnemy.name} was stunned by the beacon!");
            }
        }
    }

    //private void UpdateVisualRotation()
    //{
    //    // Rotates the actual GameObject so the Player sees where it's pointing
    //    // Assuming your "Up" sprite/model faces North by default
    //    float angle = (int)_currentFacingDirection * 90f;
    //    transform.rotation = Quaternion.Euler(0, 0, -angle);
    //}

    [SerializeField] private float rotationOffset = 0f; // Adjust this in the Inspector (e.g., 90, -90, or 180)

    private void UpdateVisualRotation()
    {
        // The angle is calculated based on the enum (0, 90, 180, 270)
        float angle = (int)_currentFacingDirection * 90f;

        // Apply the offset to align the model's "front" with the logic's "Up"
        // We use a local rotation on the Y axis for 3D or Z axis for 2D
        transform.rotation = Quaternion.Euler(0, angle + rotationOffset, 0);

        // If you are using DOTween for a smoother transition:
        // transform.DORotate(new Vector3(0, angle + rotationOffset, 0), 0.3f);
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

    private void PlayStunFeedback(Unit targetUnit)
    {
        // Define the Y offset for the VFX spawn position
        float yOffset = 1.0f;

        // Calculate the new spawn position with the Y offset
        Vector3 stunVFXSpawnPosition = targetUnit.transform.position + new Vector3(0, yOffset, 0);

        // Instantiate the VFX at the new position
        GameObject stunVFX = Instantiate(Resources.Load<GameObject>("StunAttackVFX"), stunVFXSpawnPosition, Quaternion.identity);

        // Get the duration of the VFX animation (you can set this to the actual duration of your VFX animation)
        float vfxDuration = 1.0f; // replace with the actual duration

        // Create a sequence
        Sequence sequence = DOTween.Sequence();

        // Add a delay to the sequence equal to the duration of the VFX
        sequence.AppendInterval(vfxDuration);

        // Add a callback to the sequence to instantiate the StunIcon after the delay
        sequence.AppendCallback(() =>
        {
            // Instantiate the StunIcon
            GameObject stunIconInstance = Instantiate(Resources.Load<GameObject>("StunIcon"), targetUnit.transform);
            GridManager.Instance.statusIcons.Add(stunIconInstance);

            // Create a sequence for the StunIcon animations
            Sequence iconSequence = DOTween.Sequence();

            // Add a scale up animation for the pop effect
            iconSequence.Append(stunIconInstance.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.2f).SetEase(Ease.OutBack));

            // Add a scale back to normal size
            iconSequence.Append(stunIconInstance.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));

            // Add a shake animation
            iconSequence.Append(stunIconInstance.transform.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0), 10, 90, false, true));

            // Play the icon sequence
            iconSequence.Play();
        });

        float stunVFXDestroyCountdown = 1.5f;
        Destroy(stunVFX, stunVFXDestroyCountdown);
    }

}