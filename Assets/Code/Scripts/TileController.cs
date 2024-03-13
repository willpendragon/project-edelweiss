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
    private void OnEnable()
    {
        //SwitchGridToMoveSelectionMode.OnMoveButtonPressed += SwitchTilesToMoveSelectionMode;
    }
    private void OnDisable()
    {
        //SwitchGridToMoveSelectionMode.OnMoveButtonPressed -= SwitchTilesToMoveSelectionMode;
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
        if (detectedUnit.tag == "Enemy")
        {
            //Spawns an information panel with Active Character Unit details on the Lower Left of the Screen

            GameObject newCurrentlySelectedUnitPanel = Instantiate(Resources.Load("CurrentlySelectedUnit") as GameObject, GameObject.FindGameObjectWithTag("BattleInterfaceCanvas").transform);
            newCurrentlySelectedUnitPanel.tag = "EnemyUnitProfile";
            newCurrentlySelectedUnitPanel.GetComponent<PlayerProfileController>().currentProfileOwner = PlayerProfileController.ProfileOwner.enemyUnit;
            newCurrentlySelectedUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.UpperLeft;
            detectedUnit.GetComponent<Unit>().unitProfilePanel = newCurrentlySelectedUnitPanel;
            Debug.Log("Clicked on Enemy Unit");
        }
        else if (currentSingleTileStatus == SingleTileStatus.basic && detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus != UnitSelectionController.UnitSelectionStatus.unitWaiting)
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
        //else if (currentSingleTileStatus == SingleTileStatus.selectionMode)
        //{
        //    currentPlayerAction.Select(this);
        //    Debug.Log("Selecting Characters on Tiles");
        //}

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
        if (detectedUnit.tag == "Enemy")
        {
            Destroy(GameObject.FindGameObjectWithTag("EnemyUnitProfile"));
        }
        if (detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus == UnitSelectionController.UnitSelectionStatus.unitTemporarilySelected)
        {
            detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitDeselected;
            Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.currentSingleTileStatus = SingleTileStatus.basic;
            }
            Debug.Log("Deselecting Unit");
        }
        else
        {
            currentPlayerAction.Deselect();
        }
    }

    /*public void CreateActivePlayerUnitProfile()
    {
        //Spawns an information panel with Active Character Unit details on the Lower Left of the Screen
        if (detectedUnit.GetComponent<Unit>().unitProfilePanel == null)
        {
            GameObject newCurrentlySelectedUnitPanel = Instantiate(currentlySelectedUnitPanel, GameObject.FindGameObjectWithTag("BattleInterfaceCanvas").transform);
            newCurrentlySelectedUnitPanel.tag = "ActiveCharacterUnitProfile";
            newCurrentlySelectedUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
            detectedUnit.GetComponent<Unit>().unitProfilePanel = newCurrentlySelectedUnitPanel;
            //The newly spawned Unit Profile Panel becomes the Detected Unit Profile Panel
            OnClickedTileWithUnit(detectedUnit);
            //The UI Panel shows the detected Unit details
            Debug.Log("Clicked on a Tile with Player Unit on it");
        }
        //Unit becomes the Active Player Unit in the GridManager
        GridManager.Instance.currentPlayerUnit = detectedUnit;
        //The Unit tag becomes ActivePlayerUnit
        detectedUnit.tag = "ActivePlayerUnit";
        currentSingleTileStatus = SingleTileStatus.selectedPlayerUnitOccupiedTile;
        //Gameplay and Spells Buttons are generated
        detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitSelected;
        detectedUnit.GetComponent<UnitSelectionController>().GenerateGameplayButtons();
        detectedUnit.GetComponent<MoveUIController>().AddMoveButton();
        detectedUnit.GetComponent<MeleeUIController>().AddMeleeButton();
        detectedUnit.GetComponent<SpellUIController>().PopulateCharacterSpellsMenu(detectedUnit);
        detectedUnit.GetComponent<TrapTileUIController>().AddTrapButton();
        detectedUnit.GetComponent<SummoningUIController>().AddSummonButton();
        detectedUnit.GetComponent<CapsuleCrystalUIController>().AddPlaceCaptureCrystalButton();
    }
}
/*
            private void HandleTileDeselection()
            {
                //This is the logic that happens when the Player clicks the Right Mouse Button and is usually employed to deselect Units, whether Units or Enemies.
                Debug.Log("Pressed Right Click on Tile");
                if (detectedUnit == null)
                {
                    Debug.Log("No Unit Found");
                    if (currentSingleTileStatus == SingleTileStatus.aoeAttackSelectionModeWaitingForConfirmation)
                    {
                        DeselectAOESpellRange();
                        Debug.Log("No Unit Found. Deselecting AOE Spell range");
                    }
                }
                else if (currentSingleTileStatus == SingleTileStatus.selectedPlayerUnitOccupiedTile)
                //What happens if the Player clicks with the Right button when the Tile is marked as occupied by a SELECTED (Active) Player Unit
                {
                    if (detectedUnit.tag == "ActivePlayerUnit" && detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus != UnitSelectionController.UnitSelectionStatus.unitAttacking)
                    {
                        Debug.Log("Active Player Unit Selection. Resets the Active Player Unit Selection");
                        GridManager.Instance.currentPlayerUnit = null;
                        detectedUnit.tag = "Player";
                        //Sends a message that resets the Unit Profile UI 23012024 Edited out
                        //OnDeselectedTileWithUnit();
                        Destroy(GameObject.FindGameObjectWithTag("ActiveCharacterUnitProfile"));
                        currentSingleTileStatus = SingleTileStatus.characterSelectionModeActive;
                        detectedUnit.GetComponent<UnitSelectionController>().ResetUnitSelection();
                    }
                    if (detectedUnit.tag == "Player")
                    {
                        Debug.Log("Player Unit Selection. Resets the Player Unit Selection");
                        //OnDeselectedTileWithUnit();
                        Destroy(GameObject.FindGameObjectWithTag("ActiveCharacterUnitProfile"));
                        currentSingleTileStatus = SingleTileStatus.characterSelectionModeActive;
                        detectedUnit.GetComponent<UnitSelectionController>().ResetUnitSelection();
                    }
                    else if (detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus == UnitSelectionController.UnitSelectionStatus.unitWaiting)
                    {
                        OnDeselectedTileWithUnit();
                        Debug.Log("This Unit is already in Waiting Mode. Resetting Waiting Unit character profile panel");
                    }
                }

                else if (detectedUnit.tag == "Enemy")
                //What happens if the Player clicks the Right Mouse Button on a Tile occupied by an Enemy 
                {
                    //During Single Target Spell Selection Final Confirmation Mode
                    if (currentSingleTileStatus == SingleTileStatus.attackSelectionModeWaitingForConfirmation)
                    {
                        detectedUnit.GetComponent<Unit>().ownedTile.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
                        currentSingleTileStatus = SingleTileStatus.characterSelectionModeActive;
                        //The Enemy Target Icon disappears
                        GameObject[] enemyTargetIcons = GameObject.FindGameObjectsWithTag("EnemyTargetIcon");
                        foreach (GameObject enemyTargetIcon in enemyTargetIcons)
                        {
                            Destroy(enemyTargetIcon);
                        }
                        Destroy(GameObject.FindGameObjectWithTag("TargetedEnemyUnitProfile"));
                        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
                        foreach (var tile in tiles)
                        {
                            //Switches all Tiles to Character Selection Mode except the one occupied by the Active Player Unit (targeting the Enemy)
                            tile.GetComponent<TileController>().currentSingleTileStatus = SingleTileStatus.characterSelectionModeActive;

                        }
                        GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().ownedTile.currentSingleTileStatus = SingleTileStatus.selectedPlayerUnitOccupiedTile;
                        GridManager.Instance.currentPlayerUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitSelected;
                    }
                    else if (currentSingleTileStatus == SingleTileStatus.attackSelectionModeActive)
                    {
                        //The Targeted Mesh changes Color to White
                        detectedUnit.GetComponent<Unit>().ownedTile.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
                        //All Tiles switch back to Character Selection Mode
                        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
                        foreach (var tile in tiles)
                        {
                            //Switches all Tiles to Character Selection Mode except the one occupied by the Active Player Unit (targeting the Enemy)
                            tile.GetComponent<TileController>().currentSingleTileStatus = SingleTileStatus.characterSelectionModeActive;

                        }
                        GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().ownedTile.currentSingleTileStatus = SingleTileStatus.selectedPlayerUnitOccupiedTile;
                        GridManager.Instance.currentPlayerUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitSelected;
                        Destroy(GameObject.FindGameObjectWithTag("TargetedEnemyUnitProfile"));
                    }
                    else if (currentSingleTileStatus == SingleTileStatus.aoeAttackSelectionModeWaitingForConfirmation)
                    {
                        DeselectAOESpellRange();
                        Debug.Log("Deselecting AOE Spell range");
                    }
                    else
                    {
                        if (currentSingleTileStatus != SingleTileStatus.aoeAttackSelectionModeWaitingForConfirmation)
                        {
                            Destroy(GameObject.FindGameObjectWithTag("EnemyUnitProfile"));
                            //OnDeselectedTileWithUnit();
                            Debug.Log("Found Enemy Unit. Resetting Enemy character profile panel");
                        }
                    }
                }
                else if (detectedUnit.tag == "Player")
                {
                    if (currentSingleTileStatus == SingleTileStatus.aoeAttackSelectionModeWaitingForConfirmation)
                    {
                        DeselectAOESpellRange();
                        Debug.Log("Found Player on AOE range. Deselecting AOE Spell range");
                    }
                }
                else if (detectedUnit.tag == "ActivePlayerUnit")
                {
                    if (currentSingleTileStatus == SingleTileStatus.aoeAttackSelectionModeWaitingForConfirmation)
                    {
                        DeselectAOESpellRange();
                        Debug.Log("Found Active Player Unit on AOE range. Deselecting AOE Spell range");
                    }
                }
            }
            public void DeselectAOESpellRange()
            {
                foreach (var tile in GridManager.Instance.gridTileControllers)
                {
                    tile.currentSingleTileStatus = SingleTileStatus.characterSelectionModeActive;
                    tile.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
                }
            }

                */
}