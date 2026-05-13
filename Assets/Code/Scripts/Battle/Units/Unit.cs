using ProjectEdelweiss.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class HealthChangeEvent : UnityEvent<float>
{
}

public class Unit : MonoBehaviour
{
    public enum UnitLifeCondition
    {
        unitDead,
        unitAlive
    }

    public enum UnitPhase
    {
        Active,
        Waiting
    }

    public enum UnitBuff
    {
        Basic,
        InvulnerableMask
    }

    public enum UnitType
    {
        PlayerUnit,
        Godling,
        Deity,
        DeityShard
    }

    [Header("Unit Basics")] public string Id;
    public UnitTemplate unitTemplate;

    [Header("Grid Map Element")] public int unitMovementLimit;
    public int currentXCoordinate;
    public int currentYCoordinate;
    public int startingXCoordinate;
    public int startingYCoordinate;

    public UnitSelectionController unitSelectionController;
    public TileController ownedTile;

    [Header("Unit Instance Stats")] public float unitHealthPoints;
    public float unitMaxHealthPoints;
    public int unitOpportunityPoints;
    public int unitFaithPoints;
    public float unitManaPoints;
    public float unitMaxManaPoints;
    public float unitShieldPoints;
    public int unitOccupiedFoodSlots;

    [Header("Progression System Stats")] public float unitCoins;
    public float unitExperiencePoints;
    public Vector2 coinsRewardRange;
    public float experiencePointsReward;
    public float unitAttackPower;
    public float unitMagicPower;

    public float unitMeleeAttackBaseDamage;

    [Header("Gameplay Elements")] public UnitLifeCondition currentUnitLifeCondition;
    public UnitBuff currentUnitBuff;
    public UnitPhase currentUnitPhase;
    public UnitStatusController unitStatusController;
    public PrizeReleaseController fieldPrizeController;
    public UnitType unitType;

    public bool hasHookshot;
    public bool bossFlag = false;

    [Header("Deity Related")] public Deity linkedDeity;
    public Deity summonedLinkedDeity;
    public string LinkedDeityId; // This will store the ID of the linked Deity.

    [Header("Visuals")] public BattleFeedbackController battleFeedbackController;
    public GameObject unitProfilePanel;
    public SpriteRenderer unitSprite;
    public Animator characterAnimator;

    public delegate void CheckGameOver();

    public static event CheckGameOver OnCheckGameOver;


    public HealthChangeEvent onHealthChanged = new HealthChangeEvent();

    public UnityEvent<float> OnTakenDamage;

    public float HealthPoints
    {
        get { return unitHealthPoints; }
        set
        {
            if (unitHealthPoints != value)
            {
                unitHealthPoints = value;
                onHealthChanged?.Invoke(unitHealthPoints);
                CheckUnitHealthStatus();
            }
        }
    }

    private System.Random localRandom = new System.Random();

    public void Start()
    {
        // Only load fresh template stats if GameStatsManager hasn't already processed this unit.
        if (unitTemplate != null && unitMaxHealthPoints == 0)
        {
            RetrieveTemplateValues();
        }

        SetPhase(UnitPhase.Active);
    }

    private void SetPhase(UnitPhase phase)
    {
        currentUnitPhase = phase;
    }

    public void TakeDamage(float receivedDamage)
    {
        if (currentUnitBuff == UnitBuff.InvulnerableMask)
            return;

        // Calculate the effective damage after considering the shield points.
        float effectiveDamage = CalculateEffectiveDamage(receivedDamage, unitShieldPoints);

        // Apply the effective damage to health points.
        HealthPoints -= effectiveDamage;

        // Invoke the event with the received damage before mitigation.
        OnTakenDamage?.Invoke(receivedDamage);
        // Log the received and effective damage.

        var slider = transform.GetComponentInChildren<Slider>();
        if (slider != null)
        {
            slider.value = unitHealthPoints;

            Debug.Log($"Unit receives {receivedDamage} damage, mitigated to {effectiveDamage} effective damage");
        }

        // Only update Player UI if the damaged unit is truly a Player.
        if (this.gameObject.CompareTag(GameTags.Player) || this.gameObject.CompareTag(GameTags.ActivePlayerUnit))
        {
            BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateHPWrapper(this.unitTemplate.unitName);
        }
    }

