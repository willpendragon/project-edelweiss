using Unity.VisualScripting;
using UnityEngine;
using static TileController;

public class MeleePlayerAction : MonoBehaviour, IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    private int meleeRange = 2;

    public Vector2Int knockbackDirection;
    public int knockbackStrength = 2;

    public delegate void UsedMeleeAction(string moveName, string attackerName);
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
        Unit defender = enemyObject.GetComponent<Unit>();

        if (activePlayerUnit.hasHookshot)
        {
            ExecuteMagnet(targetTile);
            activePlayerUnit.unitOpportunityPoints--;
            UpdateActivePlayerUnitProfile(activePlayerUnit);
            return;
        }

        AttemptKnockback(activePlayerUnit, defender);
        HitTarget(activePlayerUnit, defender, targetTile);

        activePlayerUnit.unitOpportunityPoints--;
        UpdateActivePlayerUnitProfile(activePlayerUnit);

        OnUsedMeleeAction("Melee Attack", activePlayerUnit.unitTemplate.unitName);
        activePlayerUnit.GetComponent<BattleFeedbackController>().PlayMeleeAttackAnimation(activePlayerUnit, defender);
    }

    public void ExecuteMagnet(TileController targetTile)
    {
        var attacker = GetActivePlayerUnit();
        if (attacker == null || targetTile?.detectedUnit == null) return;

        var defender = targetTile.detectedUnit.GetComponent<Unit>();
        if (defender == null || LookUpDeityComponent(defender)) return;

        int magnetRange = 3;
        Vector2Int attackerPos = attacker.GetGridPosition();
        Vector2Int defenderPos = defender.GetGridPosition();

        if (GetManhattanDistance(attackerPos, defenderPos) > magnetRange) return;

        Vector2Int pullDirection = Vector2Int.zero;
        int deltaX = defenderPos.x - attackerPos.x;
        int deltaY = defenderPos.y - attackerPos.y;

        if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            pullDirection.x = (int)Mathf.Sign(deltaX);
        else
            pullDirection.y = (int)Mathf.Sign(deltaY);

        RemoveInvulnerableMask(defender);

        attacker.GetComponentInChildren<MagnetHelper>()?.OrientMagnet(attacker, defender);

        AnimateConveyorTiles(attackerPos, defenderPos, pullDirection, attacker);

        Vector2Int newGridPos = attackerPos + pullDirection;
        newGridPos = ClampGridPosition(newGridPos);

        TileController destinationTile = GridManager.Instance.GetTileControllerInstance(newGridPos.x, newGridPos.y);

        if (destinationTile != null && destinationTile.currentSingleTileCondition != SingleTileCondition.occupied)
        {
            defender.ownedTile.detectedUnit = null;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.free;

            defender.MoveUnit(newGridPos.x, newGridPos.y, true);
            MoveUnitToTile(defender, destinationTile);

            destinationTile.detectedUnit = defender.gameObject;
            defender.ownedTile = destinationTile;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.occupied;
            OnUsedMeleeAction?.Invoke("Magnet", attacker.unitTemplate.unitName);
        }

        OnUsedMagnet?.Invoke();
        attacker.GetComponentInChildren<MagnetHelper>()?.DestroyMagnet();
    }

    public void AttemptKnockback(Unit attacker, Unit defender)
    {
        if (!IsKnockbackPossible(attacker, defender.ownedTile))
            return;
        if (defender.unitType == Unit.UnitType.Deity)
            return;

        bool modifierIsActive = true;
        HitTarget(attacker, defender, modifierIsActive);
        ExecuteKnockback(attacker, defender);

        Vector2Int defenderPos = defender.GetGridPosition();
        Vector2Int newGridPos = defenderPos + (knockbackDirection * knockbackStrength);

        newGridPos = ClampGridPosition(newGridPos);

        TileController projectedTile = GridManager.Instance.GetTileControllerInstance(newGridPos.x, newGridPos.y);
        if (projectedTile == null)
            return;
        if (projectedTile.detectedUnit != null)
        {
            return;
        }

        if (defender.MoveUnit(newGridPos.x, newGridPos.y, true) && defender.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        {
            defender.ownedTile.detectedUnit = null;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.free;

            TileController destinationTile = GridManager.Instance.GetTileControllerInstance(newGridPos.x, newGridPos.y);
            MoveUnitToTile(defender, destinationTile);
            destinationTile.tileShaderController.ResetTileFadeHeightAnimation(destinationTile);
        }

        RemoveInvulnerableMask(defender);

        ResetTileColours();
    }

    private void ExecuteKnockback(Unit attacker, Unit defender)
    {
        Vector2Int attackerPos = attacker.GetGridPosition();
        Vector2Int defenderPos = defender.GetGridPosition();

        int deltaX = attackerPos.x - defenderPos.x;
        int deltaY = attackerPos.y - defenderPos.y;

        knockbackDirection = Vector2Int.zero;
        if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            knockbackDirection.x = -(int)Mathf.Sign(deltaX);
        else
            knockbackDirection.y = -(int)Mathf.Sign(deltaY);

        knockbackStrength = Mathf.Clamp(knockbackStrength, 1, 3);

        Vector2Int previewGridPos = defenderPos + (knockbackDirection * knockbackStrength);
        previewGridPos = ClampGridPosition(previewGridPos);

        TileController previewTile = GridManager.Instance.GetTileControllerInstance(previewGridPos.x, previewGridPos.y);
    }

    private bool IsKnockbackPossible(Unit activePlayerUnit, TileController targetTile)
    {
        DistanceController distanceController = GridManager.Instance.GetComponentInChildren<DistanceController>();
        return distanceController.CheckDistance(activePlayerUnit.ownedTile, targetTile);
    }

    private bool IsEnemyReachable(Unit activePlayerUnit, TileController targetTile)
    {
        GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>();
        int distance = gridMovementController.GetDistance(activePlayerUnit.ownedTile, targetTile);
        if (distance > meleeRange)
        {
            targetTile.tileShaderController.AnimateFadeHeightError(2.75f, 0.5f, Color.red);
            return false;
        }
        return true;
    }

    private void HitTarget(Unit attacker, Unit defender, bool modifierIsActive)
    {
        float damage = CalculateDamage(attacker, defender, modifierIsActive);
        defender.TakeDamage(damage);
    }

    private float CalculateDamage(Unit attacker, Unit defender, bool modifierIsActive)
    {
        float damageOutput = attacker.unitAttackPower * attacker.unitMeleeAttackBaseDamage;
        if (modifierIsActive)
            damageOutput += 2;
        return damageOutput;
    }

    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        activePlayerUnit.unitProfilePanel.GetComponent<UnitProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
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

    private void MoveUnitToTile(Unit unit, TileController destinationTile)
    {
        if (unit == null || destinationTile == null)
            return;

        // Update previous tile
        if (unit.ownedTile != null)
        {
            unit.ownedTile.detectedUnit = null;
            unit.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
        }

        // Update new tile
        destinationTile.detectedUnit = unit.gameObject;
        destinationTile.currentSingleTileCondition = SingleTileCondition.occupied;

        // Update unit's reference
        unit.ownedTile = destinationTile;
    }

    private Vector2Int ClampGridPosition(Vector2Int pos)
    {
        var grid = GridManager.Instance;
        pos.x = Mathf.Clamp(pos.x, 0, grid.gridHorizontalSize - 1);
        pos.y = Mathf.Clamp(pos.y, 0, grid.gridVerticalSize - 1);
        return pos;
    }

    private Unit GetActivePlayerUnit() =>
        GameObject.FindGameObjectWithTag("ActivePlayerUnit")?.GetComponent<Unit>();

    private int GetManhattanDistance(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private void RemoveInvulnerableMask(Unit defender)
    {
        if (defender.currentUnitBuff == Unit.UnitBuff.InvulnerableMask)
        {
            defender.currentUnitBuff = Unit.UnitBuff.Basic;
            defender.GetComponentInChildren<MaskFeedbackHelper>()?.DeactivateMask();
        }
    }
}
