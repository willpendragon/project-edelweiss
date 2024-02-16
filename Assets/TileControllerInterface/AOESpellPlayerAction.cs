using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GridTargetingController;
using static TileController;
using UnityEngine.UI;


public class AOESpellPlayerAction : IPlayerAction
{
    public TileController savedSelectedTile;
    public void Select(TileController selectedTile)
    {
        savedSelectedTile = selectedTile;
        foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
        {
            tile.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
            selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;

        }
        Debug.Log("Selecting AOE Range");
    }
    public void Execute()
    {
        foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
        {
            Debug.Log("Using AOE Spell on Multiple Targets");
            tile.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            if (tile.detectedUnit == null)
            {
                Debug.Log("No Unit found. Can't apply damage");
            }
            else if (tile.detectedUnit.tag == "Enemy")
            {
                tile.detectedUnit.GetComponent<Unit>().HealthPoints -= GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>().currentSelectedSpell.damage;
                Debug.Log("Applied damage on Enemy Units affected by the AOE Spell");
            }
        }
    }
    public void Deselect()
    {
        foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
        {
            tile.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
            savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;

        }
        savedSelectedTile = null;
        Debug.Log("Selecting AOE Range");
    }
}
