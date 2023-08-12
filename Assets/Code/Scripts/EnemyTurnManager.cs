using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyTurnManager : MonoBehaviour
{
    public List<Enemy> enemiesInQueue;
    public int currentEnemyTurnIndex;
    public BattleManager battleManager;
    [SerializeField] float singleEnemyturnDuration;

    public void Start()
    {
        GameObject[] enemiesOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");
        enemiesInQueue = new List<Enemy>();        
        //Convert the array of Enemy GameObjects to a List<Enemy>
        foreach (GameObject enemyGameObject in enemiesOnBattlefield)
        {
            Enemy enemyComponent = enemyGameObject.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemiesInQueue.Add(enemyComponent);
            }
            else
            {
                Debug.Log("No Enemies found");
            }
        }
    }
    public void EnemyTurnSequence()
    {
        enemiesInQueue.Sort((a, b) => b.speed.CompareTo(a.speed));
        currentEnemyTurnIndex = 0;
        StartCoroutine(ExecuteTurns());
    }

    private IEnumerator ExecuteTurns()
    {
        while (currentEnemyTurnIndex < enemiesInQueue.Count)
        {
            Enemy currentEnemy = enemiesInQueue[currentEnemyTurnIndex];
            Debug.Log("Current Turn: " + currentEnemy.name);
            currentEnemy.Attack();
            //Add the logic for the Enemy's turn here
            yield return new WaitForSeconds(singleEnemyturnDuration);
            currentEnemyTurnIndex++;
        }
        battleManager.StartCoroutine("PassTurnToDeity");
        Debug.Log("Enemy turns are over.");
    }
}
