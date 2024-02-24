using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlaceCrystalPlayerAction : MonoBehaviour, IPlayerAction
{
    public int selectionLimiter = 1;
    public GameObject captureCrystal;
    public TileController currentSavedTile;
    public void Select(TileController selectedTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (activePlayerUnit.unitOpportunityPoints > 0 && selectionLimiter > 0)
        {
            selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
            currentSavedTile = selectedTile;
            activePlayerUnit.SpendManaPoints(20);
            //Warning: Magic Numbers
            activePlayerUnit.unitOpportunityPoints--;
            selectionLimiter--;
        }
    }
    public void Execute()
    {
        if (currentSavedTile.currentSingleTileStatus == SingleTileStatus.waitingForConfirmationMode)
        {
            GameObject captureCrystalInstance = Instantiate(Resources.Load("CaptureCrystal") as GameObject, currentSavedTile.transform.position, Quaternion.identity);
            Debug.Log("Attempt to Capture the Deity");
            if (DeityCaptureRoll() > 10)
            //Beware, Magic Number
            {
                Debug.Log("Deity was captured");
                //Initiate the Deity captured sequence
            }
            else
            {
                Debug.Log("Deity was not captured");
            }
            //Insert the logic for Capturing the Deity here.
            //I'll probably need to create a class for handling the Crystal entirely and retrieve data from a Scriptable Object
        }
    }

    public void Deselect()
    {

    }

    public int DeityCaptureRoll()
    {
        int deityCaptureRoll = Random.Range(0, 10);
        //Beware, Magic Number
        GameObject[] captureCrystalsOnBattlefield = GameObject.FindGameObjectsWithTag("CaptureCrystal");
        deityCaptureRoll = deityCaptureRoll * captureCrystalsOnBattlefield.Length;
        return deityCaptureRoll;
    }
}
