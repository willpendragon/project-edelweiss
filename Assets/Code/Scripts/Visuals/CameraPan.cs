using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour
{
    public float panSpeed = 20f;
    public Vector2 panLimitX; // Set the X-axis boundaries
    public Vector2 panLimitY; // Set the Y-axis boundaries
    public Vector2 panLimitZ; // Set the Z-axis boundaries
    public float margin = 10f; // Margin in pixels from the edge of the screen
    public Camera currentCamera;

    [SerializeField] OverworldMapGenerator overworldMapGenerator;

    // Update is called once per frame

    void Update()
    {
        Vector3 pos = transform.position;

        if (Input.mousePosition.x >= Screen.width - margin)
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (Input.mousePosition.x <= margin)
        {
            pos.x -= panSpeed * Time.deltaTime;
        }
        //if (Input.mousePosition.y >= Screen.height - margin)
        //{
        //    pos.z += panSpeed * Time.deltaTime;
        //}
        //if (Input.mousePosition.y <= margin)
        //{
        //    pos.z -= panSpeed * Time.deltaTime;
        //}

        // Clamp the camera position to the boundaries
        pos.x = Mathf.Clamp(pos.x, panLimitX.x, panLimitX.y);
        pos.z = Mathf.Clamp(pos.z, panLimitZ.x, panLimitZ.y);
        //// Adjust for your camera's orientation if it's not aligned with the XZ plane
        pos.y = Mathf.Clamp(pos.y, panLimitY.x, panLimitY.y);

        transform.position = pos;

    }
    void Start()
    {
        float horizontalNodePosition = overworldMapGenerator.currentMapNodeTransform.position.x;
        Vector3 camPosition = currentCamera.transform.position;
        camPosition.x = horizontalNodePosition;
        currentCamera.transform.position = camPosition;
    }
}