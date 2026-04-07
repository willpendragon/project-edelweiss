using DG.Tweening;
using ProjectEdelweiss.Utils;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 _originalCameraPosition;
    private float _originalZoomAmount;

    [Header("Settings")]
    [SerializeField] private BattleCameraSettings _battleCameraSettings;
    [SerializeField] private GeneralCameraSettings _generalCameraSettings;
    
    [Header("Cameras")]
    public List<Camera> _cameras;

    private void Start()
    {
        ApplyGeneralCameraSettings();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CameraCloseUp();
        }
    }

    private void OnEnable()
    {
        PhysicalAttackBehavior.OnKnockbackFired += CameraCloseUp;
        
        // Listen to live tweaks from the ScriptableObject
        if (_generalCameraSettings != null)
        {
            _generalCameraSettings.OnSettingsChanged += ApplyGeneralCameraSettings;
        }
    }

    private void OnDisable()
    {
        PhysicalAttackBehavior.OnKnockbackFired -= CameraCloseUp;
        
        // Stop listening when disabled/destroyed
        if (_generalCameraSettings != null)
        {
            _generalCameraSettings.OnSettingsChanged -= ApplyGeneralCameraSettings;
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
            UpdateCameraPosition(tile.gameObject.transform);
        }
    }

    private void UpdateCameraPosition(Transform tileTransform)
    {
        foreach (var cam in _cameras)
        {
            Vector3 finalTransform = tileTransform.position + _battleCameraSettings.CameraOffset;
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
}
