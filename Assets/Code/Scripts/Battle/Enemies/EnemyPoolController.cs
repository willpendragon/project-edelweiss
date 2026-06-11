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
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.RegularBattle ||
            BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            SpawnEnemies();
            Debug.Log("Spawned Regular Battle Enemies");
        }
        else if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.PuzzleBattle)
        {
            SpawnEnemies();
            Debug.Log("Spawned Regular Battle Enemies");
        }
        //else if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BossBattle)
        //{
        //SpawnBossBattleEnemies();
        //}
    }

    private void Start()
    {
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BossBattle)
        {
            //SetEnemiesStartingCoordinatesInBossBattle();
        }
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < GameManager.Instance.EnemyPartyManager.currentEnemySelectionIds.Count; i++)
        {
            EnemyType type = GameManager.Instance.EnemyPartyManager.currentEnemySelectionIds[i];
            Vector2 coords = GameManager.Instance.EnemyPartyManager.currentEnemySelectionCoords[i];

            GameObject spawnedEnemy = Instantiate(EnemyPoolGameObjects[(int)type]);
            Unit unitComponent = spawnedEnemy.GetComponent<Unit>();

            unitComponent.startingXCoordinate = (int)coords.x;
            unitComponent.startingYCoordinate = (int)coords.y;
            SetTileDetectedUnit(unitComponent, spawnedEnemy);
            Debug.Log("Spawned Enemies on the Battlefield");
        }
    }

    private void SetTileDetectedUnit(Unit unitComponent, GameObject spawnedEnemy)
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