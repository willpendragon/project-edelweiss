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
    public delegate void DeityTurn();
    public static event DeityTurn OnDeityTurn;
    public GameObject deity;
    public void Start()
    {
        AddEnemiesToQueue();
    }
    public void Update()
    {
        deity = GameObject.FindGameObjectWithTag("Deity");
    }
    public void AddEnemiesToQueue()
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
            currentEnemy.EnemyTurnEvents();
            //Add the logic for the Enemy's turn here
            yield return new WaitForSeconds(singleEnemyturnDuration);
            currentEnemyTurnIndex++;
        }
        if (deity != null)
        {
            OnDeityTurn();
            //battleManager.StartCoroutine("PassTurnToDeity");
            Debug.Log("Enemy turns are over. Passing turn to Deity");
        }
        else if (deity == null)
        {
            battleManager.PassTurnToPlayer();
            Debug.Log("No Deity on the battlefield. Passing turn to the Player");
        }
    }
}
