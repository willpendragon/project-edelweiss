using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    public float normalSpeed = 10.0f;
    public float sprintSpeed = 30.0f;
    public float movementSmoothness = 0.2f; // Higher = more "slide"

    [Header("Rotation Settings")]
    public float mouseSensitivity = 2.0f;
    public float rotationSmoothness = 0.1f; // Higher = more cinematic drag

    private float targetYaw;
    private float targetPitch;
    private Vector3 currentRotation;
    private Vector3 rotationSmoothVelocity;

    private Vector3 targetPosition;
    private Vector3 currentPosition;
    private Vector3 positionSmoothVelocity;

    void Start()
    {
        // Lock and hide the cursor for smooth mouse tracking
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize positions and rotations to prevent initial snapping
        currentPosition = targetPosition = transform.position;
        Vector3 angles = transform.eulerAngles;
        targetYaw = angles.y;
        targetPitch = angles.x;
        currentRotation = new Vector3(targetPitch, targetYaw, 0f);
    }

    void Update()
    {
        // Press Escape to unlock the cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Click on the game screen to lock the cursor again
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                return; // Stop processing camera input if cursor is unlocked
            }
        }

        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        // Get raw mouse input
        targetYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        targetPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Prevent flipping the camera upside down
        targetPitch = Mathf.Clamp(targetPitch, -90f, 90f);

        // Smoothly interpolate the rotation
        Vector3 targetRot = new Vector3(targetPitch, targetYaw, 0f);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRot, ref rotationSmoothVelocity, rotationSmoothness);
        transform.eulerAngles = currentRotation;
    }

    private void HandleMovement()
    {
        // Determine speed based on Shift key
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : normalSpeed;

        // Get raw keyboard input (WASD / Arrows)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate direction relative to camera rotation
        Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized;

        // Handle vertical movement (E to go up, Q to go down)
        if (Input.GetKey(KeyCode.E)) moveDirection += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) moveDirection += Vector3.down;

        // Move the target position
        targetPosition += moveDirection * currentSpeed * Time.deltaTime;

        // Smoothly interpolate to the target position
        currentPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref positionSmoothVelocity, movementSmoothness);
        transform.position = currentPosition;
    }
}
