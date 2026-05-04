using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening; // Required for delayed calls

[CreateAssetMenu(fileName = "MoonPrincessBehavior", menuName = "DeityBehavior/MoonPrincess")]
public class DeityMoonPrincessBehavior : DeityBehavior
{
    public float vfxDurationDelay = 1f;
    public string deityName = "MoonPrincess";
    public string zapAttackName = "Lunar Zap";
    public string windGustAttackName = "Wind Gust";
    public GameObject zapAttackVFX; // Assign in Inspector
    public GameObject windGustVFX; // Assign in Inspector

    private System.Random localRandom;
    private bool _useZapAttackNext = true; // Flag to alternate behaviors
    private float _lastHpPercentage = 1f; // To track HP thresholds for notifications

    private const float HALF_HP_THRESHOLD = 0.5f;
    private const float ONE_THIRD_HP_THRESHOLD = 0.333f;
    private const int WIND_GUST_PUSH_DISTANCE = 3;

    public override void ExecuteBehavior(Deity deity)
    {
        if (deity.currentDeityStatus == Deity.DeityStatus.Summoned)
            return;

        if (localRandom == null)
            localRandom = new System.Random();

        Unit deityUnit = deity.gameObject.GetComponent<Unit>();
        if (deityUnit == null)
        {
            Debug.LogError("MoonPrincess deity object does not have a Unit component.");
            return;
        }

        CheckHPThresholds(deityUnit);

        if (_useZapAttackNext)
        {
            AttemptZapAttack(deity, deityUnit);
        }
        else
        {
            AttemptWindGustAttack(deity, deityUnit);
        }

        _useZapAttackNext = !_useZapAttackNext; // Toggle for the next turn
    }

    public override void ExecuteBuffBehaviour(Deity deity, Unit unit)
    {
        // MoonPrincess does not have a buff behavior for units.
    }

    private void CheckHPThresholds(Unit deityUnit)
    {
        float currentHp = deityUnit.unitHealthPoints;
        float maxHp = deityUnit.unitMaxHealthPoints;
        if (maxHp <= 0) return; // Avoid division by zero

        float currentHpPercentage = currentHp / maxHp;

        // Check for crossing below half HP
        if (currentHpPercentage < HALF_HP_THRESHOLD && _lastHpPercentage >= HALF_HP_THRESHOLD)
        {
            BattleInterface.Instance.SetDeityNotification($"{deityName} is now below half HP! Her power grows.");
        }
        // Check for crossing below one-third HP
        else if (currentHpPercentage < ONE_THIRD_HP_THRESHOLD && _lastHpPercentage >= ONE_THIRD_HP_THRESHOLD)
        {
            BattleInterface.Instance.SetDeityNotification($"{deityName} is now below one-third HP! Her power intensifies!");
        }

        _lastHpPercentage = currentHpPercentage; // Update last known HP percentage
    }

    private void AttemptZapAttack(Deity deity, Unit deityUnit)
    {
        BattleInterface.Instance.SetDeityNotification($"{deityName} prepares to unleash {zapAttackName}!");
        DOVirtual.DelayedCall(1.5f, () => ZapAttack(deity, deityUnit));
    }

    private void ZapAttack(Deity deity, Unit deityUnit)
    {
        BattleInterface.Instance.SetDeityNotification($"{deityName} used {zapAttackName}!");
        deity.deityCry.Play();

        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController")
            .GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;

        float damageModifier = GetZapDamageModifier(deityUnit);
        float baseDamage = deity.deitySpecialAttackPower;
        float totalDamage = baseDamage * damageModifier;

        foreach (var playerUnitGO in playerUnitsOnBattlefield)
        {
            Unit playerUnit = playerUnitGO.GetComponent<Unit>();
            if (playerUnit != null && playerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitAlive)
            {
                // Instantiate VFX
                if (zapAttackVFX != null)
                {
                    GameObject newDeityAttackVFX = Instantiate(zapAttackVFX, playerUnit.ownedTile.transform.position, Quaternion.identity);
                    Vector3 attackVFXOffset = new Vector3(0, 1, 0);
                    newDeityAttackVFX.transform.localPosition += attackVFXOffset;
                    Destroy(newDeityAttackVFX, vfxDurationDelay);
                }

                playerUnit.TakeDamage(totalDamage);
            }
        }
        ResetDeityEnmity(deity); // Assuming enmity reset after attack
    }

