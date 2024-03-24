using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SummonPlayerAction : MonoBehaviour, IPlayerAction
{
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    public int deityLimiter = 1;

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
        Unit currentActivePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        Deity linkedDeity = currentActivePlayerUnit.linkedDeity;

        if (linkedDeity != null && currentActivePlayerUnit.unitOpportunityPoints > 0 && deityLimiter > 0)
        {
            Debug.Log("Start of Summon Deity on Battlefield");
            foreach (var deitySpawningZoneTile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
            {
                deitySpawningZoneTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
            }
            currentActivePlayerUnit.SpendManaPoints(50);
            //Beware, Magic Number
            Debug.Log("Summon Deity on Battlefield");

            var summonPosition = savedSelectedTile.transform.position + new Vector3(0, 3, 0);
            GameObject deityInstance = Instantiate(linkedDeity.gameObject, summonPosition, Quaternion.identity);
            currentActivePlayerUnit.linkedDeity = deityInstance.GetComponent<Deity>();
            deityInstance.transform.localScale = new Vector3(2, 2, 2);
            //deityPowerLoadingBarSliderIsActive = true;

            currentActivePlayerUnit.GetComponent<SummoningUIController>().SwitchButtonToPrayMode();
            //22022024 This switches the Summon Button to Pray Mode only for the Active Player. Consider also spawning a Pray Button on the other Player Units.
            currentActivePlayerUnit.unitOpportunityPoints--;
            deityLimiter--;
            Debug.Log("Summoning Deity");
        }
        else
        {
            Debug.Log("Unable to Summon Deity on Battlefield");
        }
    }
    public void Deselect()
    {
        selectionLimiter++;
        if (savedSelectedTile != null)
        {
            foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
            {
                tile.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            }
            Debug.Log("Deselecting Summon Spawn Area");
            deityLimiter++;
        }
    }
}
