using System;
using UnityEngine;
using Edelweiss.Core;

public class TrapPlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    public float trapCreationCost = 5;
    public int trapCreationRange = 1;

    public static event System.Action OnTrapPlaced;

    public void Select(TileController selectedTile) { }

    public void Deselect() { }

    public void Execute(TileController targetTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit == null) return;

        TrapController trapController = targetTile.GetComponentInChildren<TrapController>();
        if (trapController == null) return;

        int distance = GridManager.Instance.gridMovementController.GetDistance(activePlayerUnit.ownedTile, targetTile);
        if (distance > trapCreationRange ||
            targetTile.currentSingleTileCondition != SingleTileCondition.free ||
            trapController.currentTrapActivationStatus == TrapController.TrapActivationStatus.active)
        {
            targetTile.tileShaderController.AnimateFadeHeightError(2.75f, 0.5f, Color.red);
            return;
        }

        if (activePlayerUnit.unitOpportunityPoints <= 0) return;
        if (activePlayerUnit.unitManaPoints < trapCreationCost) return;

        trapController.currentTrapActivationStatus = TrapController.TrapActivationStatus.active;
        Transform tilePosition = targetTile.transform;
        Vector3 offSet = new Vector3(0, 1f, 0);
        Vector3 spawnPosition = targetTile.transform.position + offSet;
        GameObject trapVFX = (GameObject)Resources.Load("TrapTileVFX");
        Instantiate(trapVFX, spawnPosition, Quaternion.identity);

        activePlayerUnit.unitOpportunityPoints--;

        UpdateActivePlayerUnitProfile(activePlayerUnit);
        activePlayerUnit.unitManaPoints -= trapCreationCost;

        targetTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.white;

        OnTrapPlaced?.Invoke();
    }
    private void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        activePlayerUnit.unitProfilePanel.GetComponent<UnitProfileController>().UpdateActivePlayerProfile(activePlayerUnit);

    }
}
