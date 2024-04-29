using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

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
    [SerializeField] GameObject battleMomentsScreen;
    [SerializeField] float enemyTurnDuration;
    [SerializeField] BattleInterface battleInterface;
    [SerializeField] float battleMomentsScreenDeactivationTime;

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
        SetBattleType(currentBattleType);
        enemiesOnBattlefield = GameManager.Instance.currentEnemySelection;
    }

    private void OnEnable()
    {
        EnemyTurnManager.OnPlayerTurn += ActivateBattleMomentsScreen;
        EnemyTurnManager.OnDeityTurn += ActivateBattleMomentsScreen;
        TurnController.OnEnemyTurn += ActivateBattleMomentsScreen;
    }

    private void OnDisable()
    {
        EnemyTurnManager.OnPlayerTurn -= ActivateBattleMomentsScreen;
        EnemyTurnManager.OnDeityTurn -= ActivateBattleMomentsScreen;
        TurnController.OnEnemyTurn -= ActivateBattleMomentsScreen;

    }

    public void SetBattleType(BattleType battleType)
    {
        currentBattleType = battleType;
        //DeityCameraMovement();
    }
    void Start()
    {
        //Looks for the Game Manager at the start of the battle.
        gameManager = GameManager.Instance;
        ActivateBattleMomentsScreen("Battle Begins!");
        enemiesOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");

        SetTurnOrder();
    }

    public void SetTurnOrder()
    {
        currentTurnOrder = TurnOrder.playerTurn;
        turnDisplay.text = "Player Turn";
        turnCounter += 1;
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

    public void ActivateBattleMomentsScreen(string battleMomentText)
    {
        battleMomentsScreen.SetActive(true);
        battleMomentsScreen.GetComponentInChildren<TextMeshProUGUI>().text = battleMomentText;
        StartCoroutine("DeactivateBattleMomentsScreen");

    }
    IEnumerator DeactivateBattleMomentsScreen()
    {
        yield return new WaitForSeconds(battleMomentsScreenDeactivationTime);
        battleMomentsScreen.SetActive(false);
    }
}
