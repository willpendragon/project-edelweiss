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
            selectedTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.magenta;
            selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
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
            savedSelectedTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
            savedSelectedTile.gameObject.GetComponentInChildren<TileShaderController>().AnimateFadeHeight(1f, 0.2f, Color.magenta);
            int summoningCost = 10;
            currentActivePlayerUnit.SpendManaPoints(summoningCost);
            Debug.Log("Summon Deity on Battlefield");

            var summonPosition = savedSelectedTile.transform.position + new Vector3(0, 3, 0);
            GameObject deityInstance = Instantiate(linkedDeity.gameObject, summonPosition, Quaternion.identity);
            currentActivePlayerUnit.summonedLinkedDeity = deityInstance.GetComponent<Deity>();
            deityInstance.transform.localScale = new Vector3(2, 2, 2);

            currentActivePlayerUnit.GetComponent<SummoningUIController>().SwitchButtonToPrayMode();
            currentActivePlayerUnit.unitOpportunityPoints--;
            deityLimiter--;
            Debug.Log("Summoning Deity");
        }
        else if (currentActivePlayerUnit.summonedLinkedDeity != null)
        {
            Debug.Log("This Unit already has summoned a Deity on the Battlefield");
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
            int summoningRange = 2;
            foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile, summoningRange))
            {
                //tile.GetComponentInChildren<SpriteRenderer>().material.color = Color.green;
                tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            }
            Debug.Log("Deselecting Summon Spawn Area");
            deityLimiter++;
        }

        foreach (var deitySpawningZoneTile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile, 2))
        {
            deitySpawningZoneTile.gameObject.GetComponentInChildren<TileShaderController>().AnimateFadeHeight(0f, 0.2f, Color.white);
        }
    }
}