    private float GetZapDamageModifier(Unit deityUnit)
    {
        float currentHp = deityUnit.unitHealthPoints;
        float maxHp = deityUnit.unitMaxHealthPoints;
        if (maxHp <= 0) return 1f;

        float hpPercentage = currentHp / maxHp;

        if (hpPercentage < ONE_THIRD_HP_THRESHOLD)
        {
            return 3f;
        }
        else if (hpPercentage < HALF_HP_THRESHOLD)
        {
            return 2f;
        }
        else
        {
            return 1f;
        }
    }

    private void AttemptWindGustAttack(Deity deity, Unit deityUnit)
    {
        BattleInterface.Instance.SetDeityNotification($"{deityName} conjures a {windGustAttackName}!");
        DOVirtual.DelayedCall(1.5f, () => WindGustAttack(deity, deityUnit));
    }

    private void WindGustAttack(Deity deity, Unit deityUnit)
    {
        BattleInterface.Instance.SetDeityNotification($"{deityName} used {windGustAttackName}!");
        deity.deityCry.Play();

        List<Unit> allUnitsToAffect = new List<Unit>();

        // Add player units
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController")
            .GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;
        foreach (var unitGO in playerUnitsOnBattlefield)
        {
            Unit unit = unitGO.GetComponent<Unit>();
            if (unit != null && unit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitAlive)
            {
                allUnitsToAffect.Add(unit);
            }
        }

        // Add enemy units (if there are other enemies besides the deity itself)
        // Assuming enemy units are tagged "Enemy"
        GameObject[] enemyUnits = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var unitGO in enemyUnits)
        {
            Unit unit = unitGO.GetComponent<Unit>();
            if (unit != null && unit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitAlive && unit != deityUnit)
            {
                allUnitsToAffect.Add(unit);
            }
        }

        Vector2Int deityGridPos = deityUnit.GetGridPosition();

        foreach (Unit targetUnit in allUnitsToAffect)
        {
            Vector2Int targetUnitGridPos = targetUnit.GetGridPosition();

            // Calculate direction from deity to unit
            Vector2Int direction = targetUnitGridPos - deityGridPos;

            // Invert direction for pushing away
            Vector2Int pushDirection = -direction;

            // Normalize the direction to get a single step
            // This simplification assumes movement along cardinal or diagonal axes
            // For a more robust solution, might need to consider GridMovementController.FindPath
            int dx = 0;
            if (pushDirection.x > 0) dx = 1;
            else if (pushDirection.x < 0) dx = -1;

            int dy = 0;
            if (pushDirection.y > 0) dy = 1;
            else if (pushDirection.y < 0) dy = -1;

            Vector2Int normalizedPushDirection = new Vector2Int(dx, dy);

            // Calculate new target position
            Vector2Int newTargetPos = targetUnitGridPos + (normalizedPushDirection * WIND_GUST_PUSH_DISTANCE);

            // Attempt to move the unit
            // Ignore movement limit for forced push
            bool moved = targetUnit.MoveUnit(newTargetPos.x, newTargetPos.y, true); 

            if (moved)
            {
                Debug.Log($"{targetUnit.gameObject.name} was pushed by Wind Gust to ({newTargetPos.x}, {newTargetPos.y})");
            }
            else
            {
                Debug.LogWarning($"Could not push {targetUnit.gameObject.name} with Wind Gust to ({newTargetPos.x}, {newTargetPos.y}). It might be blocked.");
                // Even if not moved, play VFX at its current position or original target position if it's the closest valid.
            }
            
            // Play VFX at unit's current position (or desired position if it moved)
            if (windGustVFX != null)
            {
                GameObject newWindGustVFX = Instantiate(windGustVFX, targetUnit.ownedTile.transform.position, Quaternion.identity);
                Vector3 vfxOffset = new Vector3(0, 1, 0);
                newWindGustVFX.transform.localPosition += vfxOffset;
                Destroy(newWindGustVFX, vfxDurationDelay);
            }
        }
        ResetDeityEnmity(deity); // Assuming enmity reset after attack
    }

    private void ResetDeityEnmity(Deity deity)
    {
        // This method assumes the Deity component has a ResetDeityEnmity method.
        deity.ResetDeityEnmity();
    }
}
