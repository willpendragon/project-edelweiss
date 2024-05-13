using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using JetBrains.Annotations;
using System;
using UnityEditor;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static TileController;

public enum SingleTileStatus
{
    basic,
    selectionMode,
    waitingForConfirmationMode,
    characterSelectionModeActive,
    selectedPlayerUnitOccupiedTile,

}

public enum SingleTileCondition
{
    free,
    occupied,
    occupiedByDeity
}

public enum TileCurseStatus
{
    notCursed,
    cursed
}
public class TileController : MonoBehaviour, IPointerClickHandler
{
    public GameObject detectedUnit;
    public int tileXCoordinate;
    public int tileYCoordinate;
    public SingleTileStatus currentSingleTileStatus;
    public SingleTileCondition currentSingleTileCondition;
    public GameObject targetIcon;
    public TileCurseStatus currentTileCurseStatus;

    public IPlayerAction currentPlayerAction = new SelectUnitPlayerAction();
    public MeleePlayerAction meleeAction;

    // A* Pathfinding properties
    public int gCost;
    public int hCost;
    public int FCost { get { return gCost + hCost; } }
    public TileController parent;

    public float clickCooldown = 0.5f; // Cooldown in seconds between clicks
    private float lastClickTime;

    public delegate void UpdateEnemyTargetUnitProfile(GameObject detectedUnit);
    public static event UpdateEnemyTargetUnitProfile OnUpdateEnemyTargetUnitProfile;

    void Start()
    {
        currentTileCurseStatus = TileCurseStatus.notCursed;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && Time.time - lastClickTime > clickCooldown)
        {
            lastClickTime = Time.time;
            HandleTileSelection();
        }
        else if (eventData.button == PointerEventData.InputButton.Right && Time.time - lastClickTime > clickCooldown)
        {
            lastClickTime = Time.time;
            HandleTileDeselection();
        }
    }

    public void HandleTileSelection()
    {
        if (currentSingleTileStatus == SingleTileStatus.selectionMode)
        {
            Debug.Log("Selecting Tiles");
            currentPlayerAction.Select(this);
        }
        else if (currentSingleTileStatus == SingleTileStatus.waitingForConfirmationMode)
        {
            //If Waiting for Confirmation is True
            currentPlayerAction.Execute();
        }
    }
    public void HandleTileDeselection()
    {
        currentPlayerAction.Deselect();
    }
}