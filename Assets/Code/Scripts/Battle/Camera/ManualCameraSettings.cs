using UnityEngine;

namespace ProjectEdelweiss.Settings
{
    [CreateAssetMenu(fileName = "ManualCameraSettings", menuName = "Camera/Manual Camera Settings")]
    public class ManualCameraSettings : ScriptableObject
    {
        [Header("Pan Settings")]
        [Tooltip("Speed at which the camera pans when using WASD/arrow keys")]
        [SerializeField] private float _panSpeed = 25f;
        
        [Tooltip("Damping/smoothing for camera movement (0 = instant, 1 = smooth). Lower = snappier.")]
        [SerializeField] [Range(0f, 1f)] private float _panDamping = 0.05f;

        [Header("Zoom Settings")]
        [Tooltip("Speed at which the camera zooms in/out with mouse scroll")]
        [SerializeField] private float _zoomSpeed = 0.5f;
        
        [Tooltip("Minimum orthographic size (most zoomed in)")]
        [SerializeField] private float _minZoom = 5f;
        
        [Tooltip("Maximum orthographic size (most zoomed out)")]
        [SerializeField] private float _maxZoom = 20f;
        
        [Tooltip("Smoothing applied to zoom transitions")]
        [SerializeField] private float _zoomSmoothTime = 0.15f;

        [Header("Boundary Settings")]
        [Tooltip("Padding added to the grid boundaries (in world units, horizontal/X axis)")]
        [SerializeField] private float _horizontalBoundaryPadding = 5f;
        
        [Tooltip("Padding added to the grid boundaries (in world units, vertical/Z axis)")]
        [SerializeField] private float _verticalBoundaryPadding = 5f;

        [Header("Reset Settings")]
        [Tooltip("Key to reset camera to default position")]
        [SerializeField] private KeyCode _resetKey = KeyCode.Home;
        
        [Tooltip("Enable camera reset functionality")]
        [SerializeField] private bool _enableReset = true;

        // Public properties
        public float PanSpeed => _panSpeed;
        public float PanDamping => _panDamping;
        public float ZoomSpeed => _zoomSpeed;
        public float MinZoom => _minZoom;
        public float MaxZoom => _maxZoom;
        public float ZoomSmoothTime => _zoomSmoothTime;
        public float HorizontalBoundaryPadding => _horizontalBoundaryPadding;
        public float VerticalBoundaryPadding => _verticalBoundaryPadding;
        public KeyCode ResetKey => _resetKey;
        public bool EnableReset => _enableReset;
    }
}
