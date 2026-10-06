using System;
using UnityEngine;
using Edelweiss.Core;

public class TrapPlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    public float trapCreationCost = 5;
    public int trapCreationRange = 1;
    public float trapVerticalOffset = 1.9f;
    public static event System.Action OnTrapPlaced;

    public delegate void NotEnoughMana(string notification);
    public static event NotEnoughMana OnNotEnoughMana;

    public void Select(TileController selectedTile) { }

    public void Deselect() { }

    public void Execute(TileController targetTile)
    {
        if (targetTile.tileCurrentFieldPrize != null)
            return;

        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit == null) return;

        TrapController trapController = targetTile.GetComponentInChildren<TrapController>();
        if (trapController == null) return;

        // GridMovementController.GetDistance is an A* pathfinding heuristic (scaled 10/14 per tile), not a raw tile count - use Manhattan distance to match trapCreationRange's unit.
        Vector3Int attackerPos = activePlayerUnit.ownedTile.gridPosition;
        Vector3Int targetPos = targetTile.gridPosition;
        int distance = Mathf.Abs(attackerPos.x - targetPos.x) + Mathf.Abs(attackerPos.z - targetPos.z);
        if (distance > trapCreationRange ||
            targetTile.currentSingleTileCondition != SingleTileCondition.free ||
            trapController.currentTrapActivationStatus == TrapController.TrapActivationStatus.active)
        {
            return;
        }

        if (activePlayerUnit.unitOpportunityPoints <= 0) return;
        if (activePlayerUnit.unitManaPoints < trapCreationCost)
        {
            OnNotEnoughMana("Not enough Mana...");
            return;
        }

        trapController.currentTrapActivationStatus = TrapController.TrapActivationStatus.active;
        Transform tilePosition = targetTile.transform;
        Vector3 offSet = new Vector3(0, trapVerticalOffset, 0);
        Vector3 spawnPosition = targetTile.transform.position + offSet;
        GameObject trapVFXPrefab = (GameObject)Resources.Load("TrapTileVFX");
        GameObject trapVFXInstance = Instantiate(trapVFXPrefab, spawnPosition, Quaternion.identity);
        trapVFXInstance.transform.localScale = new Vector3(2, 2, 2);

        activePlayerUnit.unitOpportunityPoints--;

        activePlayerUnit.unitManaPoints -= trapCreationCost;
        UpdateActivePlayerUnitProfile(activePlayerUnit);

        targetTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
        OnTrapPlaced?.Invoke();
    }
    private void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateProfile(activePlayerUnit.unitTemplate.unitName);
        BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateRemainingMoves(activePlayerUnit.unitTemplate.unitName);
    }
}
