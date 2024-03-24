using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RPGCharacterAnims.Actions;
using Unity.VisualScripting;
using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;
using System;
using UnityEditor;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static TileController;
//using static GridTargetingController;
using Unity.PlasticSCM.Editor.WebApi;

public enum SingleTileStatus
{
    basic,
    selectionMode,
    waitingForConfirmationMode,
    characterSelectionModeActive,
    selectedPlayerUnitOccupiedTile,

    //selectionModeActive,

    //meleeSelectionModeActive,
    //meleeSelectionModeWaitingForConfirmation,
    //meleeSelectionModeConfirmedTarget,

    //attackSelectionModeActive,
    //attackSelectionModeWaitingForConfirmation,
    //attackSelectionModeConfirmedTarget,

    //aoeAttackSelectionModeActive,
    //aoeAttackSelectionModeWaitingForConfirmation,
    //aoeAttackSelectionModeConfirmedTarget,

    //trapTileSelectionModeActive,
    //trapTileSelectionModeWaitingForConfirmation,
    //trapTileSelectionModeConfirmedTarget,

    //summonAreaSelectionModeActive,
    //summonAreaSelectionModeWaitingForConfirmation,
    //summonAreaSelectionModeConfirmedTarget
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
    //public GameObject currentlySelectedUnitPanel;
    public TileCurseStatus currentTileCurseStatus;

    public IPlayerAction currentPlayerAction;
    public MeleePlayerAction meleeAction;

    // A* Pathfinding properties
    public int gCost;
    public int hCost;
    public int FCost { get { return gCost + hCost; } }
    public TileController parent;

    public delegate void TileClicked(int x, int y);
    public static event TileClicked OnTileClicked;

    public delegate Unit TileClickedMeleeMode(int x, int y);
    public static event TileClickedMeleeMode OnTileClickedMeleeMode;

    public delegate Unit TileClickedAttackMode(int x, int y);
    public static event TileClickedAttackMode OnTileClickedAttackMode;

    public delegate Unit TileClickedAOESpellMode(int x, int y);
    public static event TileClickedAOESpellMode OnTileClickedAOESpellMode;

    public delegate TileController TileClickedTrapTileMode(int x, int y);
    public static event TileClickedTrapTileMode OnTileClickedTrapTileMode;

    public delegate void TileWaitingForConfirmationAOESpellMode(TileController spellEpicenterTarget);
    public static event TileWaitingForConfirmationAOESpellMode OnTileWaitingForConfirmationAOESpellMode;

    public delegate void TileWaitingForConfirmationSummonMode(TileController summonCenterTarget);
    public static event TileWaitingForConfirmationSummonMode OnTileWaitingForConfirmationSummonMode;

    public delegate void TileConfirmedMeleeMode();
    public static event TileConfirmedMeleeMode OnTileConfirmedMeleeMode;

    public delegate void TileConfirmedAttackMode();
    public static event TileConfirmedAttackMode OnTileConfirmedAttackMode;

    public delegate void TileConfirmedAOESpellMode();
    public static event TileConfirmedAOESpellMode OnTileConfirmedAOESpellMode;

    public delegate void TileConfirmedTrapTileMode();
    public static event TileConfirmedTrapTileMode OnTileConfirmedTrapTileMode;

    public delegate void TileConfirmedSummonMode();
    public static event TileConfirmedSummonMode OnTileConfirmedSummonMode;

    //public delegate void ClickedTileWithUnit(GameObject detectedUnit);
    //public static event ClickedTileWithUnit OnClickedTileWithUnit;

    public delegate void DeselectedTileWithUnit();
    public static event DeselectedTileWithUnit OnDeselectedTileWithUnit;

    public delegate void UpdateEnemyTargetUnitProfile(GameObject detectedUnit);
    public static event UpdateEnemyTargetUnitProfile OnUpdateEnemyTargetUnitProfile;

    void Start()
    {
        currentTileCurseStatus = TileCurseStatus.notCursed;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleTileSelection();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            HandleTileDeselection();
        }
    }

    public void HandleTileSelection()
    {
        if (currentSingleTileStatus == SingleTileStatus.basic && detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus != UnitSelectionController.UnitSelectionStatus.unitWaiting
            && detectedUnit.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        {
            SelectUnitPlayerAction selectUnitPlayerActionInstance = new SelectUnitPlayerAction();

            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.currentPlayerAction = selectUnitPlayerActionInstance;
                tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
                Debug.Log("Switching Tiles to Character Selection Mode");
            }
            detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitTemporarilySelected;
            GameObject playerSelectorIconIstance = Instantiate(Resources.Load("PlayerCharacterSelectorIcon") as GameObject, detectedUnit.transform);
            //Beware: Magic Number
            playerSelectorIconIstance.transform.localPosition += new Vector3(0, 2.5f, 0);
        }

        else if (currentSingleTileStatus == SingleTileStatus.selectionMode)
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
        if (detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus == UnitSelectionController.UnitSelectionStatus.unitTemporarilySelected)
        {
            detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitDeselected;
            Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.currentSingleTileStatus = SingleTileStatus.basic;
            }
            Destroy(detectedUnit.GetComponent<Unit>().unitProfilePanel);
            Debug.Log("Deselecting Temporarily Selected Unit");
        }
        else
        {
            currentPlayerAction.Deselect();
            Debug.Log("Deselecting Unit");
        }
    }
}