using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyPoolController : MonoBehaviour
{
    public GameObject[] EnemyPoolGameObjects;
    // Start is called before the first frame update
    void OnEnable()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < GameManager._instance.currentEnemySelectionIds.Count; i++)
        {
            EnemyType type = GameManager._instance.currentEnemySelectionIds[i];
            Vector2 coords = GameManager._instance.currentEnemySelectionCoords[i];

            GameObject spawnedEnemy = Instantiate(EnemyPoolGameObjects[(int)type]);
            Unit unitComponent = spawnedEnemy.GetComponent<Unit>();

            unitComponent.startingXCoordinate = (int)coords.x;
            unitComponent.startingYCoordinate = (int)coords.y;
            SetTileDetectedUnit(unitComponent, spawnedEnemy);
        }
    }

    void SetTileDetectedUnit(Unit unitComponent, GameObject spawnedEnemy)
    {
        Transform tileSpawnPosition = GridManager.Instance.GetTileControllerInstance(unitComponent.startingXCoordinate, unitComponent.startingYCoordinate).transform;
        TileController enemyControlledTile = tileSpawnPosition.GetComponent<TileController>();
        enemyControlledTile.detectedUnit = spawnedEnemy;
        StartCoroutine(SetEnemyTilesAsOccupied(enemyControlledTile));
    }

    IEnumerator SetEnemyTilesAsOccupied(TileController enemyControlledTile)
    {
        yield return new WaitForSeconds(0.5f);
        enemyControlledTile.currentSingleTileCondition = SingleTileCondition.occupied;
    }
}
