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
public class BattleManager : MonoBehaviour
{
    public Player player;
    public GameObject enemyOne;
    public GameObject enemyTwo;
    public GameObject enemyThree;
    public GameObject enemyFour;
    public GameObject enemyFive;
    [SerializeField] Transform enemyOneSpot;
    [SerializeField] Transform enemyTwoSpot;
    [SerializeField] Transform enemyThreeSpot;
    [SerializeField] Transform enemyFourSpot;
    [SerializeField] Transform enemyFiveSpot;
    [SerializeField] Image battleIsOverScreen;
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

    public delegate void ChargeDeityPowerLoadingBar();
    public static event ChargeDeityPowerLoadingBar OnChargeDeityPowerLoadingBar;

    public UnityEvent PlayerTurnStarts;
    public UnityEvent PlayerTurnEnds;

    void Awake()
    {
        //Looks for a GameObject holding the Enemy selected before entering the Battle Node.
        //Consider refactoring this in a way that allows to use the Nodes on the GameManager.
        enemySelection = GameObject.FindGameObjectWithTag("EnemySelection").GetComponent<EnemySelection>();
        enemyOne = enemySelection.enemyOne;
        enemyTwo = enemySelection.enemyTwo;
        enemyThree = enemySelection.enemyThree;
        enemyFour = enemySelection.enemyFour;
        enemyFive = enemySelection.enemyFive;
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
//        Moveset.OnCheckEnemies += CheckEnemies;
        TileController.OnPlayerEscapedFromJudgmentAttack += ShowBattleIsOverScreen;
        Enemy.OnCheckPlayer += CheckPlayer;
        Enemy.OnCheckEnemiesOnBattlefield += CheckEnemies;
    }
    public void OnDisable()
    {
        Moveset.OnPlayerTurnIsOver -= PassTurnToEnemies;
 //       Moveset.OnCheckEnemies -= CheckEnemies;
        TileController.OnPlayerEscapedFromJudgmentAttack -= ShowBattleIsOverScreen;
        Enemy.OnCheckPlayer -= CheckPlayer;
        Enemy.OnCheckEnemiesOnBattlefield -= CheckEnemies;
    }
    public void SpawnEnemies()
    {
        //Makes the Enemies from the Selection appear on the Battlefield at specific spots.
        Instantiate(enemyOne, enemyOneSpot);
        Instantiate(enemyTwo, enemyTwoSpot);
        Instantiate(enemyThree, enemyThreeSpot);
        Instantiate(enemyFour, enemyFourSpot);
        Instantiate(enemyFive, enemyFiveSpot);
    }
    public void CheckEnemies()
    {
        //This method scans the Battlefield for Enemies that have been tagged as DeadEnemy.
        //If all of the Enemy have been marked as DeadEnemy, the battle is considered finished and the
        //end of Battle screen is triggered.
        GameObject[] deadEnemies = GameObject.FindGameObjectsWithTag("DeadEnemy");
        if (enemiesOnBattlefield.Length == deadEnemies.Length)
        {
            //If all of the enemies on the battlefield are dead, the Battle End gameplay sequence starts.
            gameManager.MarkCurrentNodeAsCompleted();
            battleIsOverScreen.transform.localScale = new Vector3(1, 1, 1);
            battleInterface.battleEndResult.text = "The Enemy was defeated";
            //Sends an Event at the end of the battle and communicates the final HP of the Player at the end of the battle.
            OnBattleEnd();
            StartCoroutine("SendStatsSavingEvents");
        }
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
            battleIsOverScreen.transform.localScale = new Vector3(1, 1, 1);
            battleInterface.battleEndResult.text = "Player Defeat";
        }
    }
    public void PassTurnToEnemies()
    {
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
            Enemy enemyScript = enemy.GetComponent<Enemy>();

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
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            enemyScript.opportunity = 1;
            enemyScript.UpdateOpportunityDisplay();
        }
    }

    IEnumerator DeactivateBattleBeginsScreen()
    {
        yield return new WaitForSeconds(2);
        battleBeginsScreen.SetActive(false);
    }

    public void ShowBattleIsOverScreen()
    {
        battleIsOverScreen.transform.localScale = new Vector3(1, 1, 1);
    }
}
