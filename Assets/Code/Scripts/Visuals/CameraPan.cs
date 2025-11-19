using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraPan : MonoBehaviour
{
    public float panSpeed = 20f;
    public Vector2 panLimitX; // Set the X-axis boundaries
    public Vector2 panLimitY; // Set the Y-axis boundaries
    public Vector2 panLimitZ; // Set the Z-axis boundaries
    public float margin = 10f; // Margin in pixels from the edge of the screen
    public Camera currentCamera;

    [SerializeField] private GameObject _enterCafePanelObject;
    [SerializeField] private GameObject _enterAltarPanelObject;
    [SerializeField] private MapItemSelector _cafeSelector;
    [SerializeField] private MapItemSelector _altarSelector;
    [SerializeField] OverworldMapGenerator overworldMapGenerator;
    [SerializeField] private Image _leftArrow;
    [SerializeField] private Image _rightArrow;
    public bool panIsActive;

    void Update()
    {
        if (panIsActive == false)
            return;
        Vector3 pos = transform.position;

        if (Input.mousePosition.x >= Screen.width - margin)
        {
            pos.x += panSpeed * Time.deltaTime;
            CloseBuildingMenu();
            SetLeftArrowTransparency(0.5f);
            SetRightArrowTransparency(1f);
        }
        else if (Input.mousePosition.x <= margin)
        {
            pos.x -= panSpeed * Time.deltaTime;
            CloseBuildingMenu();
            SetLeftArrowTransparency(1f);
            SetRightArrowTransparency(0.5f);
        }
        else
        {
            SetLeftArrowTransparency(0.2f);
            SetRightArrowTransparency(0.2f);
        }

        // Clamp the camera position to the boundaries
        pos.x = Mathf.Clamp(pos.x, panLimitX.x, panLimitX.y);
        pos.z = Mathf.Clamp(pos.z, panLimitZ.x, panLimitZ.y);
        // Adjust for camera's orientation if it's not aligned with the XZ plane
        pos.y = Mathf.Clamp(pos.y, panLimitY.x, panLimitY.y);

        transform.position = pos;
    }
    private void SetLeftArrowTransparency(float transparency)
    {
        Color leftArrowColor = _leftArrow.color;
        leftArrowColor.a = transparency;
        _leftArrow.color = leftArrowColor;
    }

    private void SetRightArrowTransparency(float transparency)
    {
        Color rightArrowColor = _rightArrow.color;
        rightArrowColor.a = transparency;
        _rightArrow.color = rightArrowColor;
    }

    void Start()
    {
        float horizontalNodePosition = overworldMapGenerator.currentMapNodeTransform.position.x;
        Vector3 camPosition = currentCamera.transform.position;
        camPosition.x = horizontalNodePosition;
        currentCamera.transform.position = camPosition;
        panIsActive = true;
    }

    // Quick methods to close any buildings panel when the camera is panned - should be moved to its own class later

    private void CloseBuildingMenu()
    {
        if (_enterAltarPanelObject.activeSelf)
        {
            CloseAltarPanel();
        }
        if (_enterCafePanelObject.activeSelf)
        {
            CloseCafePanel();
        }
    }

    private void CloseCafePanel()
    {
        _enterCafePanelObject.SetActive(false);
        _cafeSelector.DeselectItem();
    }

    private void CloseAltarPanel()
    {
        _enterAltarPanelObject.SetActive(false);
        _altarSelector.DeselectItem();
    }
}