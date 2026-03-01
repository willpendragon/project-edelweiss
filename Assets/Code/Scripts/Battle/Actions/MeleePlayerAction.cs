using Unity.VisualScripting;
using UnityEngine;
using static TileController;
using Edelweiss.Core;

public class MeleePlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    //public int selectionLimiter = 1;
    private int meleeRange = 2; // Move to attack SO

    //public Vector2Int knockbackDirection;
    //public int knockbackStrength = 2;

    public delegate void UsedMeleeAction(string notification);
    public static event UsedMeleeAction OnUsedMeleeAction;

    public delegate void UsedMagnet();
    public static event UsedMagnet OnUsedMagnet;

    public void Execute(TileController targetTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit.unitOpportunityPoints <= 0 ||
            activePlayerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead ||
            activePlayerUnit.unitStatusController.unitCurrentStatus == UnitStatus.Faithless || // Negating Melee if Player is Faithless
            !IsEnemyReachable(activePlayerUnit, targetTile))
            return;

        GameObject enemyObject = targetTile.detectedUnit;
        activePlayerUnit.unitTemplate.meleeBehavior.AttackSequence(targetTile.detectedUnit.GetComponent<Unit>(), targetTile, activePlayerUnit);
        ResetTileColours();
        // Insert Interaction Logic here (for mirrors) - should have a dedicated behaviur

        activePlayerUnit.unitOpportunityPoints--;
        UpdateActivePlayerUnitProfile(activePlayerUnit);

        OnUsedMeleeAction($"{activePlayerUnit.unitTemplate.unitName} used Melee Attack");
        //activePlayerUnit.GetComponent<BattleFeedbackController>().PlayMeleeAttackAnimation(activePlayerUnit, defender);
        // After executing a Melee Attack, resets the Enemy initial tile (typically, shows the Movement Range - must take into account other cases in the future).
        targetTile.tileShaderController.SetTileGlowIntensity(1f);
    }

    //public void ExecuteMagnet(TileController targetTile) // Move Magnet to dedicated SO Class
    //{
    //    var attacker = GetActivePlayerUnit();
    //    if (attacker == null || targetTile?.detectedUnit == null) return;

    //    var defender = targetTile.detectedUnit.GetComponent<Unit>();
    //    if (defender == null || LookUpDeityComponent(defender)) return;

    //    int magnetRange = 3;
    //    Vector2Int attackerPos = attacker.GetGridPosition();
    //    Vector2Int defenderPos = defender.GetGridPosition();

    //    // Check if the Magnet target is out of range (redundant, the cursor already does this check).
    //    if (GetManhattanDistance(attackerPos, defenderPos) > magnetRange) return;

    //    // Return if the Magnet target is sitting on the adjacent tile
    //    if (GetManhattanDistance(attackerPos, defenderPos) <= 1)
    //    {
    //        OnUsedMeleeAction?.Invoke($"{targetTile.detectedUnit.GetComponent<Unit>().unitTemplate.unitName} is already close.");
    //        return;
    //    }

    //    Vector2Int pullDirection = Vector2Int.zero;
    //    int deltaX = defenderPos.x - attackerPos.x;
    //    int deltaY = defenderPos.y - attackerPos.y;

    //    if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
    //        pullDirection.x = (int)Mathf.Sign(deltaX);
    //    else
    //        pullDirection.y = (int)Mathf.Sign(deltaY);

    //    //RemoveInvulnerableMask(defender);

    //    attacker.GetComponentInChildren<MagnetHelper>()?.OrientMagnet(attacker, defender);

    //    AnimateConveyorTiles(attackerPos, defenderPos, pullDirection, attacker);

    //    Vector2Int newGridPos = attackerPos + pullDirection;
    //    //newGridPos = ClampGridPosition(newGridPos);

    //    TileController destinationTile = GridManager.Instance.GetTileControllerInstance(newGridPos.x, newGridPos.y);

    //    if (destinationTile != null && destinationTile.currentSingleTileCondition != SingleTileCondition.occupied)
    //    {
    //        defender.ownedTile.detectedUnit = null;
    //        defender.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
    //        defender.ownedTile.tileShaderController.ResetEnemyTileFeedback();

    //        defender.MoveUnit(newGridPos.x, newGridPos.y, true);
    //        MoveUnitToTile(defender, destinationTile);

    //        destinationTile.detectedUnit = defender.gameObject;
    //        defender.ownedTile = destinationTile;
    //        defender.ownedTile.currentSingleTileCondition = SingleTileCondition.occupied;
    //        destinationTile.tileShaderController.EnemyTileFeedback();
    //        OnUsedMeleeAction?.Invoke($"{attacker.unitTemplate.unitName} used Magnet");
    //    }
    //    // Possibly redundant
    //    OnUsedMagnet?.Invoke();
    //    attacker.GetComponentInChildren<MagnetHelper>()?.DestroyMagnet();
    //}


    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        //activePlayerUnit.unitProfilePanel.GetComponent<UnitProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
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

    private void AnimateConveyorTiles(Vector2Int attackerPos, Vector2Int defenderPos, Vector2Int pullDirection, Unit attacker)
    {
        int distance = Mathf.Abs(defenderPos.x - attackerPos.x) + Mathf.Abs(defenderPos.y - attackerPos.y);
        Vector2Int currentPos = attackerPos;

        for (int i = 0; i < distance; i++)
        {
            currentPos += pullDirection;
            TileController currentTile = GridManager.Instance.GetTileControllerInstance(currentPos.x, currentPos.y);

            if (currentTile != null)
            {
                GameObject conveyorPlane = currentTile.GetComponentInChildren<ConveyorBeltHelper>()?.gameObject;

                if (conveyorPlane != null)
                {
                    Vector3 direction = new Vector3(pullDirection.x, 0, pullDirection.y);
                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                    conveyorPlane.transform.rotation = rotation;

                    ConveyorBeltHelper conveyorBeltHelper = conveyorPlane.GetComponent<ConveyorBeltHelper>();
                    conveyorBeltHelper?.ManageConveyorBelt(1);
                }
            }
        }
    }

    private bool LookUpDeityComponent(Unit defenderUnit)
    {
        return defenderUnit.gameObject.GetComponent<Deity>() != null;
    }

    //private void MoveUnitToTile(Unit unit, TileController destinationTile)
    //{
    //    if (unit == null || destinationTile == null)
    //        return;

    //    // Update previous tile
    //    if (unit.ownedTile != null)
    //    {
    //        unit.ownedTile.detectedUnit = null;
    //        unit.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
    //    }

    //    // Update new tile
    //    destinationTile.detectedUnit = unit.gameObject;
    //    destinationTile.currentSingleTileCondition = SingleTileCondition.occupied;

    //    // Update unit's reference
    //    unit.ownedTile = destinationTile;
    //    GameObject.FindGameObjectWithTag("CameraDistanceController").GetComponent<CameraDistanceController>().SortUnits();
    //}

    private Unit GetActivePlayerUnit() =>
        GameObject.FindGameObjectWithTag("ActivePlayerUnit")?.GetComponent<Unit>();

    private int GetManhattanDistance(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    //private void RemoveInvulnerableMask(Unit defender)
    //{
    //    if (defender.currentUnitBuff == Unit.UnitBuff.InvulnerableMask)
    //    {
    //        defender.currentUnitBuff = Unit.UnitBuff.Basic;
    //        defender.GetComponentInChildren<MaskFeedbackHelper>()?.DeactivateMask();
    //    }
    //}

    private bool IsEnemyReachable(Unit activePlayerUnit, TileController targetTile)
    {
        GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>();
        int distance = gridMovementController.GetDistance(activePlayerUnit.ownedTile, targetTile);
        if (distance > meleeRange)
        {
            return false;
        }
        return true;
    }
    //private Vector2Int ClampGridPosition(Vector2Int pos)
    //{
    //    var grid = GridManager.Instance;
    //    pos.x = Mathf.Clamp(pos.x, 0, grid.gridHorizontalSize - 1);
    //    pos.y = Mathf.Clamp(pos.y, 0, grid.gridVerticalSize - 1);
    //    return pos;
    //}

}
