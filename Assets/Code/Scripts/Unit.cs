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

    public int unitMovementLimit;
    public int currentXCoordinate;
    public int currentYCoordinate;
    public int startingXCoordinate;
    public int startingYCoordinate;

    public UnitTemplate unitTemplate;

    public TileController ownedTile;

    //Player Stats    
    public float unitHealthPoints;
    public float unitMaxHealthPoints;
    public int unitOpportunityPoints;
    public float unitManaPoints;
    public float unitShieldPoints;
    public float unitCoins;
    public float unitExperiencePoints;
    public float coinsReward;
    public float experiencePointsReward;
    public Deity linkedDeity;

    public delegate void CheckGameOver();
    public static event CheckGameOver OnCheckGameOver;

    public UnitLifeCondition currentUnitLifeCondition;

    public HealthChangeEvent onHealthChanged = new HealthChangeEvent();

    public GameObject unitProfilePanel;

    public UnitStatusController unitStatusController;

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

    public void OnEnable()
    {
        GridManager.OnSetUnitInitialPositionOnGrid += SetUnitInitialPositionOnGrid;
    }
    public void OnDisable()
    {
        GridManager.OnSetUnitInitialPositionOnGrid -= SetUnitInitialPositionOnGrid;
    }

    public void Start()
    {
        if (unitTemplate != null)
        {
            unitHealthPoints = unitTemplate.unitHealthPoints;
            unitMaxHealthPoints = unitTemplate.unitMaxHealthPoints;
            unitManaPoints = unitTemplate.unitManaPoints;
            unitOpportunityPoints = unitTemplate.unitOpportunityPoints;
            unitShieldPoints = unitTemplate.unitShieldPoints;
            currentUnitLifeCondition = UnitLifeCondition.unitAlive;

            coinsReward = GetComponent<Unit>().unitTemplate.unitCoinsReward;
            experiencePointsReward = GetComponent<Unit>().unitTemplate.unitExperiencePointsReward;

        }
    }

    public void SetUnitInitialPositionOnGrid()
    {
        MoveUnit(startingXCoordinate, startingYCoordinate);
        Debug.Log("Moving at Start Position");
    }

    public void MoveUnit(int targetX, int targetY)
    {
        // Convert current world position to grid coordinates
        Vector2Int startGridPos = GridManager.Instance.GetGridCoordinatesFromWorldPosition(transform.position);

        // Find path using grid coordinates
        List<TileController> path = GridManager.Instance.GetComponentInChildren<GridMovementController>().FindPath(startGridPos.x, startGridPos.y, targetX, targetY);

        if (path != null && path.Count > 0 && path.Count <= unitMovementLimit)
        {
            foreach (var tile in path)
            {
                // Apply visual feedback for the path
                tile.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;

                // Convert grid coordinates back to world position for actual movement
                Vector3 worldPosition = GridManager.Instance.GetWorldPositionFromGridCoordinates(tile.tileXCoordinate, tile.tileYCoordinate);
                transform.position = worldPosition + new Vector3(0, transform.localScale.y / 2, 0);

                // Update current grid coordinates
                currentXCoordinate = tile.tileXCoordinate;
                currentYCoordinate = tile.tileYCoordinate;
                //tile.GetComponent<TileController>().detectedUnit = this.gameObject;
                //030120240216 Removed because otherwise every tile of the movement path gets its DetectedUnit set as the moving Unit. 
                Debug.Log($"Moving to Tile at: ({tile.tileXCoordinate}, {tile.tileYCoordinate})");
            }
        }
        else
        {
            Debug.Log("No valid path found or path exceeds movement limit.");
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
            GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
            currentUnitLifeCondition = UnitLifeCondition.unitDead;
            Debug.Log("This Unit has died");
            Destroy(unitProfilePanel);
            Destroy(GameObject.FindGameObjectWithTag("EnemyTargetIcon"));
            if (this.gameObject.tag == "Enemy")
            {
                var activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
                activePlayerUnit.GetComponent<BattleRewardsController>().AddCoinsRewardToCoinsRewardPool(coinsReward);
                activePlayerUnit.GetComponent<BattleRewardsController>().AddExperienceRewardToExperienceRewardPool(experiencePointsReward);
                Debug.Log("Adding Enemy and Experience Points Rewards to Active Player Units Rewards Pool");
                var deityAchievementsController = GameObject.FindGameObjectWithTag("DeityAchievementsController").GetComponent<DeityAchievementsController>();
                deityAchievementsController.killedEnemies++;
            }
            OnCheckGameOver();
        }
    }
}