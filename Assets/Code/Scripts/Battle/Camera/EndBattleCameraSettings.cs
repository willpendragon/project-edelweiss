using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EndBattleCameraSettings", menuName = "Level Design/EndBattleCamSettings", order = 1)]
public class EndBattleCameraSettings : ScriptableObject
{
    public Vector3 CameraPosition;
    public Vector3 CameraRotation;
    // Interpreted as orthographic size for ortho cameras, or field-of-view degrees for perspective cameras.
    public float ZoomAmount = 60f;

    // Event triggered when a value is changed in the Inspector
    public event Action OnSettingsChanged;

    private void OnValidate()
    {
        // Notify listeners that a setting has been tweaked
        OnSettingsChanged?.Invoke();
    }
}