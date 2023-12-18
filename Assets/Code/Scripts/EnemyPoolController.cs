using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyPoolController : MonoBehaviour
{
    public GameObject[] EnemyPoolGameObjects;
    // Start is called before the first frame update
    void Awake()
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
        }

    }
}
