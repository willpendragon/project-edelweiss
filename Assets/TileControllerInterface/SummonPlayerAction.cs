using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonPlayerAction : MonoBehaviour, IPlayerAction
{
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;

    public void Select(TileController selectedTile)
    {
        if (selectionLimiter > 0)
        {
            foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(selectedTile))
            {
                tile.GetComponentInChildren<MeshRenderer>().material.color = Color.magenta;
                selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
            }
            savedSelectedTile = selectedTile;
            selectionLimiter--;
        }
    }
    public void Execute()
    {
        GameObject currentActivePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");

        Deity linkedDeity = currentActivePlayerUnit.GetComponent<Unit>().linkedDeity;
        if (linkedDeity != null)
        {
            Debug.Log("Start of Summon Deity on Battlefield");
            foreach (var deitySpawningZoneTile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
            {
                deitySpawningZoneTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
            }
            currentActivePlayerUnit.GetComponent<Unit>().SpendManaPoints(50);
            //Beware, Magic Number
            Debug.Log("Summon Deity on Battlefield");
            var summonPosition = savedSelectedTile.transform.position + new Vector3(0, 3, 0);
            GameObject deityInstance = Instantiate(linkedDeity.gameObject, summonPosition, Quaternion.identity);
            deityInstance.transform.localScale = new Vector3(2, 2, 2);
            //deityPowerLoadingBarSliderIsActive = true;
        }
        else
        {
            Debug.Log("Unable to Summon Deity on Battlefield");
        }
    }
    public void Deselect()
    {
        foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
        {
            tile.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        }
        savedSelectedTile = null;
        selectionLimiter++;
    }
}
