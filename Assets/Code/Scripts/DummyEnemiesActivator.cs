using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyEnemiesActivator : MonoBehaviour
{
    public BattleManager battleManager;
    public GameObject dummyEnemyOne;
    public GameObject dummyEnemyTwo;
    public GameObject dummyEnemyThree;
public void PopulateBattleWithDummyEnemies()
{
        battleManager.enemyOne = dummyEnemyOne;
        battleManager.enemyTwo = dummyEnemyTwo;
        battleManager.enemyThree = dummyEnemyThree;
        battleManager.battleBeginsScreen.SetActive(false);
        battleManager.SpawnEnemies();

}
}
