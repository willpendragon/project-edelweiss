using DG.Tweening;
using ProjectEdelweiss.Utils;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 _originalCameraPosition;
    private float _originalZoomAmount;
    private float _zoomAmount;
    private float _cameraResetDelay;
    private Vector3 _cameraOffset;

    [SerializeField] BattleCameraSettings _battleCameraSettings;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CameraCloseUp();
        }
    }

    public void Start()
    {
        // Set original CameraTransform + Zoom
        _originalCameraPosition = _cameras[0].gameObject.transform.position;
        _originalZoomAmount = _cameras[0].fieldOfView;
    }
    public List<Camera> _cameras;

    private void OnEnable()
    {
        MeleeBehavior.OnKnockbackFired += CameraCloseUp;
    }

    private void OnDisable()
    {
        MeleeBehavior.OnKnockbackFired -= CameraCloseUp;
    }

    public void CameraCloseUp()
    {
        // Retrieve the position of the character
        var activeUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit);
        var tile = activeUnit.GetComponent<Unit>().ownedTile;
        UpdateCameraPosition(tile.gameObject.transform);
    }

    private void UpdateCameraPosition(Transform tileTransform)
    {
        // Zoom the Camera (use DoTween)
        // Reset Camera to original position
        // Shake Camera

        foreach (var cam in _cameras)
        {
            // Set Camera Transform
            Vector3 finalTransform = tileTransform.position + _battleCameraSettings.CameraOffset; // Retrieve values from SO
            cam.transform.position = finalTransform;
            //cam.transform.DOShakePosition(0.5f, 0.5f, 0, 0, false, false);
            // Set Camera Zoom
            cam.fieldOfView = _battleCameraSettings.ZoomAmount; // Retrieve values from SO
        }
        Invoke("ResetCameraPosition", _battleCameraSettings.CameraResetDelay); // Retrieve values from SO
    }
    private void ResetCameraPosition()
    {
        foreach (var cam in _cameras)
        {
            cam.transform.position = _originalCameraPosition;
            cam.fieldOfView = _originalZoomAmount;
        }
    }
}
