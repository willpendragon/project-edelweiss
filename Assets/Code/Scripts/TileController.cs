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
    void Update()
    {
        SetTileStatus();
    }

    public bool IsTileOccupied()
    {
        GameObject[] playerUnitsToDetect = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] assistantUnitsToDetect = GameObject.FindGameObjectsWithTag("Companion");
        GameObject[] unitsToDetect = playerUnitsToDetect.Concat(assistantUnitsToDetect).ToArray();

        foreach (var unitToDetect in unitsToDetect)
        {
            float distance = Vector3.Distance(transform.position, unitToDetect.transform.position);
            detectedUnit = unitToDetect;
            if (distance <= proximityDistance)
            {
                return true;
            }
        }
        //If no GameObject has been found within proximity, the Tiles is not occupied
        return false;
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