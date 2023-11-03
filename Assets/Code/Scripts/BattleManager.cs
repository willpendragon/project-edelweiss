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
    [SerializeField] Transform enemyOneSpot;
    [SerializeField] Transform enemyTwoSpot;
    [SerializeField] Transform enemyThreeSpot;
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

    public delegate void BattleEnd(float finalPlayerHealth);
    public static event BattleEnd OnBattleEnd;

    void Awake()
    {
        //Looks for a GameObject holding the Enemy selected before entering the Battle Node.
        //Consider refactoring this in a way that allows to use the Nodes on the GameManager.
        enemySelection = GameObject.FindGameObjectWithTag("EnemySelection").GetComponent<EnemySelection>();
        enemyOne = enemySelection.enemyOne;
        enemyTwo = enemySelection.enemyTwo;
        enemyThree = enemySelection.enemyThree;
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
        TileController.OnPlayerEscapedFromJudgmentAttack += ShowBattleIsOverScreen;
    }
    public void OnDisable()
    {
        Moveset.OnPlayerTurnIsOver -= PassTurnToEnemies;
        TileController.OnPlayerEscapedFromJudgmentAttack += ShowBattleIsOverScreen;

    }
    void Update()
    {
        //Checks whether the Player has reached "0" HP. If that's the case, triggers the End of Battle screen
        //with a notification telling about the Player's defeat.
        if (player.healthPoints <= 0)
        {
            battleIsOverScreen.transform.localScale = new Vector3(1, 1, 1);
            battleInterface.battleEndResult.text = "Player Defeat";
        }
        CheckEnemies();
    }
    public void SpawnEnemies()
    {
        //Makes the Enemies from the Selection appear on the Battlefield at specific spots.
        Instantiate(enemyOne, enemyOneSpot);
        Instantiate(enemyTwo, enemyTwoSpot);
        Instantiate(enemyThree, enemyThreeSpot);
    }
    public void CheckEnemies()
    {
        //This method scans the Battlefield for Enemies that have been tagged as DeadEnemy.
        //If all of the Enemy have been marked as DeadEnemy, the battle is considered finished and the
        //end of Battle screen is triggered.
        GameObject[] deadEnemies = GameObject.FindGameObjectsWithTag("DeadEnemy");
        if (enemiesOnBattlefield.Length == deadEnemies.Length)
        {
            gameManager.MarkCurrentNodeAsCompleted();
            battleIsOverScreen.transform.localScale = new Vector3(1, 1, 1);
            battleInterface.battleEndResult.text = "The Enemy was defeated";
            //Sends an Event at the end of the battle and communicates the final HP of the Player at the end of the battle.
            OnBattleEnd(player.healthPoints);
        }
    }
    public void PassTurnToEnemies()
    {
        //Hands the turn to the Enemies.
        currentTurnOrder = TurnOrder.enemyTurn;
        turnDisplay.text = "Enemy Turn";
        Debug.Log("Passing turn to Enemies");
        enemyTurnManager.EnemyTurnSequence();
    }
    /*IEnumerator PassTurnToDeity()
    {
        //Hands the turn to the Deity after the Enemies Turn.
        yield return new WaitForSeconds(0);
        deity.DeityBehaviour();
    }
    */

    public void PassTurnToPlayer()
    {
        //Hands the turn to the Player.
        currentTurnOrder = TurnOrder.playerTurn;
        UpdateTurnCounter();
        Debug.Log("Turn Passed to Player");
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
