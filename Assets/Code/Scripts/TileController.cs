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

public enum SingleTileStatus
{
    characterSelectionModeActive,
    selectionModeActive,
    selectedPlayerUnitOccupiedTile,
    attackSelectionModeActive,
    attackSelectionModeWaitingForConfirmation,
    attackSelectionModeConfirmedTarget
}

public enum SingleTileCondition
{
    free,
    occupied
}
public class TileController : MonoBehaviour
{

    public enum TileStatus
    {
        free,
        occupied
    }

    public enum TileAlignment
    {
        neutral,
        red,
        blue
    }

    public TileStatus currentTileStatus;
    public TileAlignment currentTileAlignment;
    public float proximityDistance = 1.5f;
    public GameObject detectedUnit;
    public int tileXCoordinate;
    public int tileYCoordinate;
    public SingleTileStatus currentSingleTileStatus;
    public SingleTileCondition currentSingleTileCondition;
    public GameObject targetIcon;

    // A* Pathfinding properties
    public int gCost;
    public int hCost;
    public int FCost { get { return gCost + hCost; } }
    public TileController parent;

    public delegate void PlayerEscapedFromJudgmentAttack();
    public static event PlayerEscapedFromJudgmentAttack OnPlayerEscapedFromJudgmentAttack;

    public delegate void JudgmentAttackSuccessful();
    public static event JudgmentAttackSuccessful OnJudgmentAttackSuccessful;

    public delegate void TileClicked(int x, int y);
    public static event TileClicked OnTileClicked;

    public delegate Unit TileClickedAttackMode(int x, int y);
    public static event TileClickedAttackMode OnTileClickedAttackMode;

    public delegate void TileConfirmedAttackMode();
    public static event TileConfirmedAttackMode OnTileConfirmedAttackMode;

    [SerializeField] ParticleSystem redParticle;
    [SerializeField] ParticleSystem blueParticle;

    public delegate void ClickedTileWithUnit(GameObject detectedUnit);
    public static event ClickedTileWithUnit OnClickedTileWithUnit;

    public delegate void DeselectedTileWithUnit();
    public static event DeselectedTileWithUnit OnDeselectedTileWithUnit;

    void Start()
    {
        currentTileAlignment = TileAlignment.neutral;
    }
    private void OnEnable()
    {
        Deity.OnDeityJudgment += AttackUnit;
        SwitchGridToMoveSelectionMode.OnMoveButtonPressed += SwitchTileToSelectionMode;
    }
    private void OnDisable()
    {
        Deity.OnDeityJudgment -= AttackUnit;
        SwitchGridToMoveSelectionMode.OnMoveButtonPressed -= SwitchTileToSelectionMode;
    }

    public void AttackUnit()
    {
        if (detectedUnit != null)
        {
            if (detectedUnit.GetComponent<Player>() == true && currentTileAlignment == TileAlignment.blue)
            {
                OnJudgmentAttackSuccessful();
            }
            else if (detectedUnit.GetComponent<Player>() == true && currentTileAlignment == TileAlignment.neutral)
            {
                OnPlayerEscapedFromJudgmentAttack();
                //Use this event to display a "Missed" attack notification after the attack happened.
            }
        }
    }
    public void ActivateRedParticle()
    {
        redParticle.Play();
        currentTileAlignment = TileAlignment.red;
    }
    public void ActivateBlueParticle()
    {
        blueParticle.Play();
        currentTileAlignment = TileAlignment.blue;
    }

