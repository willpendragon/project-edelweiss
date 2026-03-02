using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BattleCameraSettings", menuName = "Level Design/BattleCamSettings", order = 1)]

public class BattleCameraSettings : ScriptableObject
{
    [SerializeField] private float _zoomAmount = 22.9f; // Actually original zoom amount
    [SerializeField] private Vector3 _cameraOffset = new Vector3(0, 0, -7f);
    [SerializeField] private float _cameraResetDelay = 1.5f;

    public float ZoomAmount => _zoomAmount;
    public Vector3 CameraOffset => _cameraOffset;
    public float CameraResetDelay => _cameraResetDelay;
}
