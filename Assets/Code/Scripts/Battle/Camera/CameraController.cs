using ProjectEdelweiss.Utils;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private List<Camera> _cameras;
    [SerializeField] private float _zoomAmount = 22.9f; // Actually original zoom amount
    [SerializeField] private Vector3 _originalCameraPosition;
    [SerializeField] private float _originalZoomAmount;
    [SerializeField] private Vector3 _cameraOffset = new Vector3(0, 0, -7f);
    [SerializeField] private float _cameraResetDelay = 1.5f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CameraCloseUp();
        }
    }

    private void OnEnable()
    {
        MeleeBehavior.OnKnockbackFired += CameraCloseUp;
    }

    private void OnDisable()
    {
        MeleeBehavior.OnKnockbackFired -= CameraCloseUp;
    }

    private void Start()
    {
        // Save original CameraTransform + Zoom
        _originalCameraPosition = _cameras[0].gameObject.transform.position;
        _originalZoomAmount = _cameras[0].fieldOfView;
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
            Vector3 finalTransform = tileTransform.position + _cameraOffset;
            cam.transform.position = finalTransform;
            // Set Camera Zoom
            cam.fieldOfView = _zoomAmount;
        }
        Invoke("ResetCameraPosition", _cameraResetDelay);
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