    private float CalculateEffectiveDamage(float receivedDamage, float shieldPoints)
    {
        float damageMitigationPercentage =
            shieldPoints / (shieldPoints + 100); // Arbitrary scaling factor for shield effectiveness.
        float effectiveDamage = receivedDamage * (1 - damageMitigationPercentage);

        effectiveDamage = Mathf.Floor(effectiveDamage);

        // Optional: Ensure there�s always at least 1 damage, to avoid zero-damage cases
        return Mathf.Max(effectiveDamage, 1f);
    }

    public void SpendManaPoints(int spentManaAmount)
    {
        unitManaPoints -= spentManaAmount;
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
        yield return null;
    }

    public bool MoveUnit(int targetX, int targetY, bool ignoreUnitMovementLimit)
    {
        Vector2Int startGridPos = GridManager.Instance.GetGridCoordinatesFromWorldPosition(transform.position);

        List<TileController> path = GridManager.Instance
            .GetComponentInChildren<GridMovementController>()
            .FindPath(startGridPos.x, startGridPos.y, targetX, targetY);

        if (ignoreUnitMovementLimit)
            unitMovementLimit = 10000;

        // Ensure valid path and within limit
        if (path != null && path.Count > 1 && (path.Count - 1) <= unitMovementLimit)
        {
            GridManager.IsUnitMoving = true;

            StartCoroutine(FollowPath(path));

            unitMovementLimit = unitTemplate.unitMovemementLimit;
            return true;
        }

        unitMovementLimit = unitTemplate.unitMovemementLimit;
        return false;
    }

    private IEnumerator FollowPath(List<TileController> path)
    {
        float stepDelay = 0.05f;

        foreach (var tile in path)
        {
            // Il Grid Manager pensa a piazzarlo al Top!
            GridManager.Instance.PlaceUnitOnTileSurface(this.gameObject, tile);

            currentXCoordinate = tile.gridPosition.x;
            currentYCoordinate = tile.gridPosition.z;

            yield return new WaitForSeconds(stepDelay);
        }

        GameObject.FindGameObjectWithTag("CameraDistanceController")
            .GetComponent<CameraDistanceController>()?.SortUnits();

        if (gameObject.CompareTag("ActivePlayerUnit"))
            FindAnyObjectByType<UnitSelectionController>()?.ChangeActivePlayerUnitTile(this);

        GridManager.IsUnitMoving = false;
    }

    public bool CheckTileAvailability(int targetX, int targetY)
    {
        Vector2Int startGridPos = GridManager.Instance.GetGridCoordinatesFromWorldPosition(transform.position);

        List<TileController> path = GridManager.Instance.GetComponentInChildren<GridMovementController>()
            .FindPath(startGridPos.x, startGridPos.y, targetX, targetY);

        if (path != null && path.Count > 1 && (path.Count - 1) <= unitMovementLimit)
        {
            return true;
        }
        else
        {
            Debug.Log("No valid path found or path exceeds movement limit.");
            return false;
        }
    }

