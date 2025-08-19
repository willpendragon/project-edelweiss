using System;
using UnityEngine;

public class TrapPlayerAction : MonoBehaviour, IPlayerAction
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
        Instantiate((GameObject)Resources.Load("TrapTileVFX"), targetTile.transform);

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
