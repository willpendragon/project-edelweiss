using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
//using static UnityEditor.PlayerSettings;

public enum TurnOrder
{
    playerTurn,
    enemyTurn
}

public enum FieldEffectStatus
{
    active,
    inactive
}

public enum BattleType
{
    regularBattle,
    battleWithDeity

}

public class BattleManager : MonoBehaviour
{
    //public Player player;
    public GameObject battleBeginsScreen;
    [SerializeField] float enemyTurnDuration;
    [SerializeField] BattleInterface battleInterface;

    [SerializeField] DeityAchievementsController deityAchievementsController;


    public TextMeshProUGUI turnDisplay;
    public TextMeshProUGUI turnTracker;
    public int turnCounter;
    public GameObject[] enemiesOnBattlefield;
    public Deity deity;
    public TurnOrder currentTurnOrder;
    public EnemySelection enemySelection;
    public FieldEffectStatus fieldEffectStatus;
    private GameManager gameManager;
    public EnemyTurnManager enemyTurnManager;

    public BattleType currentBattleType;

    public delegate void SavePlayerHealth(float finalPlayerHealth);
    public static event SavePlayerHealth OnSavePlayerHealth;

    public delegate void SavePlayerCoinsReward(float coinsReward);
    public static event SavePlayerCoinsReward OnSavePlayerCoinsReward;

    public delegate void SavePlayerExperienceReward(float experienceReward);
    public static event SavePlayerExperienceReward OnSavePlayerExperienceReward;

    public delegate void BattleEnd();
    public static event BattleEnd OnBattleEnd;

    public delegate void BattleEndResultsScreen(string battleEndMessage);
    public static event BattleEndResultsScreen OnBattleEndResultsScreen;

    public delegate void ChargeDeityPowerLoadingBar();
    public static event ChargeDeityPowerLoadingBar OnChargeDeityPowerLoadingBar;

    public UnityEvent PlayerTurnStarts;
    public UnityEvent PlayerTurnEnds;

    public GridManager gridManager;

    void Awake()
    {
        enemiesOnBattlefield = GameManager._instance.currentEnemySelection;
    }
    void Start()
    {
        //Looks for the Game Manager at the start of the battle.
        gameManager = GameManager._instance;
        if (deityAchievementsController.CheckRequirements())
        {
            ActivateDeityBattle();
            battleBeginsScreen.SetActive(true);
            StartCoroutine("DeactivateBattleBeginsScreen");
            enemiesOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");
            currentTurnOrder = TurnOrder.playerTurn;
            turnDisplay.text = "Player Turn";
            turnCounter += 1;
        }
        else
        {
            battleBeginsScreen.SetActive(true);
            StartCoroutine("DeactivateBattleBeginsScreen");
            enemiesOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");
            currentTurnOrder = TurnOrder.playerTurn;
            turnDisplay.text = "Player Turn";
            turnCounter += 1;
        }
    }

    public void OnEnable()
    {
    }
    public void OnDisable()
    {
    }

    public void ActivateDeityBattle()
    {
        currentBattleType = BattleType.battleWithDeity;

    }

    public void PassTurnToPlayer()
    {
        //Hands the turn to the Player.
        currentTurnOrder = TurnOrder.playerTurn;
        UpdateTurnCounter();
        Debug.Log("Turn Passed to Player");
        PlayerTurnStarts.Invoke();
        OnChargeDeityPowerLoadingBar();
    }

    public bool AllEnemiesOpportunityZero()
    {
        foreach (GameObject enemy in enemiesOnBattlefield)
        {
            EnemyAgent enemyScript = enemy.GetComponent<EnemyAgent>();

            if (enemyScript != null && enemyScript.opportunity > 0)
            {
                return false;
            }
        }
        return true;
    }
    public void UpdateTurnCounter()
    {
        turnDisplay.text = "Player Turn";
        turnCounter += 1;
        turnTracker.text = turnCounter.ToString();
    }

    public void RestoreOpportunityEnemies()
    {
        foreach (GameObject enemy in enemiesOnBattlefield)
        {
            EnemyAgent enemyScript = enemy.GetComponent<EnemyAgent>();
            enemyScript.opportunity = 1;
        }
    }

    IEnumerator DeactivateBattleBeginsScreen()
    {
        yield return new WaitForSeconds(0.5f);
        battleBeginsScreen.SetActive(false);
    }
}
