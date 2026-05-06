using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening; // Required for delayed calls

[CreateAssetMenu(fileName = "MoonPrincessBehavior", menuName = "DeityBehavior/MoonPrincess")]
public class DeityMoonPrincessBehavior : DeityBehavior
{
    public enum WindDirection
    {
        North,
        South,
        East,
        West,
        AwayFromDeity
    }

    public float vfxDurationDelay = 1f;
    public string deityName = "MoonPrincess";
    public string zapAttackName = "Lunar Zap";
    public string windGustAttackName = "Wind Gust";
    public GameObject zapAttackVFX; // Assign in Inspector
    public GameObject windGustVFX; // Assign in Inspector

    [Range(0, 100)]
    public int zapAttackChance = 50; // Tweak the chance for Zap Attack vs Wind Gust

    public WindDirection defaultWindDirection = WindDirection.AwayFromDeity;
    public bool randomizeWindDirection = false;

    private System.Random localRandom;
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

        int roll = localRandom.Next(1, 101);

        if (roll <= zapAttackChance)
        {
            AttemptZapAttack(deity, deityUnit);
        }
        else
        {
            AttemptWindGustAttack(deity, deityUnit);
        }
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
            BattleInterface.Instance.SetDeityNotification(
                $"{deityName} is now below one-third HP! Her power intensifies!");
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
                    GameObject newDeityAttackVFX = Instantiate(zapAttackVFX, playerUnit.ownedTile.transform.position,
                        Quaternion.identity);
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
        WindDirection currentWindDirection = defaultWindDirection;
        
        if (randomizeWindDirection)
        {
            // Pick a random direction (0 to 3) corresponding to North, South, East, West
            currentWindDirection = (WindDirection)localRandom.Next(0, 4);
        }

        string dirText = currentWindDirection == WindDirection.AwayFromDeity ? "away from her" : $"towards {currentWindDirection}";
        BattleInterface.Instance.SetDeityNotification($"{deityName} conjures a {windGustAttackName} blowing {dirText}!");
        
