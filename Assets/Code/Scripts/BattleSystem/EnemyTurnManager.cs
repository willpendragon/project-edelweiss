using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

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

    [SerializeField] IconDisplayHelper _iconDisplayHelper;

    public GameObject deity;

    private void OnEnable()
    {
        TurnController.OnEnemyTurnSwap += TriggerEnemyActions;
    }

    private void OnDisable()
    {
        TurnController.OnEnemyTurnSwap -= TriggerEnemyActions;
    }

    private void Start()
    {
        AddEnemiesToQueue();
    }
    private void Update()
    {
        // Fix this, doesn't make any sense to be on Update 07012024.
        deity = GameObject.FindGameObjectWithTag("Deity");
    }
    private void AddEnemiesToQueue()
    {
        GameObject[] enemiesOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");
        enemiesInQueue = new List<EnemyAgent>();

        foreach (GameObject enemyGameObject in enemiesOnBattlefield)
        {
            EnemyAgent enemyComponent = enemyGameObject.GetComponent<EnemyAgent>();
            if (enemyComponent == null)
                return;
            enemiesInQueue.Add(enemyComponent);
        }
    }
    private void TriggerEnemyActions()
    {
        enemiesInQueue.Sort((a, b) => b.speed.CompareTo(a.speed));
        currentEnemyTurnIndex = 0;
        StartCoroutine(ExecuteTurns());
    }
    private IEnumerator ExecuteTurns()
    {
        // Refactor this moving out Deity logic. Call the Deity Logic and separately (if the Deity is present).

        while (currentEnemyTurnIndex < enemiesInQueue.Count)
        {
            EnemyAgent activeEnemy = enemiesInQueue[currentEnemyTurnIndex];
            Debug.Log("Current Turn: " + activeEnemy.name);
            ActivateThinkingIcon(activeEnemy);

            if (activeEnemy.gameObject.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
            {
                _iconDisplayHelper.ShowIcon();
                activeEnemy.EnemyTurnEvents();
                yield return new WaitForSeconds(singleEnemyturnDuration);
                _iconDisplayHelper.HideIcon();
            }
            else
            {
                float deadEnemyTurnWaitingTime = 0.1f;
                yield return new WaitForSeconds(deadEnemyTurnWaitingTime);
            }
            currentEnemyTurnIndex++;
        }

        ActivateTrap();

        if (deity == null)
        {
            DOVirtual.DelayedCall(1f, () => OnPlayerTurn?.Invoke("Player Turn"));
            DOVirtual.DelayedCall(1f, () => OnPlayerTurnSwap?.Invoke());
        }
        else
        {
            float deityTurnDuration = 5f;
            DOVirtual.DelayedCall(1f, () => OnDeityTurn?.Invoke("Deity Turn"));
            yield return new WaitForSeconds(deityTurnDuration);
            OnPlayerTurn?.Invoke("Player Turn");
            DOVirtual.DelayedCall(1.5f, () => OnPlayerTurnSwap?.Invoke());
        }
    }

    private void ActivateThinkingIcon(EnemyAgent enemy)
    {
        _iconDisplayHelper = enemy.gameObject.GetComponentInChildren<IconDisplayHelper>();
    }

    private void ActivateTrap()
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
