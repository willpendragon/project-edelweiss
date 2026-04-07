using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GeneralCameraSettings", menuName = "Camera/General Camera Settings")]
public class GeneralCameraSettings : ScriptableObject
{
    public Vector3 CameraPosition;
    public Vector3 CameraRotation;
    public float ZoomAmount = 60f;

    // Event triggered when a value is changed in the Inspector
    public event Action OnSettingsChanged;

    private void OnValidate()
    {
        // Notify listeners that a setting has been tweaked
        OnSettingsChanged?.Invoke();
    }
}
