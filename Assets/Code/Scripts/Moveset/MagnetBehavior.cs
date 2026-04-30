using UnityEngine;

[CreateAssetMenu(fileName = "New Magnet Behavior", menuName = "Moveset/MagnetBehavior")]

public class MagnetBehavior : PhysicalAttackBehavior
{
    public delegate void UsedMagnet();
    public static event UsedMagnet OnUsedMagnet;

    public override void AttackSequence(Unit targetUnit, TileController targetTile, Unit activePlayerUnit)
    {
        var attacker = GetActivePlayerUnit();
        if (attacker == null || targetTile?.detectedUnit == null) return;

        var defender = targetTile.detectedUnit.GetComponent<Unit>();
        if (defender == null || LookUpDeityComponent(defender)) return;

        int magnetRange = 3;
        Vector2Int attackerPos = attacker.GetGridPosition();
        Vector2Int defenderPos = defender.GetGridPosition();

        // Check if the Magnet target is out of range (redundant, the cursor already does this check).
        if (GetManhattanDistance(attackerPos, defenderPos) > magnetRange) return;

        // Return if the Magnet target is sitting on the adjacent tile
        if (GetManhattanDistance(attackerPos, defenderPos) <= 1)
        {
            BroadcastAttackNotification($"{targetTile.detectedUnit.GetComponent<Unit>().unitTemplate.unitName} is already close.");
            return;
        }

        // Play Magnet SFX
        BattleSFXManager.PlaySound(SoundType.MAGNET);
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
        //newGridPos = ClampGridPosition(newGridPos);

        TileController destinationTile = GridManager.Instance.GetTileControllerInstance(newGridPos.x, newGridPos.y);

        if (destinationTile != null && destinationTile.currentSingleTileCondition != SingleTileCondition.occupied)
        {
            defender.ownedTile.detectedUnit = null;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
            defender.ownedTile.tileShaderController.ResetEnemyTileFeedback();

            defender.MoveUnit(newGridPos.x, newGridPos.y, true);
            MoveUnitToTile(defender, destinationTile);

            destinationTile.detectedUnit = defender.gameObject;
            defender.ownedTile = destinationTile;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.occupied;
            destinationTile.tileShaderController.EnemyTileFeedback();
            BroadcastAttackNotification($"{attacker.unitTemplate.unitName} used Magnet");
        }
        OnUsedMagnet?.Invoke(); // Possibly redundant
        attacker.GetComponentInChildren<MagnetHelper>()?.DestroyMagnet();
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

    private int GetManhattanDistance(Vector2Int a, Vector2Int b) =>
    Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    private Unit GetActivePlayerUnit() =>
    GameObject.FindGameObjectWithTag("ActivePlayerUnit")?.GetComponent<Unit>();

    private bool LookUpDeityComponent(Unit defenderUnit)
    {
        return defenderUnit.gameObject.GetComponent<Deity>() != null;
    }
    private void RemoveInvulnerableMask(Unit defender)
    {
        if (defender.currentUnitBuff == Unit.UnitBuff.InvulnerableMask)
        {
            defender.currentUnitBuff = Unit.UnitBuff.Basic;
            defender.GetComponentInChildren<MaskFeedbackHelper>()?.DeactivateMask();
        }
    }

}
