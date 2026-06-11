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
        
        // --- NEW: Intercept Beacon Logic Before Unit Logic ---
        if (enemyObject != null)
        {
            Beacon hitBeacon = enemyObject.GetComponent<Beacon>();
            if (hitBeacon != null)
            {
                hitBeacon.OnHitByUnit(); // Trigger the Beacon!

                // Manually progress the turn
                ResetTileColours();
                activePlayerUnit.unitOpportunityPoints--;
                UpdateActivePlayerUnitProfile(activePlayerUnit);
                targetTile.tileShaderController.SetTileGlowIntensity(1f);
                return;
            }
        }
        // -----------------------------------------------------

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
        int distanceX = Mathf.Abs(pPos.x - targetPos.x);
        int distanceZ = Mathf.Abs(pPos.z - targetPos.z);
        
        int actualDistance = distanceX + distanceZ;
        meleeRange = activePlayerUnit.unitTemplate.physicAttackBehavior.GetAttackRange(); 

        if (actualDistance > meleeRange)
        {
            return false;
        }

        // Raycast over grid to prevent attacking through straight walls/obstacles
        if (actualDistance > 1 && (distanceX == 0 || distanceZ == 0)) 
        {
            Vector2 dir = new Vector2(targetPos.x - pPos.x, targetPos.z - pPos.z);
            float maxSteps = Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            dir /= maxSteps;
            
            for (int i = 1; i < maxSteps; i++)
            {
                int checkX = Mathf.RoundToInt(pPos.x + dir.x * i);
                int checkZ = Mathf.RoundToInt(pPos.z + dir.y * i);
                
                TileController obstacleTile = GridManager.Instance.GetTileControllerInstance(checkX, checkZ);
                
                // If there's a wall or puzzle obstacle sitting at eye level or taller between them, block it!
                if (obstacleTile != null && 
                   (obstacleTile.tileType == TileType.Obstacle || obstacleTile.tileType == TileType.Environment) && 
                   obstacleTile.gridPosition.y >= pPos.y) 
                {
                    return false;
                }
            }
        }
        
        return true;
    }
}
