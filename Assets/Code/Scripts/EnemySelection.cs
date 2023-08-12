using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelection : MonoBehaviour
{
    public GameObject enemyOne;
    public GameObject enemyTwo;
    public GameObject enemyThree;

    public EnemySelection preliminaryEnemySelection;

    public void CreatePreliminaryEnemySelection()
    {
        enemyOne = preliminaryEnemySelection.enemyOne;
        enemyTwo = preliminaryEnemySelection.enemyTwo;
        enemyThree = preliminaryEnemySelection.enemyThree;
    }
}