    public virtual void CheckUnitHealthStatus()
    {
        // This logic works for both Player Units and Enemies.
        if (unitHealthPoints > 0)
        {
            Debug.Log("Unit is Still Alive");
        }
        else if (unitHealthPoints <= 0)
        {
            //ComboController.Instance.IncreaseComboCounter(this);
            var meshRenderer = GetComponentInChildren<MeshRenderer>();

            if (unitSprite != null)
            {
                // Play Fade Animation on Sprite.
                if (characterAnimator != null)
                {
                    characterAnimator.SetTrigger("Die");
                }

                if (battleFeedbackController != null)
                {
                    battleFeedbackController.PlayUnitDeathAnimationVFX();
                }

                // Play Enemy Death SFX
                BattleSFXManager.PlaySound(SoundType.ENEMYDEATH);
            }
            else if (meshRenderer != null)
            {
                meshRenderer.material.color = Color.black;
            }

            currentUnitLifeCondition = UnitLifeCondition.unitDead;

            if (unitProfilePanel != null)
                Destroy(unitProfilePanel);

            var enemyTargetIcon = GameObject.FindGameObjectWithTag("EnemyTargetIcon");
            if (enemyTargetIcon != null)
                Destroy(enemyTargetIcon);

            // Destroy Stun Icon, temporary solution.
            if (battleFeedbackController != null)
            {
                // Destroy Stun Icon, temporary solution.
                if (battleFeedbackController.stunIcon != null)
                    Destroy(battleFeedbackController.stunIcon);
            }

            CheckEnemyDefeat();

            // Reset TileController color to Movement Range.
            // This assumes that a tile occupied by dead enemy is always in the Movement Range (vertical slice only).
            if (ownedTile != null)
            {
                ownedTile.tileShaderController.SetTileToMoveRangeColor();
                ownedTile.tileShaderController.SetTileGlowIntensity(1f);
            }

            OnCheckGameOver?.Invoke();

            // Deactivates Player Unit Profile when applicable.
            if (gameObject.CompareTag(GameTags.Player) || gameObject.CompareTag(GameTags.ActivePlayerUnit))
            {
                if (unitTemplate != null)
                {
                    BattleInterface.Instance.PlayerPartyProfilesUIManager.SetDeadUnitProfile(unitTemplate.unitName);
                }
            }
        }
    }

    protected virtual void CheckEnemyDefeat()
    {
        if (this.gameObject.tag != "Enemy")
            return;
        var activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        if (activePlayerUnit != null)
        {
            // After an Enemy dies, retrieve the Rewards from it.
            CheckBattleRewards(activePlayerUnit);
        }

        if (ownedTile != null)
        {
            ownedTile.currentSingleTileCondition = SingleTileCondition.free;
            ownedTile.detectedUnit = null;
        }
    }

    private void CheckBattleRewards(GameObject activePlayerUnit)
    {
        BattleRewardsController battleRewardsController = activePlayerUnit.GetComponent<BattleRewardsController>();
        int newKill = 1;
        battleRewardsController.IncreaseMultiKillCounter(newKill);
        float coinsReward;
        int multiKillMultiplier = battleRewardsController.CalculateMultiKillCounter();
        coinsReward = CalculateCoinsReward() * multiKillMultiplier;
        battleRewardsController.resetMultiKillCounter();
        // Applies the rewards to the Pool. The rewards have NOT been looted yet.
        battleRewardsController.AddCoinsRewardToCoinsRewardPool(coinsReward);
        battleRewardsController.AddExperienceRewardToExperienceRewardPool(experiencePointsReward);
        SpawnPrize();

        // Enemy Ingredient Loot

        var enemyLoot = transform.GetComponentInChildren<EnemyLoot>();
        if (enemyLoot == null)
            return;
        Ingredient lootedItem = enemyLoot.RollLootChance();
        if (lootedItem != null)
        {
            // Add Ingredient to temporary Loot
            battleRewardsController.AddTemporaryLoot(lootedItem);
        }
    }

    private void SpawnPrize()
    {
        if (fieldPrizeController != null && ownedTile != null)
        {
            fieldPrizeController.UnlockFieldPrize(ownedTile);
        }
        else if (ownedTile == null)
        {
            Debug.Log("Handle cases where enemy fell into void or in general went out of grid due to Player action");
        }
    }

