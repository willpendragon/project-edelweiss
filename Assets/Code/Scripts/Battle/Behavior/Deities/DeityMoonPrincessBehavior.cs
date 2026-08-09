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

    public float angryHpThreshold = 0.666f; // 2/3 HP left
    public float veryAngryHpThreshold = 0.333f; // 1/3 HP left

    [Header("Rage Attack (triggered when enmity is full)")]
    public string rageAttackName = "Moonlight's Glare";
    public GameObject rageAttackVFX; // Assign in Inspector
    [SerializeField, Range(0f, 100f)] private float paralyzeSuccessChancePercentage = 75f;
    [SerializeField] private float rageAttackVfxYOffset = 1.0f;

    private System.Random localRandom;
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

        // RAGE ATTACK PRIORITY: Check if enmity is full first
        if (deity.PerformDeityEnmityCheck())
        {
            AttemptRageAttack(deity, deityUnit);
            return;
        }

        // Normal attack logic if enmity is not full
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

    private void AttemptRageAttack(Deity deity, Unit deityUnit)
    {
        BattleInterface.Instance.SetDeityNotification($"{deityName} used {rageAttackName}!");
        DOVirtual.DelayedCall(1.5f, () => DoRageAttack(deity, deityUnit));
    }

    private void DoRageAttack(Deity deity, Unit deityUnit)
    {
        BattleInterface.Instance.SetDeityNotification($"{deityName} used {rageAttackName}!");
        deity.deityCry.Play();

        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController")
            .GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;

        foreach (var playerUnitGO in playerUnitsOnBattlefield)
        {
            Unit playerUnit = playerUnitGO.GetComponent<Unit>();
            if (playerUnit != null && playerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitAlive)
            {
                // Instantiate VFX at player unit's position
                Vector3 vfxPosition = playerUnit.transform.position + new Vector3(0, rageAttackVfxYOffset, 0);
                if (rageAttackVFX != null)
                {
                    GameObject rageVFX = Instantiate(rageAttackVFX, vfxPosition, Quaternion.identity);
                    Destroy(rageVFX, vfxDurationDelay);
                }

                // Roll for paralyze success
                float randomRoll = (float)localRandom.NextDouble() * 100f;
                if (randomRoll <= paralyzeSuccessChancePercentage)
                {
                    // Apply stun status to the player unit
                    playerUnit.GetComponentInChildren<UnitStatusController>().unitCurrentStatus = UnitStatus.stun;
                    playerUnit.GetComponentInChildren<UnitStatusController>().UnitStun.Invoke();
                    
                    // Play visual feedback for the stun
                    PlayRageStunFeedback(playerUnit);
                }
            }
        }

        // Reset enmity after rage attack completes
        ResetDeityEnmity(deity);

        // Mark turn complete after animations finish
        float totalAnimationTime = vfxDurationDelay + 0.8f; // VFX duration + icon animation time
        DOVirtual.DelayedCall(totalAnimationTime, () =>
        {
            Debug.Log($"<color=cyan>[DeityMoonPrincessBehavior] Rage attack complete, turn finished</color>");
        });
    }

    private void PlayRageStunFeedback(Unit targetUnit)
    {
        // Create a sequence for timing
        Sequence sequence = DOTween.Sequence();

        // Add a delay to the sequence equal to the duration of the attack VFX
        sequence.AppendInterval(vfxDurationDelay);

        // Add a callback to instantiate the StunIcon after the VFX delay
        sequence.AppendCallback(() =>
        {
            // Instantiate the StunIcon
            GameObject stunIconInstance = Instantiate(Resources.Load<GameObject>("StunIcon"), targetUnit.transform);
            targetUnit.gameObject.GetComponent<BattleFeedbackController>().stunIcon = stunIconInstance;

            GridManager.Instance.statusIcons.Add(stunIconInstance);

            // Create a sequence for the StunIcon animations
            Sequence iconSequence = DOTween.Sequence();

            // Add a scale up animation for the pop effect
            iconSequence.Append(stunIconInstance.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.2f).SetEase(Ease.OutBack));

            // Add a scale back to normal size
            iconSequence.Append(stunIconInstance.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));

            // Add a shake animation
            iconSequence.Append(stunIconInstance.transform.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0), 10, 90, false, true));

            // Play the icon sequence
            iconSequence.Play();
        });
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

        if (hpPercentage <= veryAngryHpThreshold)
        {
            return 3f; // Very Angry State multiplier
        }
        else if (hpPercentage <= angryHpThreshold)
        {
            return 2f; // Angry State multiplier
        }
        else
        {
            return 1f; // Base State multiplier
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
                targetUnit.FallIntoVoid(normalizedPushDirection, WIND_GUST_PUSH_DISTANCE);
            }
            else if (currentPos != targetUnitGridPos)
            {
                // Save the original tile before moving
                TileController originalTile = targetUnit.ownedTile;
                
                // Attempt to move the unit normally to the valid safe tile we found
                bool moved = targetUnit.MoveUnit(currentPos.x, currentPos.y, true);

                if (moved)
                {
                    // Free the originally occupied tile
                    if (originalTile != null)
                    {
                        originalTile.detectedUnit = null;
                        originalTile.currentSingleTileCondition = SingleTileCondition.free;
                    }

                    // Take possess of the target Tile destination
                    TileController destinationTile = GridManager.Instance.GetTileControllerInstance(currentPos.x, currentPos.y);
                    if (destinationTile != null)
                    {
                        destinationTile.detectedUnit = targetUnit.gameObject;
                        destinationTile.currentSingleTileCondition = SingleTileCondition.occupied;
                        targetUnit.ownedTile = destinationTile;
                    }
                    
                    // Update unit's position coordinates
                    targetUnit.currentXCoordinate = currentPos.x;
                    targetUnit.currentYCoordinate = currentPos.y;
                    
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