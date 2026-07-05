using DG.Tweening;
using ProjectEdelweiss.Utils;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 _originalCameraPosition;
    private float _originalZoomAmount;
    private Tween _cameraTween;

    [Header("Settings")][SerializeField] private BattleCameraSettings _battleCameraSettings;
    [SerializeField] private Vector3 _deityFocusOffset = new Vector3(0f, 3f, 0f);

    [SerializeField] private GeneralCameraSettings _generalCameraSettings;

    // These settings change how the camera looks at the end of a battle.
    [SerializeField] private EndBattleCameraSettings _endBattleCameraSettings;

    [Header("Camera Transition")][SerializeField] private float _cameraPanDuration = 0.8f;
    [SerializeField] private float _cameraFollowDuration = 0.6f;
    [SerializeField] private Ease _cameraPanEase = Ease.InOutQuad;

    [Header("Cameras")] public List<Camera> _cameras;



    private void Start()
    {
        // Check if GridManager has an active map with an override
        var mapData = GridManager.Instance != null ? GridManager.Instance.currentMapData : null;

        if (mapData != null && mapData.overrideCameraSettings)
        {
            ApplyMapCameraSettings(mapData.cameraPosition, mapData.cameraRotation, mapData.cameraZoom,
                mapData.isOrthographic, mapData.orthographicSize);
            ResetCameraPosition();
        }
        else
        {
            ApplyGeneralCameraSettings();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CameraCloseUp();
        }

        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     DeityCameraCloseUp();
        // }
    }

    private void OnEnable()
    {
        PhysicalAttackBehavior.OnKnockbackFired += CameraCloseUp;
        AOESpellPlayerAction.OnDeityAngered += DeityCameraCloseUp;
        UnitSelectionController.OnUnitTurnEnded += PanCameraToNextUnit;
        TurnController.OnPlayerTurn += HandlePlayerTurnCamera;
        TurnController.OnEnemyTurnSwap += HandleEnemyTurnCamera;
        MovePlayerAction.OnUnitMovedToTile += FollowActiveUnitMovement;

        BumperEnemyBehavior.OnEnemyActionFocusRequested += HandleEnemyFocus;

        // Listen to live tweaks from the ScriptableObject
        if (_generalCameraSettings != null)
        {
            _generalCameraSettings.OnSettingsChanged += ApplyGeneralCameraSettings;
        }

        if (_endBattleCameraSettings != null)
        {
            _endBattleCameraSettings.OnSettingsChanged += ApplyBattleEndCameraSettings;
        }
    }

    private void OnDisable()
    {
        PhysicalAttackBehavior.OnKnockbackFired -= CameraCloseUp;
        AOESpellPlayerAction.OnDeityAngered -= DeityCameraCloseUp;
        UnitSelectionController.OnUnitTurnEnded -= PanCameraToNextUnit;
        TurnController.OnPlayerTurn -= HandlePlayerTurnCamera;
        TurnController.OnEnemyTurnSwap -= HandleEnemyTurnCamera;
        MovePlayerAction.OnUnitMovedToTile -= FollowActiveUnitMovement;
        BumperEnemyBehavior.OnEnemyActionFocusRequested -= HandleEnemyFocus;

        // Stop listening when disabled/destroyed
        if (_generalCameraSettings != null)
        {
            _generalCameraSettings.OnSettingsChanged -= ApplyGeneralCameraSettings;
        }

        if (_endBattleCameraSettings != null)
        {
            _endBattleCameraSettings.OnSettingsChanged -= ApplyBattleEndCameraSettings;
        }

        // Kill any active camera tween to prevent lingering animations
        _cameraTween?.Kill();
    }

    public void ApplyGeneralCameraSettings()
    {
        if (_generalCameraSettings == null || _cameras == null || _cameras.Count == 0) return;

        foreach (var cam in _cameras)
        {
            cam.transform.position = _generalCameraSettings.CameraPosition;
            cam.transform.eulerAngles = _generalCameraSettings.CameraRotation;
            cam.fieldOfView = _generalCameraSettings.ZoomAmount;
        }

        // Keep the original references updated so battle camera reset works correctly
        _originalCameraPosition = _cameras[0].transform.position;
        _originalZoomAmount = _cameras[0].fieldOfView;
    }

    public void ApplyMapCameraSettings(Vector3 position, Vector3 rotation, float zoom, bool isOrtho, float orthoSize)
    {
        if (_cameras == null || _cameras.Count == 0) return;

        foreach (var cam in _cameras)
        {
            // Detach positioning so parent wrappers don't skew vectors
            cam.transform.position = position;
            cam.transform.eulerAngles = rotation;
            cam.orthographic = isOrtho;

            if (isOrtho)
            {
                cam.orthographicSize = orthoSize;
            }
            else
            {
                cam.fieldOfView = zoom;
            }
        }

        _originalCameraPosition = _cameras[0].transform.position;
        _originalZoomAmount = isOrtho ? _cameras[0].orthographicSize : _cameras[0].fieldOfView;
    }

    [ContextMenu("Save Current Camera To Settings")]
    public void UpdateSettingsFromCurrentCamera()
    {
        if (_generalCameraSettings == null || _cameras.Count == 0) return;

        // Take the first camera as the source of truth and save to the SO
        _generalCameraSettings.CameraPosition = _cameras[0].transform.position;
        _generalCameraSettings.CameraRotation = _cameras[0].transform.eulerAngles;
        _generalCameraSettings.ZoomAmount = _cameras[0].fieldOfView;

        Debug.Log("Saved current camera transforms to GeneralCameraSettings SO.");
    }

    /// <summary>
    /// Smoothly pans camera to a given target position with optional zoom adjustment.
    /// </summary>
    private void PanCameraToPosition(Vector3 targetPosition, float? targetZoom = null, float duration = -1f)
    {
        // Kill any existing tween to prevent conflicts
        _cameraTween?.Kill();

        if (_cameras == null || _cameras.Count == 0) return;

        float zoomTarget = targetZoom ?? _battleCameraSettings.ZoomAmount;
        float panDuration = duration < 0 ? _cameraPanDuration : duration;

        foreach (var cam in _cameras)
        {
            Vector3 finalPosition = targetPosition + _battleCameraSettings.CameraOffset;

            // Tween position
            _cameraTween = cam.transform.DOMove(finalPosition, panDuration).SetEase(_cameraPanEase);

            // Tween zoom in parallel
            DOVirtual.Float(cam.fieldOfView, zoomTarget, panDuration, value =>
            {
                cam.fieldOfView = value;
            }).SetEase(_cameraPanEase);
        }
    }

    /// <summary>
    /// Called when player selects a unit. Camera smoothly pans to the active player unit.
    /// </summary>
    public void PanCameraToActiveUnit()
    {
        var activeUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit);
        if (activeUnit != null)
        {
            var unit = activeUnit.GetComponent<Unit>();
            if (unit != null && unit.ownedTile != null)
            {
                PanCameraToPosition(unit.ownedTile.gameObject.transform.position);
            }
        }
    }

    /// <summary>
    /// Called when active unit moves to a new tile. Camera follows smoothly with a faster duration.
    /// </summary>
    private void FollowActiveUnitMovement(TileController targetTile)
    {
        if (targetTile != null)
        {
            // Use a faster follow duration to keep pace with character movement animation
            PanCameraToPosition(targetTile.gameObject.transform.position, null, _cameraFollowDuration);
        }
    }

    /// <summary>
    /// Called when player turn begins. Pans camera to first available active player unit.
    /// </summary>
    private void HandlePlayerTurnCamera(string turnMessage)
    {
        PanCameraToActiveUnit();
    }

    /// <summary>
    /// Called when the unit turn ends to reset camera. Optional: pan to next available unit.
    /// </summary>
    private void PanCameraToNextUnit()
    {
        // Option 1: Pan back to general overview
        ResetCameraPositionSmooth();

        // Option 2 (Alternative): Automatically pan to next available player unit
        // PanCameraToActiveUnit();
    }

    /// <summary>
    /// Called when enemy turn starts. Pans camera to cycle through enemies on battlefield.
    /// </summary>
    private void HandleEnemyTurnCamera()
    {
        PanCameraToFirstAvailableEnemy();
    }

    /// <summary>
    /// Pans camera to the first available living enemy on the battlefield.
    /// </summary>
    private void PanCameraToFirstAvailableEnemy()
    {
        var turnController = TurnController.Instance;
        if (turnController == null || turnController.enemyUnitsOnBattlefield == null) return;

        foreach (var enemy in turnController.enemyUnitsOnBattlefield)
        {
            if (enemy != null)
            {
                var unit = enemy.GetComponent<Unit>();
                if (unit != null && unit.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
                {
                    if (unit.ownedTile != null)
                    {
                        PanCameraToPosition(unit.ownedTile.gameObject.transform.position);
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Close-up camera effect for knockback or special actions on active player unit.
    /// </summary>
    public void CameraCloseUp()
    {
        var activeUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit);
        if (activeUnit != null)
        {
            var tile = activeUnit.GetComponent<Unit>().ownedTile;
            UpdateCameraPosition(tile.gameObject.transform.position);
        }
    }

    /// <summary>
    /// Close-up camera effect on a Deity unit.
    /// </summary>
    public void DeityCameraCloseUp()
    {
        var deity = FindObjectOfType<Deity>();
        if (deity != null)
        {
            Vector3 targetPos = deity.transform.position;
            var unit = deity.GetComponent<Unit>();
            if (unit != null && unit.ownedTile != null)
            {
                targetPos = unit.ownedTile.gameObject.transform.position;
            }

            // Add the inspector offset to adjust the focus (e.g., higher up to see the face)
            UpdateCameraPosition(targetPos + _deityFocusOffset);
        }
    }

    /// <summary>
    /// Instant camera move with optional auto-reset delay.
    /// </summary>
    private void UpdateCameraPosition(Vector3 targetPosition)
    {
        // Kill any smooth pan tween
        _cameraTween?.Kill();

        foreach (var cam in _cameras)
        {
            Vector3 finalTransform = targetPosition + _battleCameraSettings.CameraOffset;
            cam.transform.position = finalTransform;
            cam.fieldOfView = _battleCameraSettings.ZoomAmount;
        }

        Invoke(nameof(ResetCameraPosition), _battleCameraSettings.CameraResetDelay);
    }

    /// <summary>
    /// Instantly reset camera to original position.
    /// </summary>
    private void ResetCameraPosition()
    {
        // Kill any active tween
        _cameraTween?.Kill();

        foreach (var cam in _cameras)
        {
            cam.transform.position = _originalCameraPosition;
            cam.fieldOfView = _originalZoomAmount;

            // Re-apply original rotation if needed
            if (_generalCameraSettings != null)
            {
                cam.transform.eulerAngles = _generalCameraSettings.CameraRotation;
            }
        }
    }

    /// <summary>
    /// Smoothly reset camera to original position using DoTween.
    /// </summary>
    private void ResetCameraPositionSmooth()
    {
        // Kill any existing tween
        _cameraTween?.Kill();

        if (_cameras == null || _cameras.Count == 0) return;

        foreach (var cam in _cameras)
        {
            // Tween position back to original
            _cameraTween = cam.transform.DOMove(_originalCameraPosition, _cameraPanDuration)
                .SetEase(_cameraPanEase);

            // Tween zoom back to original
            DOVirtual.Float(cam.fieldOfView, _originalZoomAmount, _cameraPanDuration, value =>
            {
                cam.fieldOfView = value;
            }).SetEase(_cameraPanEase);

            // Re-apply original rotation if needed
            if (_generalCameraSettings != null)
            {
                cam.transform.DORotate(_generalCameraSettings.CameraRotation, _cameraPanDuration)
                    .SetEase(_cameraPanEase);
            }
        }
    }

    public void ApplyBattleEndCameraSettings()
    {
        if (_endBattleCameraSettings == null || _cameras == null || _cameras.Count == 0) return;

        foreach (var cam in _cameras)
        {
            cam.transform.position = _endBattleCameraSettings.CameraPosition;
            cam.transform.eulerAngles = _endBattleCameraSettings.CameraRotation;
            cam.fieldOfView = _endBattleCameraSettings.ZoomAmount;
        }
    }

    private void HandleEnemyFocus(TileController targetTile, float duration)
    {
        if (targetTile != null)
        {
            PanCameraToPosition(targetTile.gameObject.transform.position, null, duration);
        }
    }
}