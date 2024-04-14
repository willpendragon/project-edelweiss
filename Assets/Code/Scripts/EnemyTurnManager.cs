using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyTurnManager : MonoBehaviour
{
    public List<EnemyAgent> enemiesInQueue;
    public int currentEnemyTurnIndex;
    public BattleManager battleManager;
    [SerializeField] float singleEnemyturnDuration;

    public delegate void DeityTurn();
    public static event DeityTurn OnDeityTurn;

    public delegate void PlayerTurn(string playerTurn);
    public static event PlayerTurn OnPlayerTurn;

    public delegate void PlayerTurnSwap();
    public static event PlayerTurnSwap OnPlayerTurnSwap;

    public GameObject deity;

    public void OnEnable()
    {
        TurnController.OnEnemyTurnSwap += EnemyTurnSequence;
    }

    public void OnDisable()
    {
        TurnController.OnEnemyTurnSwap -= EnemyTurnSequence;
    }

    public void Start()
    {
        AddEnemiesToQueue();
    }
    public void Update()
    {
        //Fix this, doesn't make any sense to be on Update 07012024
        deity = GameObject.FindGameObjectWithTag("Deity");
    }
    public void AddEnemiesToQueue()
    {
        GameObject[] enemiesOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");
        enemiesInQueue = new List<EnemyAgent>();
        //Convert the array of Enemy GameObjects to a List<Enemy>
        foreach (GameObject enemyGameObject in enemiesOnBattlefield)
        {
            EnemyAgent enemyComponent = enemyGameObject.GetComponent<EnemyAgent>();
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
        if (battleManager.currentBattleType == BattleType.regularBattle)
        {
            while (currentEnemyTurnIndex < enemiesInQueue.Count)
            {
                EnemyAgent currentEnemy = enemiesInQueue[currentEnemyTurnIndex];
                Debug.Log("Current Turn: " + currentEnemy.name);
                currentEnemy.EnemyTurnEvents();
                //Add the logic for the Enemy's turn here
                yield return new WaitForSeconds(singleEnemyturnDuration);
                currentEnemyTurnIndex++;
            }
            if (deity != null)
            {
                OnDeityTurn();
                Debug.Log("Enemy turns are over. Passing turn to Deity");
            }
            else
            {
                OnPlayerTurn("Player Turn");
                OnPlayerTurnSwap();
                Debug.Log("No Deity on the battlefield. Passing turn to the Player");
                //Reenable Player UI. Replenish Player Opportunity Points.
            }
        }
        else if (battleManager.currentBattleType == BattleType.battleWithDeity)
        {
            Debug.Log("This is a battle with a Deity");
        }

    }
}
