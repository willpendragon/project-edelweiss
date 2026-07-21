using DG.Tweening;
using ProjectEdelweiss.Settings;
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
    [SerializeField] private EnemyCameraSettings _enemyCameraSettings;

    // These settings change how the camera looks at the end of a battle.
    [SerializeField] private EndBattleCameraSettings _endBattleCameraSettings;
    
    [Header("Manual Camera Control")][SerializeField] private ManualCameraSettings _manualCameraSettings;

    // Manual camera control state
    private bool _isManualControlEnabled = false;
    private bool _isAutomaticPanningActive = false;
    private Vector3 _manualPanVelocity;
    private float _currentManualZoom;
    private float _zoomVelocity;
    
    // Camera boundaries
    private float _minX, _maxX, _minZ, _maxZ;

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
        
        // Calculate camera boundaries based on grid size
        CalculateCameraBoundaries();
        
        // Initialize manual zoom to current orthographic size
        if (_cameras != null && _cameras.Count > 0)
        {
            _currentManualZoom = _cameras[0].orthographicSize;
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Z))
        //{
        //    CameraCloseUp();
        //}

        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     DeityCameraCloseUp();
        // }
        
        // Manual camera control (only during player turn and when not automatically panning)
        if (_isManualControlEnabled && !_isAutomaticPanningActive && _manualCameraSettings != null)
        {
            HandleManualPanInput();
            HandleManualZoomInput();
            
            // Reset camera with configured key
            if (_manualCameraSettings.EnableReset && Input.GetKeyDown(_manualCameraSettings.ResetKey))
            {
                ResetToDefaultPosition();
            }
        }
    }

    private void OnEnable()
    {
        PhysicalAttackBehavior.OnKnockbackFired += CameraCloseUp;
        AOESpellPlayerAction.OnDeityAngered += DeityCameraCloseUp;
        UnitSelectionController.OnUnitTurnEnded += PanCameraToNextUnit;
        TurnController.OnPlayerTurn += HandlePlayerTurnCamera;
        TurnController.OnEnemyTurnSwap += HandleEnemyTurnCamera;
        MovePlayerAction.OnUnitMovedToTile += FollowActiveUnitMovement;
        EnemyTurnManager.OnEnemyTurnStarted += HandleIndividualEnemyTurnStart;
        EnemyTurnManager.OnPlayerTurnSwap += PanCameraToActiveUnit;
        EnemyTurnManager.OnDeityTurn += HandleDeityTurnCamera;
        
        // Manual camera control events
        TurnController.OnPlayerTurn += EnableManualCameraControl;
        TurnController.OnEnemyTurnSwap += DisableManualCameraControl;

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
        EnemyTurnManager.OnEnemyTurnStarted -= HandleIndividualEnemyTurnStart;
        EnemyTurnManager.OnPlayerTurnSwap -= PanCameraToActiveUnit;
        EnemyTurnManager.OnDeityTurn -= HandleDeityTurnCamera;
        
        // Manual camera control events
        TurnController.OnPlayerTurn -= EnableManualCameraControl;
        TurnController.OnEnemyTurnSwap -= DisableManualCameraControl;

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

    private void PanCameraToPosition(Vector3 targetPosition, float? targetZoom = null, float duration = -1f, Ease? overrideEase = null)
    {
        // Kill any existing tween and clear pending invokes to prevent conflicts
        _cameraTween?.Kill();
        CancelInvoke(nameof(ResetCameraPosition));

        if (_cameras == null || _cameras.Count == 0) return;
        
        // Block manual control during automatic panning
        _isAutomaticPanningActive = true;

        float zoomTarget = targetZoom ?? _battleCameraSettings.ZoomAmount;
        float panDuration = duration < 0 ? _cameraPanDuration : duration;

        Ease easeToUse = overrideEase ?? _cameraPanEase;

        foreach (var cam in _cameras)
        {
            Vector3 finalPosition = targetPosition + _battleCameraSettings.CameraOffset;
            
            // Clamp to boundaries before tweening to prevent camera from going outside valid area
            finalPosition.x = Mathf.Clamp(finalPosition.x, _minX, _maxX);
            finalPosition.z = Mathf.Clamp(finalPosition.z, _minZ, _maxZ);
            
            Debug.Log($"[Auto Pan to Character] Target tile: ({targetPosition.x:F2}, {targetPosition.z:F2}) | Camera will be at: ({finalPosition.x:F2}, {finalPosition.z:F2}) | Boundaries: X[{_minX:F1}, {_maxX:F1}], Z[{_minZ:F1}, {_maxZ:F1}]");

            _cameraTween = cam.transform.DOMove(finalPosition, panDuration).SetEase(easeToUse).OnComplete(() =>
            {
                // Re-enable manual control when automatic panning completes
                _isAutomaticPanningActive = false;
            });

            DOVirtual.Float(cam.fieldOfView, zoomTarget, panDuration, value =>
            {
                cam.fieldOfView = value;
            }).SetEase(easeToUse);
        }
    }
    public void PanCameraToActiveUnit()
    {
        var activeUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit);
        if (activeUnit != null)
        {
            var unit = activeUnit.GetComponent<Unit>();
            if (unit != null && unit.ownedTile != null)
            {
                PanCameraToPosition(unit.ownedTile.gameObject.transform.position);
                Debug.Log("Panning camera to active player unit at tile: " + unit.ownedTile.name);
            }
        }
    }

    private void FollowActiveUnitMovement(TileController targetTile)
    {
        if (targetTile != null)
        {
            // Use a faster follow duration to keep pace with character movement animation
            PanCameraToPosition(targetTile.gameObject.transform.position, null, _cameraFollowDuration);
        }
    }

    private void HandlePlayerTurnCamera(string turnMessage)
    {
        PanCameraToActiveUnit();
    }

    private void PanCameraToNextUnit()
    {
        ResetCameraPositionSmooth();

        // Alternative: Automatically pan to next available player unit
        // PanCameraToActiveUnit();
    }
    private void HandleEnemyTurnCamera()
    {
        PanCameraToFirstAvailableEnemy();
    }

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

    public void CameraCloseUp()
    {
        var activeUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit);
        if (activeUnit != null)
        {
            var tile = activeUnit.GetComponent<Unit>().ownedTile;
            UpdateCameraPosition(tile.gameObject.transform.position);
        }
    }
    public void DeityCameraCloseUp()
    {

        // These camera settings are applied only when the camera close up to the Deity after their Anger meter got full.
        var deity = FindObjectOfType<Deity>();
        if (deity != null)
        {
            Vector3 targetPos = deity.transform.position;
            var unit = deity.GetComponent<Unit>();
            if (unit != null && unit.ownedTile != null)
            {
                targetPos = unit.ownedTile.gameObject.transform.position;
            }
            float panTime = _enemyCameraSettings != null ? _enemyCameraSettings.AngeredPanDuration : _cameraPanDuration;
            Ease panEase = _enemyCameraSettings != null ? _enemyCameraSettings.AngeredPanEase : _cameraPanEase;
            float pauseTime = _enemyCameraSettings != null ? _enemyCameraSettings.AngeredPauseDuration : _battleCameraSettings.CameraResetDelay;

            PanCameraToPosition(targetPos + _deityFocusOffset, _battleCameraSettings.ZoomAmount, panTime, panEase);

            float totalDelay = panTime + pauseTime;

            DOVirtual.DelayedCall(totalDelay, () =>
            {
                if (this != null && gameObject.activeInHierarchy)
                {
                    PanCameraToActiveUnit();
                }
            });
        }
    }
    private void UpdateCameraPosition(Vector3 targetPosition)
    {
        // Kill current tweens and cancel any pending camera resets to prevent overlap jitter.
        _cameraTween?.Kill();
        CancelInvoke(nameof(ResetCameraPosition));
        
        // Block manual control during automatic panning
        _isAutomaticPanningActive = true;

        // Use a fast transition instead of an instant snap to smooth out back-to-back calls
        float fastTransitionTime = 0.25f;

        foreach (var cam in _cameras)
        {
            Vector3 finalTransform = targetPosition + _battleCameraSettings.CameraOffset;
            
            // Clamp to boundaries before tweening to prevent camera from going outside valid area
            finalTransform.x = Mathf.Clamp(finalTransform.x, _minX, _maxX);
            finalTransform.z = Mathf.Clamp(finalTransform.z, _minZ, _maxZ);

            // Smoothly slide to the closeup 
            _cameraTween = cam.transform.DOMove(finalTransform, fastTransitionTime).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                // Re-enable manual control when automatic panning completes
                _isAutomaticPanningActive = false;
            });

            DOVirtual.Float(cam.fieldOfView, _battleCameraSettings.ZoomAmount, fastTransitionTime, value =>
            {
                cam.fieldOfView = value;
            });
        }

        // Restart the reset timer from right now.
        // If another attack happens before this timer ends, this invoke gets canceled and restarted.
        Invoke(nameof(ResetCameraPosition), _battleCameraSettings.CameraResetDelay + fastTransitionTime);
    }
    private void ResetCameraPosition()
    {
        _cameraTween?.Kill();
        CancelInvoke(nameof(ResetCameraPosition));

        foreach (var cam in _cameras)
        {
            cam.transform.position = _originalCameraPosition;
            cam.fieldOfView = _originalZoomAmount;

            if (_generalCameraSettings != null)
            {
                cam.transform.eulerAngles = _generalCameraSettings.CameraRotation;
            }
        }
    }
    private void ResetCameraPositionSmooth()
    {
        _cameraTween?.Kill();
        CancelInvoke(nameof(ResetCameraPosition));

        if (_cameras == null || _cameras.Count == 0) return;

        foreach (var cam in _cameras)
        {
            _cameraTween = cam.transform.DOMove(_originalCameraPosition, _cameraPanDuration)
                .SetEase(_cameraPanEase);

            DOVirtual.Float(cam.fieldOfView, _originalZoomAmount, _cameraPanDuration, value =>
            {
                cam.fieldOfView = value;
            }).SetEase(_cameraPanEase);

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

    /// <summary>
    /// Pans camera to each enemy when their turn starts.
    /// The isTurnComplete flag in EnemyTurnManager ensures we wait for parry resolution before moving to next enemy.
    /// </summary>
    private void HandleIndividualEnemyTurnStart(EnemyAgent enemy)
    {
        if (enemy != null)
        {
            var unit = enemy.GetComponent<Unit>();
            if (unit != null && unit.ownedTile != null)
            {
                Debug.Log($"<color=magenta>[CameraController] Panning to {enemy.name} at turn start</color>");
                PanCameraToPosition(unit.ownedTile.gameObject.transform.position);
            }
        }
    }

    /// <summary>
    /// Called when an enemy performs a parryable attack. Locks camera on attacker.
    /// </summary>
    public void FocusOnEnemyAttack(TileController enemyTile)
    {
        if (enemyTile != null)
        {
            PanCameraToPosition(enemyTile.gameObject.transform.position);
        }
    }

    private void HandleDeityTurnCamera(string turnMessage)
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

            // Grab settings from the ScriptableObject to match the enemy feel
            float panTime = _enemyCameraSettings != null ? _enemyCameraSettings.PanDuration : _cameraPanDuration;
            Ease panEase = _enemyCameraSettings != null ? _enemyCameraSettings.PanEase : _cameraPanEase;

            PanCameraToPosition(targetPos + _deityFocusOffset, _battleCameraSettings.ZoomAmount, panTime, panEase);
        }
    }
    
    #region Manual Camera Control
    
    /// <summary>
    /// Calculates camera movement boundaries based on grid size and padding settings
    /// </summary>
    private void CalculateCameraBoundaries()
    {
        if (GridManager.Instance == null || _manualCameraSettings == null)
        {
            Debug.LogWarning("[CameraController] Cannot calculate boundaries: GridManager or ManualCameraSettings is null");
            return;
        }
        
        // Get actual world positions of all tiles to find grid extents
        if (GridManager.Instance.gridTileControllers == null || GridManager.Instance.gridTileControllers.Length == 0)
        {
            Debug.LogWarning("[CameraController] No tiles found for boundary calculation");
            return;
        }
        
        // Find the actual min/max positions of all tiles in world space
        float minWorldX = float.MaxValue;
        float maxWorldX = float.MinValue;
        float minWorldZ = float.MaxValue;
        float maxWorldZ = float.MinValue;
        
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            if (tile != null)
            {
                Vector3 tilePos = tile.transform.position;
                
                if (tilePos.x < minWorldX) minWorldX = tilePos.x;
                if (tilePos.x > maxWorldX) maxWorldX = tilePos.x;
                if (tilePos.z < minWorldZ) minWorldZ = tilePos.z;
                if (tilePos.z > maxWorldZ) maxWorldZ = tilePos.z;
            }
        }
        
        // Account for camera offset - boundaries should be based on camera positions, not tile positions
        // The camera sits at tile position + offset, so we need to adjust boundaries accordingly
        Vector3 cameraOffset = _battleCameraSettings.CameraOffset;
        float minCameraX = minWorldX + cameraOffset.x;
        float maxCameraX = maxWorldX + cameraOffset.x;
        float minCameraZ = minWorldZ + cameraOffset.z;
        float maxCameraZ = maxWorldZ + cameraOffset.z;
        
        // Apply padding from settings to create absolute world-space boundaries
        _minX = minCameraX - _manualCameraSettings.HorizontalBoundaryPadding;
        _maxX = maxCameraX + _manualCameraSettings.HorizontalBoundaryPadding;
        _minZ = minCameraZ - _manualCameraSettings.VerticalBoundaryPadding;
        _maxZ = maxCameraZ + _manualCameraSettings.VerticalBoundaryPadding;
        
        Debug.Log($"[CameraController] Camera boundaries calculated: X[{_minX:F1}, {_maxX:F1}], Z[{_minZ:F1}, {_maxZ:F1}] | Tile extents: X[{minWorldX:F1}, {maxWorldX:F1}], Z[{minWorldZ:F1}, {maxWorldZ:F1}] | Camera offset: {cameraOffset}");
    }
    
    /// <summary>
    /// Handles WASD/arrow key input for manual camera panning
    /// </summary>
    private void HandleManualPanInput()
    {
        if (_cameras == null || _cameras.Count == 0) return;
        
        // Get input axes
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
        float vertical = Input.GetAxis("Vertical"); // W/S or Up/Down arrows
        
        // Skip if no input
        if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
            return;
        
        Camera mainCam = _cameras[0];
        Vector3 currentPos = mainCam.transform.position;
        
        // For isometric camera: calculate movement relative to camera's orientation
        // Get camera's forward and right vectors projected onto the horizontal plane (XZ)
        Vector3 forward = mainCam.transform.forward;
        Vector3 right = mainCam.transform.right;
        
        // Project onto XZ plane (remove Y component) and normalize
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        // Calculate movement direction based on camera orientation
        Vector3 moveDirection = (right * horizontal + forward * vertical).normalized;
        
        // Calculate target position with snappy movement
        float moveAmount = _manualCameraSettings.PanSpeed * Time.deltaTime;
        Vector3 targetPosition = currentPos + moveDirection * moveAmount;
        
        // Clamp to boundaries
        targetPosition.x = Mathf.Clamp(targetPosition.x, _minX, _maxX);
        targetPosition.z = Mathf.Clamp(targetPosition.z, _minZ, _maxZ);
        
        // Apply with optional light damping for feel (lerp is faster than SmoothDamp)
        Vector3 newPosition = Vector3.Lerp(currentPos, targetPosition, 1f - _manualCameraSettings.PanDamping);
        
        // Final clamp to ensure boundaries are never exceeded
        newPosition.x = Mathf.Clamp(newPosition.x, _minX, _maxX);
        newPosition.z = Mathf.Clamp(newPosition.z, _minZ, _maxZ);
        
        // Press B during play to see boundary debug info
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log($"[Manual Pan] Pos: ({currentPos.x:F2}, {currentPos.z:F2}) | Boundaries: X[{_minX:F1}, {_maxX:F1}], Z[{_minZ:F1}, {_maxZ:F1}] | AtMinX: {Mathf.Approximately(currentPos.x, _minX)} | AtMaxX: {Mathf.Approximately(currentPos.x, _maxX)}");
        }
        
        foreach (var cam in _cameras)
        {
            cam.transform.position = newPosition;
        }
    }
    
    /// <summary>
    /// Handles mouse scroll wheel input for zoom (orthographic size)
    /// </summary>
    private void HandleManualZoomInput()
    {
        if (_cameras == null || _cameras.Count == 0) return;
        
        float scrollDelta = Input.mouseScrollDelta.y;
        
        // Debug: Press Z to see current state
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log($"[Zoom Debug] Manual control: {_isManualControlEnabled} | Auto-panning: {_isAutomaticPanningActive} | Current Size: {_cameras[0].orthographicSize:F1} | Target: {_currentManualZoom:F1} | Range: [{_manualCameraSettings.MinZoom}, {_manualCameraSettings.MaxZoom}]");
        }
        
        // Update target zoom based on scroll input
        if (!Mathf.Approximately(scrollDelta, 0f))
        {
            float oldTarget = _currentManualZoom;
            _currentManualZoom -= scrollDelta * _manualCameraSettings.ZoomSpeed;
            _currentManualZoom = Mathf.Clamp(_currentManualZoom, _manualCameraSettings.MinZoom, _manualCameraSettings.MaxZoom);
            
            Debug.Log($"[Manual Zoom] SCROLL DETECTED! Delta: {scrollDelta:F2} | Old: {oldTarget:F1} → New: {_currentManualZoom:F1} | Current Size: {_cameras[0].orthographicSize:F1} | ZoomSpeed: {_manualCameraSettings.ZoomSpeed}");
        }
        
        // Apply smooth zoom to all cameras EVERY FRAME (not just when scrolling)
        foreach (var cam in _cameras)
        {
            // Use Lerp for responsive zoom feel
            float newSize = Mathf.Lerp(cam.orthographicSize, _currentManualZoom, Time.deltaTime / _manualCameraSettings.ZoomSmoothTime);
            cam.orthographicSize = newSize;
        }
    }
    
    /// <summary>
    /// Enables manual camera control (called on player turn start)
    /// </summary>
    private void EnableManualCameraControl(string turnMessage)
    {
        _isManualControlEnabled = true;
        Debug.Log("[CameraController] Manual camera control enabled");
    }
    
    /// <summary>
    /// Disables manual camera control (called on enemy turn start)
    /// </summary>
    private void DisableManualCameraControl()
    {
        _isManualControlEnabled = false;
        Debug.Log("[CameraController] Manual camera control disabled");
    }
    
    /// <summary>
    /// Resets camera to the original/default position
    /// </summary>
    private void ResetToDefaultPosition()
    {
        if (_cameras == null || _cameras.Count == 0) return;
        
        // Smoothly return to original position
        foreach (var cam in _cameras)
        {
            cam.transform.DOMove(_originalCameraPosition, 0.5f).SetEase(Ease.OutQuad);
            
            DOVirtual.Float(cam.fieldOfView, _originalZoomAmount, 0.5f, value =>
            {
                cam.fieldOfView = value;
                _currentManualZoom = value;
            }).SetEase(Ease.OutQuad);
        }
        
        Debug.Log("[CameraController] Camera reset to default position");
    }
    
    #endregion
}