    public float CalculateCoinsReward()
    {
        int coinsRewardMinRange = (int)coinsRewardRange.x;
        int coinsRewardMaxRange = (int)coinsRewardRange.y;
        float finalCoinsReward = localRandom.Next(coinsRewardMinRange, coinsRewardMaxRange);
        return finalCoinsReward;
    }

    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(currentXCoordinate, currentYCoordinate);
    }

    public void SetPosition(int x, int z) // Era la Y
    {
        currentXCoordinate = x;
        currentYCoordinate = z; // Sempre la Z spaziale che fa da asse orizzontale sulla mappa

        if (unitType != UnitType.Deity)
        {
            // Troviamo il tile...
            // NOTA: Usa GetTileControllerInstance Voxel o quello di compatibilit�
            TileController targetTile = GridManager.Instance.GetTileControllerInstance(x, z);
            if (targetTile != null)
            {
                // Appoggia perfettamente il giocatore!
                GridManager.Instance.PlaceUnitOnTileSurface(this.gameObject, targetTile);
            }
        }

        // Il resto del codice di Update di detectedUnit rimane uguale
    }

    public void RetrieveTemplateValues()
    {
        unitHealthPoints = unitTemplate.unitHealthPoints;
        unitMaxHealthPoints = unitTemplate.unitMaxHealthPoints;
        unitManaPoints = unitTemplate.unitManaPoints;
        unitMaxManaPoints = unitTemplate.unitManaPoints;
        unitOpportunityPoints = unitTemplate.unitOpportunityPoints;
        unitFaithPoints = unitTemplate.unitFaithPoints;
        unitShieldPoints = unitTemplate.unitShieldPoints;
        coinsRewardRange = unitTemplate.coinsRewardRange;
        unitAttackPower = unitTemplate.meleeAttackPower;
        unitMagicPower = unitTemplate.unitMagicPower;
        unitMovementLimit = unitTemplate.unitMovemementLimit;
        unitMeleeAttackBaseDamage = unitTemplate.unitMeleeAttackBaseDamage;
        currentUnitLifeCondition = UnitLifeCondition.unitAlive;

        experiencePointsReward = GetComponent<Unit>().unitTemplate.unitExperiencePointsReward;
    }

    public void FallIntoVoid(Vector2Int normalizedPushDirection, int distanceToVoid, float stepDuration = 0.15f)
    {
        // Free its previous tile
        if (ownedTile != null)
        {
            ownedTile.detectedUnit = null;
            ownedTile.currentSingleTileCondition = SingleTileCondition.free;
            ownedTile = null;
        }

        Debug.Log($"{gameObject.name} was pushed into the void and died.");

        // Build a tile-by-tile traversal sequence ending in a fall
        Sequence fallSequence = DOTween.Sequence();
        Vector3 lastValidPos = transform.position;
        Vector2Int tracePos = GetGridPosition();

        for (int i = 0; i < distanceToVoid; i++)
        {
            tracePos += normalizedPushDirection;
            TileController traceTile = GridManager.Instance.GetTileControllerInstance(tracePos.x, tracePos.y);

            bool isLastStep = (i == distanceToVoid - 1);

            if (traceTile == null)
            {
                // We reached the void gap! Calculate the exact empty XZ coordinates.
                Vector3 voidXZ = GridManager.Instance.GetWorldPositionFromGridCoordinates(tracePos.x, tracePos.y);
                Vector3 plungeTarget = new Vector3(voidXZ.x, lastValidPos.y - 10f, voidXZ.z);

                // Add a slide to the hole, then a plunge directly into it
                fallSequence.Append(transform.DOMove(new Vector3(voidXZ.x, lastValidPos.y, voidXZ.z), stepDuration)
                    .SetEase(Ease.Linear));
                fallSequence.Append(transform.DOMove(plungeTarget, 0.75f).SetEase(Ease.InQuad));
                break;
            }
            else
            {
                // Move to this valid tile
                Vector3 tileWorldPos = GridManager.Instance.GetWorldPositionFromGridCoordinates(tracePos.x, tracePos.y);
                fallSequence.Append(transform.DOMove(tileWorldPos, stepDuration).SetEase(Ease.Linear));
                lastValidPos = tileWorldPos;

                // If it's a decorative edge block and we are at the end of our push, fall off it!
                if (isLastStep)
                {
                    Vector3 furtherVoidXZ =
                        GridManager.Instance.GetWorldPositionFromGridCoordinates(tracePos.x + normalizedPushDirection.x,
                            tracePos.y + normalizedPushDirection.y);
                    Vector3 plungeTarget = new Vector3(furtherVoidXZ.x, lastValidPos.y - 10f, furtherVoidXZ.z);

                    // Add a slide OFF the edge block, then plunge
                    fallSequence.Append(transform
                        .DOMove(new Vector3(furtherVoidXZ.x, lastValidPos.y, furtherVoidXZ.z), stepDuration)
                        .SetEase(Ease.Linear));
                    fallSequence.Append(transform.DOMove(plungeTarget, 0.75f).SetEase(Ease.InQuad));
                }
            }
        }

        fallSequence.OnComplete(() => { HealthPoints = 0; });
    }
}