using ProjectEdelweiss.Utils;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class HealthChangeEvent : UnityEvent<float> { }

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
        Deity
    }

    [Header("Unit Basics")]

    public string Id;
    public UnitTemplate unitTemplate;

    [Header("Grid Map Element")]

    public int unitMovementLimit;
    public int currentXCoordinate;
    public int currentYCoordinate;
    public int startingXCoordinate;
    public int startingYCoordinate;

    public UnitSelectionController unitSelectionController;
    public TileController ownedTile;

    [Header("Unit Instance Stats")]

    public float unitHealthPoints;
    public float unitMaxHealthPoints;
    public int unitOpportunityPoints;
    public int unitFaithPoints;
    public float unitManaPoints;
    public float unitMaxManaPoints;
    public float unitShieldPoints;
    public int unitFoodSlots;

    [Header("Progression System Stats")]

    public float unitCoins;
    public float unitExperiencePoints;
    public Vector2 coinsRewardRange;
    public float experiencePointsReward;
    public float unitAttackPower;
    public float unitMagicPower;

    public float unitMeleeAttackBaseDamage;

    [Header("Gameplay Elements")]

    public UnitLifeCondition currentUnitLifeCondition;
    public UnitBuff currentUnitBuff;
    public UnitPhase currentUnitPhase;
    public UnitStatusController unitStatusController;
    public FieldPrizeController fieldPrizeController;
    public UnitType unitType;

    public bool hasHookshot;
    public bool bossFlag = false;

    [Header("Deity Related")]

    public Deity linkedDeity;
    public Deity summonedLinkedDeity;
    public string LinkedDeityId; // This will store the ID of the linked Deity.

    [Header("Visuals")]

    public BattleFeedbackController battleFeedbackController;
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
        if (unitTemplate != null)
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
        OnTakenDamage.Invoke(receivedDamage);
        // Log the received and effective damage.

        var slider = transform.GetComponentInChildren<Slider>();
        if (slider != null)
        {
            slider.value = unitHealthPoints;

            Debug.Log($"Unit receives {receivedDamage} damage, mitigated to {effectiveDamage} effective damage");
        }

        if (this.gameObject.tag != GameTags.Enemy)
        {
            BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateHPWrapper(this.unitTemplate.unitName);
        }
    }
    private float CalculateEffectiveDamage(float receivedDamage, float shieldPoints)
    {
        float damageMitigationPercentage = shieldPoints / (shieldPoints + 100); // Arbitrary scaling factor for shield effectiveness.
        float effectiveDamage = receivedDamage * (1 - damageMitigationPercentage);

        effectiveDamage = Mathf.Floor(effectiveDamage);

        // Optional: Ensure there’s always at least 1 damage, to avoid zero-damage cases
        return Mathf.Max(effectiveDamage, 1f);
    }

    public void SpendManaPoints(int spentManaAmount)
    {
        unitManaPoints -= spentManaAmount;
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float time = 0;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    public bool MoveUnit(int targetX, int targetY, bool ignoreUnitMovementLimit)
    {
        // Convert current world position to grid coordinates.
        Vector2Int startGridPos = GridManager.Instance.GetGridCoordinatesFromWorldPosition(transform.position);

        // Find path using grid coordinates.
        List<TileController> path = GridManager.Instance.GetComponentInChildren<GridMovementController>()
            .FindPath(startGridPos.x, startGridPos.y, targetX, targetY);

        if (ignoreUnitMovementLimit)
        {
            unitMovementLimit = 10000;
        }

        if (path != null && path.Count > 1 && (path.Count - 1) <= unitMovementLimit)
        {
            StartCoroutine(FollowPath(path));
            unitMovementLimit = unitTemplate.unitMovemementLimit;
            return true;
        }
        else
        {
            unitMovementLimit = unitTemplate.unitMovemementLimit;
            return false;
        }
    }


    private IEnumerator FollowPath(List<TileController> path)
    {
        foreach (var tile in path)
        {
            // Convert grid coordinates back to world position for actual movement.
            Vector3 worldPosition = GridManager.Instance.GetWorldPositionFromGridCoordinates(tile.tileXCoordinate, tile.tileYCoordinate);
            Vector3 targetPosition = worldPosition + new Vector3(0, transform.localScale.y / 2, 0);

            // Adjust this value to make the Unit's movement across tiles faster.
            float moveToTileDurationTime = 0.15f;
            yield return StartCoroutine(MoveToPosition(targetPosition, moveToTileDurationTime));

            // Update current grid coordinates
            currentXCoordinate = tile.tileXCoordinate;
            currentYCoordinate = tile.tileYCoordinate;
            Debug.Log(tile.name);
            Debug.Log($"Moving to Tile at: ({tile.tileXCoordinate}, {tile.tileYCoordinate})");
        }
        GameObject.FindGameObjectWithTag("CameraDistanceController").GetComponent<CameraDistanceController>().SortUnits();
        // Show Active PlayerUnit Tile color
        if (gameObject.CompareTag("ActivePlayerUnit"))
        {
            var unitSelection = FindAnyObjectByType<UnitSelectionController>();
            unitSelection.ChangeActivePlayerUnitTile(this);
        }
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

    public void CheckUnitHealthStatus()
    {
        if (unitHealthPoints > 0)
        {
            Debug.Log("Unit is Still Alive");
        }
        else if (unitHealthPoints <= 0)
        {
            ComboController.Instance.IncreaseComboCounter(this);
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
            }
            else if (meshRenderer != null)
            {
                meshRenderer.material.color = Color.black;
            }
            currentUnitLifeCondition = UnitLifeCondition.unitDead;
            Destroy(unitProfilePanel);
            Destroy(GameObject.FindGameObjectWithTag("EnemyTargetIcon"));
            CheckEnemyDefeat();

            // Reset TileController color to Movement Range.
            // This assumes that a tile occupied by dead enemy is always in the Movement Range (vertical slice only).
            ownedTile.tileShaderController.SetTileToMoveRangeColor();
            ownedTile.tileShaderController.SetTileGlowIntensity(1f);
            OnCheckGameOver();
        }
    }

    private void CheckEnemyDefeat()
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
        if (fieldPrizeController != null)
        {
            fieldPrizeController.UnlockFieldPrize(ownedTile);
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

    public void SetPosition(int x, int y)
    {
        // Update the unit's logical grid coordinates.
        currentXCoordinate = x;
        currentYCoordinate = y;

        if (unitType != UnitType.Deity) // Deities can't change their physical position on the battlefield.
        {
            // Update the unit's physical position.
            Vector3 newPosition = GridManager.Instance.GetWorldPositionFromGridCoordinates(x, y);
            transform.position = newPosition + new Vector3(0, transform.localScale.y / 2, 0);
        }

        // Update the TileController's detected unit for both the old and new positions.
        TileController oldTile = GridManager.Instance.GetTileControllerInstance(currentXCoordinate, currentYCoordinate);
        if (oldTile != null)
        {
            oldTile.detectedUnit = null;
            oldTile.currentSingleTileCondition = SingleTileCondition.free;
        }

        TileController newTile = GridManager.Instance.GetTileControllerInstance(x, y);
        if (newTile != null)
        {
            newTile.detectedUnit = this.gameObject;
            newTile.currentSingleTileCondition = SingleTileCondition.occupied;
        }
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
}