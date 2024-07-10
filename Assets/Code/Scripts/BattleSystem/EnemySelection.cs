using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelection : MonoBehaviour
{
    public EnemySelection preliminaryEnemySelection;
    public GameObject[] enemySelection;
    public GridManager gridManager;
    public List<EnemyType> EnemyTypeIds;
    public List<Vector2> EnemyCoordinates;
    public Level levelData;
    public int levelNumber;

    public void SelectMapNode()
    {
        GameManager.Instance.GenerateLevelData(levelData); // Generate random enemy data
    }
}
