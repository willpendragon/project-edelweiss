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

    public delegate void EnemyTurnStarted(EnemyAgent enemy);
    public static event EnemyTurnStarted OnEnemyTurnStarted;

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
        deity = GameObject.FindGameObjectWithTag("Deity");
    }
    private void AddEnemiesToQueue()
    {
        GameObject[] enemiesOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");
        enemiesInQueue = new List<EnemyAgent>();

        foreach (GameObject enemyGameObject in enemiesOnBattlefield)
        {
            EnemyAgent enemyComponent = enemyGameObject.GetComponent<EnemyAgent>();
            if (enemyComponent != null)
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
            Debug.Log($"<color=cyan>[EnemyTurnManager] Starting turn for {activeEnemy.name} (Index: {currentEnemyTurnIndex})</color>");
            ActivateThinkingIcon(activeEnemy);

            if (activeEnemy.gameObject.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
            {
                // Pan camera to this enemy - waits for previous enemy's parry to complete due to isTurnComplete check
                OnEnemyTurnStarted?.Invoke(activeEnemy);
                
                _iconDisplayHelper.ShowIcon();
                
                // Reset turn completion flag before starting enemy's turn
                activeEnemy.isTurnComplete = false;
                
                activeEnemy.EnemyTurnEvents();
                
                // Wait for the enemy to complete their turn, with timeout fallback
                float timeout = 10f; // Safety timeout to prevent infinite hangs
                float elapsedTime = 0f;
                
                while (!activeEnemy.isTurnComplete && elapsedTime < timeout)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                
                if (activeEnemy.isTurnComplete)
                {
                    Debug.Log($"<color=cyan>[EnemyTurnManager] {activeEnemy.name} completed turn normally (Time: {elapsedTime:F2}s)</color>");
                }
                else
                {
                    Debug.LogWarning($"<color=yellow>[EnemyTurnManager] {activeEnemy.name} TIMED OUT after {timeout}s - forcing completion</color>");
                }
                
                _iconDisplayHelper.HideIcon();
            }
            else
            {
                float deadEnemyTurnWaitingTime = 0.1f;
                Debug.Log($"<color=cyan>[EnemyTurnManager] {activeEnemy.name} is dead, skipping turn</color>");
                yield return new WaitForSeconds(deadEnemyTurnWaitingTime);
            }
            currentEnemyTurnIndex++;
        }

        ActivateTrap();

        if (deity == null)
        {
            DOVirtual.DelayedCall(1f, () => OnPlayerTurn?.Invoke("Player Turn"));
            BattleSFXManager.PlaySound(SoundType.NEXTTURN);
            DOVirtual.DelayedCall(1f, () => OnPlayerTurnSwap?.Invoke());
        }
        else
        {
            float deityTurnDuration = 5f;
            DOVirtual.DelayedCall(1f, () => OnDeityTurn?.Invoke("Deity Turn"));
            yield return new WaitForSeconds(deityTurnDuration);
            BattleSFXManager.PlaySound(SoundType.NEXTTURN);
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
