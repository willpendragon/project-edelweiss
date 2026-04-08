using UnityEngine;
using Edelweiss.Core;

public class MeleePlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    private int meleeRange;

    public void Execute(TileController targetTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit.unitOpportunityPoints <= 0 ||
            activePlayerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead ||
            activePlayerUnit.unitStatusController.unitCurrentStatus == UnitStatus.Faithless || // Negating Melee if Player is Faithless
            !IsEnemyReachable(activePlayerUnit, targetTile))
            return;

        GameObject enemyObject = targetTile.detectedUnit;
        activePlayerUnit.unitTemplate.physicAttackBehavior.AttackSequence(targetTile.detectedUnit.GetComponent<Unit>(), targetTile, activePlayerUnit);
        ResetTileColours();
        activePlayerUnit.unitOpportunityPoints--;
        UpdateActivePlayerUnitProfile(activePlayerUnit);
        targetTile.tileShaderController.SetTileGlowIntensity(1f);
    }

    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        // Use centralized logic.
        BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateRemainingMoves(activePlayerUnit.unitTemplate.unitName);
    }

    public void ResetTileColours()
    {
        if (savedSelectedTile != null)
        {
            savedSelectedTile.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            savedSelectedTile = null;
        }
    }
    private bool IsEnemyReachable(Unit activePlayerUnit, TileController targetTile)
    {
        // Voxel Distance Calculation (Manhattan Distance)
        Vector3Int pPos = activePlayerUnit.ownedTile.gridPosition;
        Vector3Int targetPos = targetTile.gridPosition;

        // Quanti "blocchi" di mappa tra l'attaccante e il difensore?
        // Z rimpiazza la vecchia Y per la profondità orizzontale.
        int distanceX = Mathf.Abs(pPos.x - targetPos.x);
        int distanceZ = Mathf.Abs(pPos.z - targetPos.z);
        
        // Puoi decidere che l'attacco valga anche per i dislivelli Y, e aggiungere:
        // int distanceY = Mathf.Abs(pPos.y - targetPos.y); 
        // per far sì che colpire un nemico su una torre alta 3 blocchi ti costi "3 range".
        // Per ora calcoliamo in piano come fosse FFTactics standard:
        int actualDistance = distanceX + distanceZ;

        meleeRange = activePlayerUnit.unitTemplate.physicAttackBehavior.GetAttackRange(); 

        if (actualDistance > meleeRange)
        {
            return false;
        }
        return true;
    }
}
