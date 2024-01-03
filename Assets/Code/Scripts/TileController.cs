using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RPGCharacterAnims.Actions;



public enum SingleTileStatus
{
    selectionModeActive,
    attackSelectionModeActive
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
    /*
    
    public void SetTileStatus()
    {
        if (IsTileOccupied())
        {
            currentTileStatus = TileStatus.occupied;
            if (detectedUnit.GetComponent<Player>() == true)
            {
                detectedUnit.GetComponent<Player>().SetUnitCurrentTile(this.gameObject.GetComponent<TileController>());
                //Debug.Log(this.gameObject + " occupied by a friendly Unit");
            }
        }
        else
        {
            currentTileStatus = TileStatus.free;
            //Debug.Log(this.gameObject + " is NOT occupied by a friendly Unit");

        }
    }
    public bool IsTileOccupied()
    {
        GameObject[] playerUnitsToDetect = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] assistantUnitsToDetect = GameObject.FindGameObjectsWithTag("Companion");
        GameObject[] unitsToDetect = playerUnitsToDetect.Concat(assistantUnitsToDetect).ToArray();

        foreach (var unitToDetect in unitsToDetect)
        {
            float distance = Vector3.Distance(this.transform.position, unitToDetect.transform.position);
            if (distance <= proximityDistance)
            {
                detectedUnit = unitToDetect;
                return true;
            }
        }
        //If no GameObject has been found within proximity, the Tiles is not occupied
        return false;
    }
    */
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
        Debug.Log("Clicked on Tile at Grid Coordinates: " + tileXCoordinate + ", " + tileYCoordinate);
        OnTileClicked?.Invoke(tileXCoordinate, tileYCoordinate);
        if (currentSingleTileStatus == SingleTileStatus.attackSelectionModeActive)
        {
            Unit targetUnit = OnTileClickedAttackMode?.Invoke(tileXCoordinate, tileYCoordinate);
            Debug.Log("Found Unit" + targetUnit);
        }
    }
}