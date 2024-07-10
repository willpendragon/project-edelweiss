using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HealthChangeEvent : UnityEvent<float> { }

public class Unit : MonoBehaviour
{
    public enum UnitLifeCondition
    {
        unitDead,
        unitAlive
    }

    [Header("Player Unit Basics")]

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

    [Header("Player Unit Instance Stats")]

    public float unitHealthPoints;
    public float unitMaxHealthPoints;
    public int unitOpportunityPoints;
    public float unitManaPoints;
    public float unitMaxManaPoints;
    public float unitShieldPoints;

    [Header("Progression System Stats")]

    public float unitCoins;
    public float unitExperiencePoints;
    public float coinsReward;
    public float experiencePointsReward;
    public float unitMagicPower;

    [Header("Gameplay Elements")]

    public UnitLifeCondition currentUnitLifeCondition;
    public UnitStatusController unitStatusController;
    public FieldPrizeController fieldPrizeController;


    [Header("Deity Related")]

    public Deity linkedDeity;
    public string LinkedDeityId; // This will store the ID of the linked Deity


    [Header("Visuals")]

    public BattleFeedbackController battleFeedbackController;
    public GameObject unitProfilePanel;


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
    public void Start()
    {
        if (unitTemplate != null)
        {
            unitHealthPoints = unitTemplate.unitHealthPoints;
            unitMaxHealthPoints = unitTemplate.unitMaxHealthPoints;
            unitManaPoints = unitTemplate.unitManaPoints;
            unitMaxManaPoints = unitTemplate.unitManaPoints;
            unitOpportunityPoints = unitTemplate.unitOpportunityPoints;
            unitShieldPoints = unitTemplate.unitShieldPoints;
            unitMagicPower = unitTemplate.unitMagicPower;

            currentUnitLifeCondition = UnitLifeCondition.unitAlive;

            coinsReward = GetComponent<Unit>().unitTemplate.unitCoinsReward;
            experiencePointsReward = GetComponent<Unit>().unitTemplate.unitExperiencePointsReward;
        }
    }

    public void TakeDamage(float receivedDamage)
    {
        HealthPoints -= receivedDamage;
        OnTakenDamage.Invoke(receivedDamage);
        Debug.Log("Unit receives Damage");
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

    public bool MoveUnit(int targetX, int targetY)
    {
        // Convert current world position to grid coordinates
        Vector2Int startGridPos = GridManager.Instance.GetGridCoordinatesFromWorldPosition(transform.position);

        // Find path using grid coordinates
        List<TileController> path = GridManager.Instance.GetComponentInChildren<GridMovementController>().FindPath(startGridPos.x, startGridPos.y, targetX, targetY);

        if (path != null && path.Count > 0 && path.Count <= unitMovementLimit)
        {
            StartCoroutine(FollowPath(path));
            return true;
        }
        else
        {
            Debug.Log("No valid path found or path exceeds movement limit.");
            return false;
        }
    }

    private IEnumerator FollowPath(List<TileController> path)
    {
        foreach (var tile in path)
        {
            // Convert grid coordinates back to world position for actual movement
            Vector3 worldPosition = GridManager.Instance.GetWorldPositionFromGridCoordinates(tile.tileXCoordinate, tile.tileYCoordinate);
            Vector3 targetPosition = worldPosition + new Vector3(0, transform.localScale.y / 2, 0);

            // Move to the next tile
            yield return StartCoroutine(MoveToPosition(targetPosition, 0.5f)); // Adjust the duration as needed

            // Update current grid coordinates
            currentXCoordinate = tile.tileXCoordinate;
            currentYCoordinate = tile.tileYCoordinate;
            Debug.Log(tile.name);
            Debug.Log($"Moving to Tile at: ({tile.tileXCoordinate}, {tile.tileYCoordinate})");
        }
    }


    public bool CheckTileAvailability(int targetX, int targetY)
    {
        // Convert current world position to grid coordinates
        Vector2Int startGridPos = GridManager.Instance.GetGridCoordinatesFromWorldPosition(transform.position);

        // Find path using grid coordinates
        List<TileController> path = GridManager.Instance.GetComponentInChildren<GridMovementController>().FindPath(startGridPos.x, startGridPos.y, targetX, targetY);

        if (path != null && path.Count > 0 && path.Count <= unitMovementLimit)
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
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            var meshRenderer = GetComponentInChildren<MeshRenderer>();

            if (spriteRenderer != null)
            {
                //Play Fade Animation on Sprite
                spriteRenderer.material.color = Color.black;
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
            unitSelectionController.currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitWaiting;
            Debug.Log("This Unit has died");
            Destroy(unitProfilePanel);
            Destroy(GameObject.FindGameObjectWithTag("EnemyTargetIcon"));
            if (this.gameObject.tag == "Enemy")
            {
                var activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
                if (activePlayerUnit != null)
                {
                    activePlayerUnit.GetComponent<BattleRewardsController>().AddCoinsRewardToCoinsRewardPool(coinsReward);
                    activePlayerUnit.GetComponent<BattleRewardsController>().AddExperienceRewardToExperienceRewardPool(experiencePointsReward);
                    Debug.Log("Adding Enemy and Experience Points Rewards to Active Player Units Rewards Pool");

                    //Spawn Prize on Battlefield
                    if (fieldPrizeController != null)
                    {
                        fieldPrizeController.SpawnFieldPrize(ownedTile);
                    }
                }
                ownedTile.currentSingleTileCondition = SingleTileCondition.free;
                ownedTile.detectedUnit = null;
                ownedTile = null;

            }
            OnCheckGameOver();
        }
    }

    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(currentXCoordinate, currentYCoordinate);
    }

    public void SetPosition(int x, int y)
    {
        // Update the unit's logical grid coordinates
        currentXCoordinate = x;
        currentYCoordinate = y;

        // Update the unit's physical position
        Vector3 newPosition = GridManager.Instance.GetWorldPositionFromGridCoordinates(x, y);
        transform.position = newPosition + new Vector3(0, transform.localScale.y / 2, 0);

        // Update the TileController's detected unit for both the old and new positions
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
}