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

    public delegate void DeityTurn(string deityTurn);
    public static event DeityTurn OnDeityTurn;

    public delegate void PlayerTurn(string playerTurn);
    public static event PlayerTurn OnPlayerTurn;

    public delegate void PlayerTurnSwap();
    public static event PlayerTurnSwap OnPlayerTurnSwap;

    public GameObject deity;

    private void OnEnable()
    {
        TurnController.OnEnemyTurnSwap += EnemyTurnSequence;
    }

    private void OnDisable()
    {
        TurnController.OnEnemyTurnSwap -= EnemyTurnSequence;
    }

    private void Start()
    {
        AddEnemiesToQueue();
    }
    private void Update()
    {
        //Fix this, doesn't make any sense to be on Update 07012024
        deity = GameObject.FindGameObjectWithTag("Deity");
    }
    private void AddEnemiesToQueue()
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
    private void EnemyTurnSequence()
    {
        enemiesInQueue.Sort((a, b) => b.speed.CompareTo(a.speed));
        currentEnemyTurnIndex = 0;
        StartCoroutine(ExecuteTurns());
    }
    private IEnumerator ExecuteTurns()
    {

        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.RegularBattle)
        {
            while (currentEnemyTurnIndex < enemiesInQueue.Count)
            {
                EnemyAgent currentEnemy = enemiesInQueue[currentEnemyTurnIndex];
                Debug.Log("Current Turn: " + currentEnemy.name);
                IconDisplayHelper iconDisplayHelper = currentEnemy.gameObject.GetComponentInChildren<IconDisplayHelper>();

                if (currentEnemy.gameObject.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
                {
                    iconDisplayHelper.ShowIcon();
                    currentEnemy.EnemyTurnEvents();
                    yield return new WaitForSeconds(singleEnemyturnDuration);
                    iconDisplayHelper.HideIcon();
                }
                else
                {
                    float deadEnemyTurnWaitingTime = 0.1f;
                    yield return new WaitForSeconds(deadEnemyTurnWaitingTime);
                }
                currentEnemyTurnIndex++;
            }

            TrapBehaviour();
            if (deity != null)
            {
                OnDeityTurn?.Invoke("Deity Turn");
                Debug.Log("Enemy turns are over. Passing turn to Deity");

                // Ensure we wait for the deity to finish its turn before proceeding.
                yield return new WaitForSeconds(singleEnemyturnDuration);

                // Now, switch to player turn explicitly.
                OnPlayerTurn?.Invoke("Player Turn");
                OnPlayerTurnSwap?.Invoke();
                Debug.Log("Deity turn is over. Passing turn to Player.");
            }
            else
            {
                OnPlayerTurn?.Invoke("Player Turn");
                OnPlayerTurnSwap?.Invoke();
                Debug.Log("No Deity on the battlefield. Passing turn to the Player");
            }
        }
        else if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            while (currentEnemyTurnIndex < enemiesInQueue.Count)
            {
                EnemyAgent currentEnemy = enemiesInQueue[currentEnemyTurnIndex];
                Debug.Log("Current Turn: " + currentEnemy.name);

                if (currentEnemy.gameObject.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
                {
                    currentEnemy.EnemyTurnEvents();
                    yield return new WaitForSeconds(singleEnemyturnDuration);
                }
                else
                {
                    float deadEnemyTurnWaitingTime = 0.1f;
                    yield return new WaitForSeconds(deadEnemyTurnWaitingTime);
                }

                currentEnemyTurnIndex++;
            }
            OnDeityTurn?.Invoke("Deity Turn");
            // Ensure deity turn ends before switching to the player.
            yield return new WaitForSeconds(singleEnemyturnDuration);

            OnPlayerTurn?.Invoke("Player Turn");
            OnPlayerTurnSwap?.Invoke();
            Debug.Log("Deity turn is over. Passing turn to Player.");
        }
    }
    private void TrapBehaviour()
    {
        //Need to move this in another class or move in a class of its own, following the single responsibility principle

        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            TrapController trapTile = tile.GetComponent<TrapController>();
            if (trapTile != null && trapTile.currentTrapActivationStatus == TrapController.TrapActivationStatus.active)
            {
                trapTile.ApplyTrapEffect();
            }
        }
    }

}