        DOVirtual.DelayedCall(1.5f, () => WindGustAttack(deity, deityUnit, currentWindDirection));
    }

    private void WindGustAttack(Deity deity, Unit deityUnit, WindDirection windDirection)
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
            Vector2Int normalizedPushDirection = Vector2Int.zero;

            if (windDirection == WindDirection.AwayFromDeity)
            {
                // Calculate direction from deity to unit
                Vector2Int direction = targetUnitGridPos - deityGridPos;

                // Invert direction for pushing away
                Vector2Int pushDirection = -direction;

                // Normalize the direction to get a single step
                int dx = 0;
                if (pushDirection.x > 0) dx = 1;
                else if (pushDirection.x < 0) dx = -1;

                int dy = 0;
                if (pushDirection.y > 0) dy = 1;
                else if (pushDirection.y < 0) dy = -1;

                normalizedPushDirection = new Vector2Int(dx, dy);
            }
            else
            {
                switch (windDirection)
                {
                    case WindDirection.North:
                        normalizedPushDirection = new Vector2Int(0, 1);
                        break;
                    case WindDirection.South:
                        normalizedPushDirection = new Vector2Int(0, -1);
                        break;
                    case WindDirection.East:
                        normalizedPushDirection = new Vector2Int(1, 0);
                        break;
                    case WindDirection.West:
                        normalizedPushDirection = new Vector2Int(-1, 0);
                        break;
                }
            }

            Vector2Int currentPos = targetUnitGridPos;
            bool fellIntoVoid = false;

            // 1. Raycast-like step loop to find the actual destination
            for (int i = 0; i < WIND_GUST_PUSH_DISTANCE; i++)
            {
                Vector2Int nextPos = currentPos + normalizedPushDirection;
                TileController nextTile = GridManager.Instance.GetTileControllerInstance(nextPos.x, nextPos.y);

                if (nextTile == null)
                {
                    // It's a hole! The unit falls into the void.
                    currentPos = nextPos;
                    fellIntoVoid = true;
                    break;
                }

                // Stop pushing if we hit a solid wall, obstacle, or an occupied tile
                if (nextTile.currentSingleTileCondition == SingleTileCondition.occupied ||
                    nextTile.tileType == TileType.Obstacle ||
                    nextTile.tileType == TileType.Environment)
                {
                    break;
                }

                currentPos = nextPos;
            }

            // Play VFX at unit's current position before it moves/dies
            if (windGustVFX != null)
            {
                GameObject newWindGustVFX = Instantiate(windGustVFX, targetUnit.transform.position, Quaternion.identity);
                Vector3 vfxOffset = new Vector3(0, 1, 0);
                newWindGustVFX.transform.localPosition += vfxOffset;
                Destroy(newWindGustVFX, vfxDurationDelay);
            }

            // 2. Apply the push result
            if (fellIntoVoid)
            {
                // Free its previous tile
                if (targetUnit.ownedTile != null)
                {
                    targetUnit.ownedTile.detectedUnit = null;
                    targetUnit.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
                    targetUnit.ownedTile = null;
                }

                targetUnit.currentXCoordinate = currentPos.x;
                targetUnit.currentYCoordinate = currentPos.y;

                Debug.Log($"{targetUnit.gameObject.name} was pushed into the void by Wind Gust and died.");

                // Build a tile-by-tile traversal sequence ending in a fall
                Sequence fallSequence = DOTween.Sequence();
                Vector3 lastValidPos = targetUnit.transform.position;
                Vector2Int tracePos = targetUnitGridPos;
                float stepDuration = 0.15f; // Fast push over tiles

                for (int i = 0; i < WIND_GUST_PUSH_DISTANCE; i++)
                {
                    tracePos += normalizedPushDirection;
                    TileController traceTile = GridManager.Instance.GetTileControllerInstance(tracePos.x, tracePos.y);

                    if (traceTile == null)
                    {
                        // We reached the void gap! Calculate the exact empty XZ coordinates.
                        Vector3 voidXZ = GridManager.Instance.GetWorldPositionFromGridCoordinates(tracePos.x, tracePos.y);
                        Vector3 plungeTarget = new Vector3(voidXZ.x, lastValidPos.y - 10f, voidXZ.z);

                        // Add a slide to the hole, then a plunge
                        fallSequence.Append(targetUnit.transform.DOMove(new Vector3(voidXZ.x, lastValidPos.y, voidXZ.z), stepDuration).SetEase(Ease.Linear));
                        fallSequence.Append(targetUnit.transform.DOMove(plungeTarget, 0.75f).SetEase(Ease.InQuad));
                        break;
                    }
                    else
                    {
                        // Move to this valid tile
                        Vector3 tileWorldPos = GridManager.Instance.GetWorldPositionFromGridCoordinates(tracePos.x, tracePos.y);
                        fallSequence.Append(targetUnit.transform.DOMove(tileWorldPos, stepDuration).SetEase(Ease.Linear));
                        lastValidPos = tileWorldPos;
                    }
                }

                fallSequence.OnComplete(() =>
                {
                    targetUnit.HealthPoints = 0;
                });
            }
            else if (currentPos != targetUnitGridPos)
            {
                // Attempt to move the unit normally to the valid safe tile we found
                bool moved = targetUnit.MoveUnit(currentPos.x, currentPos.y, true);

                if (moved)
                {
                    // Take possess of the target Tile destination.
                    targetUnit.ownedTile = GridManager.Instance.GetTileControllerInstance(currentPos.x, currentPos.y);
                    targetUnit.ownedTile.detectedUnit = targetUnit.gameObject;
                    targetUnit.currentXCoordinate = currentPos.x;
                    targetUnit.currentYCoordinate = currentPos.y;
                    targetUnit.ownedTile.currentSingleTileCondition = SingleTileCondition.occupied;
                    Debug.Log($"{targetUnit.gameObject.name} was pushed by Wind Gust to ({currentPos.x}, {currentPos.y})");
                }
                else
                {
                    Debug.LogWarning($"Could not push {targetUnit.gameObject.name} with Wind Gust to ({currentPos.x}, {currentPos.y}). It might be blocked.");
                }
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