using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        SetTileStatus();
    }

    public void AttackUnit()
    {
        if (detectedUnit.GetComponent<Player>() == true && currentTileAlignment == TileAlignment.blue)
        {
            Debug.Log("Detected Player on cursed Tile during Judgment move. Player loses the battle.");
            //Destroy(detectedUnit, 3);
        }
        else if (detectedUnit.GetComponent<Player>() == true && currentTileAlignment == TileAlignment.neutral)
        {
            Debug.Log("The Player escaped. Player wins the battle");
        }
    }

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
}