    //Should Move this into a different component class
    public void OnMouseDown()
    {
        //The Player left clicks on a Tile while the Tile is undergoing MOVE SELECTION Mode, calls the method to move to that Tile.
        if (Input.GetMouseButton(0) && currentSingleTileStatus == SingleTileStatus.selectionModeActive)
        {
            OnTileClicked?.Invoke(tileXCoordinate, tileYCoordinate);
        }

        //The Player left clicks on a Tile while Character Selection Mode is Active
        if (Input.GetMouseButton(0) && currentSingleTileStatus == SingleTileStatus.characterSelectionModeActive)
        {
            //Tile should have a Detected Unit
            if (detectedUnit != null)
            {
                //If the Detected Unit is not an Player Unit
                if (detectedUnit.gameObject.tag == "Player")
                {
                    if (GridManager.Instance.currentPlayerUnit == null)
                    {
                        Debug.Log("Clicked on a Tile with Player Unit on it");
                        //Unit becomes the Active Player Unit in the GridManager
                        GridManager.Instance.currentPlayerUnit = detectedUnit;
                        //The Unit tag becomes ActivePlayerUnit
                        detectedUnit.tag = "ActivePlayerUnit";
                        //The UI shows the detected Unit details
                        OnClickedTileWithUnit(detectedUnit);
                        currentSingleTileStatus = SingleTileStatus.selectedPlayerUnitOccupiedTile;
                        //Gameplay and Spells Buttons are generated
                        detectedUnit.GetComponent<UnitSelectionController>().GenerateGameplayButtons();
                        detectedUnit.GetComponent<SpellUIController>().PopulateCharacterSpellsMenu(detectedUnit);
                    }
                }
                else if (detectedUnit.gameObject.tag == "Enemy")
                {
                    //Just displays the profile of the Enemy Unit sitting on the Clicked Tile
                    //To implement
                    Debug.Log("Clicked on Enemy Unit");
                }

                else if (detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus == UnitSelectionController.UnitSelectionStatus.unitWaiting)
                {
                    Debug.Log("This Player Unit is currently in Waiting Mode");
                }
            }
        }

        //The Player left clicks on a Tile while ATTACK SELECTION MODE (aka, Targeting during Spells) is Active
        else if (Input.GetMouseButton(0) && currentSingleTileStatus == SingleTileStatus.attackSelectionModeActive)
        {
            //The System retrieves the coordinates from the Grid and sets the corresponding Unit as the Target
            Unit targetUnit = OnTileClickedAttackMode?.Invoke(tileXCoordinate, tileYCoordinate);
            if (targetUnit != null)
            {
                //The System retrieves the Tile Controller Instance from the Target Unit
                TileController targetUnitTileController = GridManager.Instance.GetTileControllerInstance(tileXCoordinate, tileYCoordinate);
                //The Tile's mode of the Target Unit and only that tile (in Single Target) becomes a Target Waiting for Confirmation
                targetUnitTileController.currentSingleTileStatus = SingleTileStatus.attackSelectionModeWaitingForConfirmation;
                //The Target Unit's tile turns to Red.
                targetUnitTileController.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                //A Red selector icon spawns over the Target Unit's head.
                GameObject newTargetIcon = Instantiate(targetIcon, targetUnit.transform.localPosition + (targetUnit.transform.up * 2), Quaternion.identity);
                Debug.Log("Set Unit" + targetUnit.gameObject.name + " as current Target");
            }
        }
        //If the Target is already waiting for confirmation, I can attack that target.
        else if (Input.GetMouseButton(0) && currentSingleTileStatus == SingleTileStatus.attackSelectionModeWaitingForConfirmation)
        {
            OnTileConfirmedAttackMode();
        }
    }
    //This is the logic that happens when the Mouse is Over a Tile and is usually employed to deselect Units, whether Units or Enemies.
    public void OnMouseOver()
    {
        if (Input.GetMouseButton(1) && currentSingleTileStatus == SingleTileStatus.selectedPlayerUnitOccupiedTile)
        //What happens if the Player is standing with the Mouse and clicks with the Right button when the Tile is marked as occupied by a SELECTED (Active) Player Unit
        {
            if (detectedUnit.tag == "ActivePlayerUnit")
            {
                Debug.Log("Active Player Unit Selection. Resets the Active Player Unit Selection");
                //Sends a message that resets the Unit Profile UI
                GridManager.Instance.currentPlayerUnit = null;
                detectedUnit.tag = "Player";
                OnDeselectedTileWithUnit();
                currentSingleTileStatus = SingleTileStatus.characterSelectionModeActive;
                detectedUnit.GetComponent<UnitSelectionController>().ResetUnitSelection();
            }
            else if (detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus == UnitSelectionController.UnitSelectionStatus.unitWaiting)
            {
                OnDeselectedTileWithUnit();
                Debug.Log("This Unit is already in Waiting Mode. Resetting Waiting Unit character profile panel");
            }
        }

        else if (Input.GetMouseButton(1) && detectedUnit.tag == "Enemy")
        //What happens if the Player is standing with the Mouse and clicks with the Right Button on a Tile occupied by an Enemy
        {
            if (currentSingleTileStatus == SingleTileStatus.attackSelectionModeWaitingForConfirmation)
            {
                currentSingleTileStatus = SingleTileStatus.attackSelectionModeActive;
                //The Enemy Target Icon disappears
                GameObject[] enemyTargetIcons = GameObject.FindGameObjectsWithTag("EnemyTargetIcon");
                foreach (GameObject enemyTargetIcon in enemyTargetIcons)
                {
                    Destroy(enemyTargetIcon);
                }
            }
            else if (currentSingleTileStatus == SingleTileStatus.attackSelectionModeActive)
            {
                //The Targeted Mesh changes Color to White
                detectedUnit.GetComponent<Unit>().ownedTile.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
                //All Tiles switch back to Character Selection Mode
                GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
                foreach (var tile in tiles)
                {
                    tile.GetComponent<TileController>().currentSingleTileStatus = SingleTileStatus.characterSelectionModeActive;
                    GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().ownedTile.currentSingleTileStatus = SingleTileStatus.selectedPlayerUnitOccupiedTile;
                }
            }
            else
            {
                OnDeselectedTileWithUnit();
                Debug.Log("Found Enemy Unit. Resetting Enemy character profile panel");
            }
        }

    }
    public void SwitchTileToSelectionMode()
    {
        currentSingleTileStatus = SingleTileStatus.selectionModeActive;
        Debug.Log("Switching tiles to selection Mode");
    }
}