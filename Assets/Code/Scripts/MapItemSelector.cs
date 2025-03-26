using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MapItemSelector : MonoBehaviour, IPointerClickHandler
{
    public enum SelectionStatus
    {
        Selected,
        Deselected
    }

    [SerializeField] string mapItemName;

    public Camera mainCamera;
    public float selectDistance = 10f; // Maximum distance within which the item can be selected/deselected
    public float proximityDistance = 5f; // Distance within which the item will be automatically deselected
    public GameObject activateEnterMenuPanelGO;
    public Canvas overworldMapCanvas;
    public SceneLoader selectedNodeSceneLoader;
    public TextMeshProUGUI footerMarqueeText;

    [SerializeField] string footerMarqueeTutorialText = $"Drag the Mouse to explore the Map. Click on Buildings to access a location";

    private SelectionStatus currentStatus = SelectionStatus.Deselected;

    void Update()
    {
        // Calculate the distance between the camera and this item
        float distanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);

        // Automatically deselect if camera is too far
        if (currentStatus == SelectionStatus.Selected && distanceToCamera > selectDistance)
        {
            DeselectItem();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Calculate the distance between the camera and this item
        float distanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);

        // Only allow selection if the camera is within the specified range
        if (distanceToCamera <= selectDistance)
        {
            if (eventData.button == PointerEventData.InputButton.Left) // Left click
            {
                if (currentStatus != SelectionStatus.Selected)
                {
                    SelectItem();
                }
            }
            if (eventData.button == PointerEventData.InputButton.Left && currentStatus == SelectionStatus.Selected)
            {
                ActivateEnterMenu();
            }
            else if (eventData.button == PointerEventData.InputButton.Right) // Right click
            {
                if (currentStatus != SelectionStatus.Deselected)
                {
                    DeselectItem();
                }
            }
        }
    }

    private void ActivateEnterMenu()
    {
        activateEnterMenuPanelGO.SetActive(true);
        overworldMapCanvas.GetComponent<GraphicRaycaster>().enabled = true;
    }

    private void SelectItem()
    {
        currentStatus = SelectionStatus.Selected;
        footerMarqueeText.text = $"{mapItemName} selected. Click on the YES button to access the Building.";
        // Implement logic for when the item is selected
        Debug.Log($"{gameObject.name} selected");
    }
    public void DeselectItem()
    {
        currentStatus = SelectionStatus.Deselected;
        footerMarqueeText.text = footerMarqueeTutorialText;
        Debug.Log($"{gameObject.name} deselected");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the selection range in the editor for visualization
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, selectDistance);

        // Draw the automatic deselect range in the editor for visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, proximityDistance);
    }

    public void EnterNode()
    {
        selectedNodeSceneLoader.ChangeScene();
    }
}
