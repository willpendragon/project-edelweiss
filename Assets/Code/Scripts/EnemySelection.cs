using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelection : MonoBehaviour
{
    public GameObject enemyOne;
    public GameObject enemyTwo;
    public GameObject enemyThree;
    public GameObject enemyFour;
    public GameObject enemyFive;

    public EnemySelection preliminaryEnemySelection;
    public GameObject[] enemySelection;
    public GridManager gridManager;
    public List<EnemyType> EnemyTypeIds;
    public List<Vector2> EnemyCoordinates;
    public Level levelData;

    //public delegate void SelectedMapNodeWithEnemies(GameObject[] mapNodeEnemySelection);
    //public static event SelectedMapNodeWithEnemies OnSelectedMapNodeWithEnemies;

    /*
    public void CreatePreliminaryEnemySelection()
    {
        enemyOne = preliminaryEnemySelection.enemyOne;
        enemyTwo = preliminaryEnemySelection.enemyTwo;
        enemyThree = preliminaryEnemySelection.enemyThree;
        enemyFour = preliminaryEnemySelection.enemyFour;
        enemyFive = preliminaryEnemySelection.enemyFive;
    }
    */
    public void SelectedMapNode()
    {
        GameManager._instance.UpdateEnemyData(levelData);
    }
}