using DG.Tweening;
using ProjectEdelweiss.Utils;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 _originalCameraPosition;
    private float _originalZoomAmount;

    [Header("Settings")] [SerializeField] private BattleCameraSettings _battleCameraSettings;
    [SerializeField] private Vector3 _deityFocusOffset = new Vector3(0f, 3f, 0f);

    [SerializeField] private GeneralCameraSettings _generalCameraSettings;

    // These settings change how the camera looks at the end of a battle.
    [SerializeField] private EndBattleCameraSettings _endBattleCameraSettings;

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

        // Stop listening when disabled/destroyed
        if (_generalCameraSettings != null)
        {
            _generalCameraSettings.OnSettingsChanged -= ApplyGeneralCameraSettings;
        }

        if (_endBattleCameraSettings != null)
        {
            _endBattleCameraSettings.OnSettingsChanged -= ApplyBattleEndCameraSettings;
        }
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

    public void CameraCloseUp()
    {
        // Retrieve the position of the character
        var activeUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit);
        if (activeUnit != null)
        {
            var tile = activeUnit.GetComponent<Unit>().ownedTile;
            UpdateCameraPosition(tile.gameObject.transform.position);
        }
    }

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

    private void UpdateCameraPosition(Vector3 targetPosition)
    {
        foreach (var cam in _cameras)
        {
            Vector3 finalTransform = targetPosition + _battleCameraSettings.CameraOffset;
            cam.transform.position = finalTransform;
            cam.fieldOfView = _battleCameraSettings.ZoomAmount;
        }

        Invoke(nameof(ResetCameraPosition), _battleCameraSettings.CameraResetDelay);
    }

    private void ResetCameraPosition()
    {
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
}