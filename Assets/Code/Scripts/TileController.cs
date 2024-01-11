using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RPGCharacterAnims.Actions;
using Unity.VisualScripting;

public enum SingleTileStatus
{
    characterSelectionModeActive,
    selectionModeActive,
    attackSelectionModeActive,
    attackSelectionModeWaitingForConfirmation
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

    void Start()
    {
        currentTileAlignment = TileAlignment.neutral;
    }
    private void OnEnable()
    {
        Deity.OnDeityJudgment += AttackUnit;
    }
    private void OnDisable()
    {
        Deity.OnDeityJudgment -= AttackUnit;
    }
    void Update()
    {
        //SetTileStatus();
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

    public void OnMouseDown()
    {
        if (currentSingleTileStatus == SingleTileStatus.characterSelectionModeActive)
        {
            Debug.Log("Unable to Select Tile");
        }
        //Debug.Log("Clicked on Tile at Grid Coordinates: " + tileXCoordinate + ", " + tileYCoordinate);
        if (currentSingleTileStatus == SingleTileStatus.selectionModeActive)
        {
            OnTileClicked?.Invoke(tileXCoordinate, tileYCoordinate);
        }
        else if (currentSingleTileStatus == SingleTileStatus.attackSelectionModeActive)
        {
            Unit targetUnit = OnTileClickedAttackMode?.Invoke(tileXCoordinate, tileYCoordinate);
            if (targetUnit != null)
            {
                TileController targetUnitTileController = GridManager.Instance.GetTileControllerInstance(tileXCoordinate, tileYCoordinate);
                targetUnitTileController.currentSingleTileStatus = SingleTileStatus.attackSelectionModeWaitingForConfirmation;
                targetUnitTileController.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                GameObject newTargetIcon = Instantiate(targetIcon, targetUnit.transform.localPosition + (targetUnit.transform.up * 2), Quaternion.identity);
                Debug.Log("Set Unit" + targetUnit.gameObject.name + " as current Target");
            }
        }
        else if (currentSingleTileStatus == SingleTileStatus.attackSelectionModeWaitingForConfirmation)
        {
            OnTileConfirmedAttackMode();
        }
    }
}