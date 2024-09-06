using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyPoolController : MonoBehaviour
{
    public GameObject[] EnemyPoolGameObjects;
    [SerializeField] BattleManager battleManager;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (battleManager.currentBattleType == BattleType.regularBattle)
        {
            SpawnEnemies();
        }
        else if (battleManager.currentBattleType == BattleType.BossBattle)
        {
            SpawnBossBattleEnemies();
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

        for (int i = 0; i < enemyCoords.Count; i++)
        {
            GameObject unitObject = GameManager.Instance.currentEnemySelection[i]; // Get the GameObject from the list
            Unit unitComponent = unitObject.GetComponent<Unit>(); // Access the Unit component

            if (unitComponent != null)
            {
                // Get the corresponding Vector2 from the coordinates list
                Vector2 coord = enemyCoords[i];

                // Assign the x and y values from the Vector2 to the Unit's coordinates
                unitComponent.startingXCoordinate = (int)coord.x;
                unitComponent.startingYCoordinate = (int)coord.y;
            }
            else
            {
                Debug.LogError($"GameObject at index {i} does not have a Unit component.");
            }
        }

        foreach (var enemy in GameManager.Instance.currentEnemySelection)
        {
            Instantiate(enemy);
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
