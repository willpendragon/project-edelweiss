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
        // CRITICAL: Validate list synchronization before looping
        if (GameManager.Instance.EnemyPartyManager.currentEnemySelectionIds.Count !=
            GameManager.Instance.EnemyPartyManager.currentEnemySelectionCoords.Count)
        {
            Debug.LogError($"[EnemyPoolController] Enemy ID list ({GameManager.Instance.EnemyPartyManager.currentEnemySelectionIds.Count}) and Coordinate list ({GameManager.Instance.EnemyPartyManager.currentEnemySelectionCoords.Count}) are out of sync! Cannot spawn enemies safely.");
            return;
        }

        // Validate EnemyPoolGameObjects array configuration
        if (EnemyPoolGameObjects == null || EnemyPoolGameObjects.Length == 0)
        {
            Debug.LogError("[EnemyPoolController] EnemyPoolGameObjects array is null or empty! Cannot spawn enemies. Please configure the array in the Unity Inspector.");
            return;
        }

        // Warn if array size doesn't match enum count
        int expectedSize = System.Enum.GetValues(typeof(EnemyType)).Length;
        if (EnemyPoolGameObjects.Length < expectedSize)
        {
            Debug.LogWarning($"[EnemyPoolController] EnemyPoolGameObjects array has {EnemyPoolGameObjects.Length} elements but EnemyType enum has {expectedSize} values. Some enemy types may fail to spawn.");
        }

        int successfulSpawns = 0;
        for (int i = 0; i < GameManager.Instance.EnemyPartyManager.currentEnemySelectionIds.Count; i++)
        {
            EnemyType type = GameManager.Instance.EnemyPartyManager.currentEnemySelectionIds[i];
            Vector2 coords = GameManager.Instance.EnemyPartyManager.currentEnemySelectionCoords[i];

            // Validate array bounds before access
            int typeIndex = (int)type;
            if (typeIndex < 0 || typeIndex >= EnemyPoolGameObjects.Length)
            {
                Debug.LogError($"[EnemyPoolController] Cannot spawn enemy of type '{type}' (index {typeIndex}). Index is out of bounds for EnemyPoolGameObjects array (length: {EnemyPoolGameObjects.Length}). Skipping this enemy.");
                continue;
            }

            // Validate prefab is not null
            if (EnemyPoolGameObjects[typeIndex] == null)
            {
                Debug.LogError($"[EnemyPoolController] EnemyPoolGameObjects[{typeIndex}] (type '{type}') is null! Please assign the prefab in the Unity Inspector. Skipping this enemy.");
                continue;
            }

            GameObject spawnedEnemy = Instantiate(EnemyPoolGameObjects[typeIndex]);
            Unit unitComponent = spawnedEnemy.GetComponent<Unit>();

            if (unitComponent == null)
            {
                Debug.LogError($"[EnemyPoolController] Spawned enemy of type '{type}' does not have a Unit component! Destroying and skipping.");
                Destroy(spawnedEnemy);
                continue;
            }

            unitComponent.startingXCoordinate = (int)coords.x;
            unitComponent.startingYCoordinate = (int)coords.y;
            SetTileDetectedUnit(unitComponent, spawnedEnemy);
            successfulSpawns++;
        }

        if (successfulSpawns > 0)
        {
            Debug.Log($"[EnemyPoolController] Successfully spawned {successfulSpawns} enemies on the battlefield.");
        }
        else
        {
            Debug.LogError("[EnemyPoolController] Failed to spawn any enemies! Battle initialization may be incomplete.");
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