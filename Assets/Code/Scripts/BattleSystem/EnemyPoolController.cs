using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyPoolController : MonoBehaviour
{
    public GameObject[] EnemyPoolGameObjects;
    [SerializeField] BattleManager battleManager;
    private void OnEnable()
    {
        BattleTypeController.OnBattleTypeInitialized += HandleBattleTypeInitialized;
    }

    private void OnDisable()
    {
        BattleTypeController.OnBattleTypeInitialized -= HandleBattleTypeInitialized;
    }

    private void HandleBattleTypeInitialized()
    {
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.RegularBattle)
        {
            SpawnEnemies();
            Debug.Log("Spawned Regular Battle Enemies");
        }
        else if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.PuzzleBattle)
        {
            SpawnEnemies();
            Debug.Log("Spawned Regular Battle Enemies");
        }
        else if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BossBattle)
        {
            SpawnBossBattleEnemies();
        }
    }

    private void Start()
    {
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BossBattle)
        {
            SetEnemiesStartingCoordinatesInBossBattle();
        }
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < GameManager.Instance.currentEnemySelectionIds.Count; i++)
        {
            EnemyType type = GameManager.Instance.currentEnemySelectionIds[i];
            Vector2 coords = GameManager.Instance.currentEnemySelectionCoords[i];

            GameObject spawnedEnemy = Instantiate(EnemyPoolGameObjects[(int)type]);
            Unit unitComponent = spawnedEnemy.GetComponent<Unit>();

            unitComponent.startingXCoordinate = (int)coords.x;
            unitComponent.startingYCoordinate = (int)coords.y;
            SetTileDetectedUnit(unitComponent, spawnedEnemy);
        }
    }

    void SpawnBossBattleEnemies()
    {
        Debug.Log("Spawning Boss Battle Enemies");
        List<Vector2> enemyCoords = GameManager.Instance.currentEnemySelectionCoords;

        foreach (var enemy in GameManager.Instance.currentEnemySelection)
        {
            Instantiate(enemy);
        }
    }
    void SetEnemiesStartingCoordinatesInBossBattle()
    {
        List<Vector2> enemyCoords = GameManager.Instance.currentEnemySelectionCoords;

        for (int i = 0; i < enemyCoords.Count; i++)
        {
            GameObject unitObject = BattleManager.Instance.enemiesOnBattlefield[i]; // Get the Enemy GameObject from the Enemies on Battlefield array
            Unit unitComponent = unitObject.GetComponent<Unit>(); // Access the Unit component

            if (unitComponent != null)
            {
                // Get the corresponding Vector2 from the coordinates list
                Vector2 coord = enemyCoords[i];
                Debug.Log(coord);
                // Assign the x and y values from the Vector2 to the Unit's coordinates
                unitComponent.startingXCoordinate = (int)coord.x;
                unitComponent.startingYCoordinate = (int)coord.y;
                SetTileDetectedUnit(unitComponent, unitObject);
                unitComponent.MoveUnit(unitComponent.startingXCoordinate, unitComponent.startingYCoordinate, false);
                unitComponent.SetPosition(unitComponent.startingXCoordinate, unitComponent.startingYCoordinate);
                Debug.Log("Assigned starting coordinates");
            }
            else
            {
                Debug.LogError($"GameObject at index {i} does not have a Unit component.");
            }
        }
    }

    void SetTileDetectedUnit(Unit unitComponent, GameObject spawnedEnemy)
    {
        Transform tileSpawnPosition = GridManager.Instance.GetTileControllerInstance(unitComponent.startingXCoordinate, unitComponent.startingYCoordinate).transform;
        TileController enemyControlledTile = tileSpawnPosition.GetComponent<TileController>();
        enemyControlledTile.detectedUnit = spawnedEnemy;
        unitComponent.ownedTile = enemyControlledTile;
        StartCoroutine(SetEnemyTilesAsOccupied(enemyControlledTile));
    }

    IEnumerator SetEnemyTilesAsOccupied(TileController enemyControlledTile)
    {
        yield return new WaitForSeconds(0.5f);
        enemyControlledTile.currentSingleTileCondition = SingleTileCondition.occupied;
    }
}
