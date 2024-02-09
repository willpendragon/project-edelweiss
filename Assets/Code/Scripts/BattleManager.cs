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
public class BattleManager : MonoBehaviour
{
    public Player player;
    public GameObject battleBeginsScreen;
    [SerializeField] float enemyTurnDuration;
    [SerializeField] BattleInterface battleInterface;
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
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        SpawnEnemies();
        battleBeginsScreen.SetActive(true);
        StartCoroutine("DeactivateBattleBeginsScreen");
        enemiesOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");
        currentTurnOrder = TurnOrder.playerTurn;
        turnDisplay.text = "Player Turn";
        turnCounter += 1;
    }

    public void OnEnable()
    {
        Moveset.OnPlayerTurnIsOver += PassTurnToEnemies;
        /*Enemy.OnCheckPlayer += CheckPlayer;*/
        //EnemyAgent.OnCheckEnemiesOnBattlefield += CheckEnemies;
    }
    public void OnDisable()
    {
        Moveset.OnPlayerTurnIsOver -= PassTurnToEnemies;
        /*Enemy.OnCheckPlayer -= CheckPlayer;*/
        //EnemyAgent.OnCheckEnemiesOnBattlefield -= CheckEnemies;
    }
    public void SpawnEnemies()
    {

    }

    IEnumerator SendStatsSavingEvents()
    {
        yield return new WaitForSeconds(0.5f);
        OnSavePlayerHealth(player.healthPoints);
        OnSavePlayerCoinsReward(player.coins);
        OnSavePlayerExperienceReward(player.playerExperiencePoints);
    }
    public void CheckPlayer()
    {
        //Checks whether the Player has reached "0" HP. If that's the case, triggers the End of Battle screen
        //with a notification telling about the Player's defeat.
        if (player.healthPoints <= 0)
        {
            OnBattleEndResultsScreen("Player Defeat");
        }
    }
    public void PassTurnToEnemies()
    {
        //DEPRECATED
        //Hands the turn to the Enemies.
        PlayerTurnEnds.Invoke();
        currentTurnOrder = TurnOrder.enemyTurn;
        turnDisplay.text = "Enemy Turn";
        Debug.Log("Passing turn to Enemies");
        enemyTurnManager.EnemyTurnSequence();
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
