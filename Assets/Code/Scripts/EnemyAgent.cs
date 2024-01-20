using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
//using static Enemy;
using UnityEditor.SearchService;
using System.Linq;
using static GridTargetingController;
using Unity.VisualScripting;

public class EnemyAgent : MonoBehaviour
{

    [Header("Unit Statistics")]
    //To be reworked. Should uniform and make the Enemy take the statistics from its own Scriptable Object template.
    /*public float healthPoints;
    */
    public int speed;
    public float attackPower = 5;

    [Header("Gameplay Logic")]
    //To be reworked. Player is a Unit now.
    public Player player;
    public int opportunity;
    [SerializeField] BattleManager battleManager;
    [SerializeField] int minEnemyMoveRollRange;
    [SerializeField] int maxEnemyMoveRollRange;

    [Header("Presentation")]
    [SerializeField] float enemyMoveElapsingTime;
    [SerializeField] Animator enemyAnimator;
    [SerializeField] GameObject attackVFXAnimator;
    public Vector3 enemyOriginalPosition;

    [Header("Enemy UI")]
    [SerializeField] TextMeshProUGUI healthPointsCounter;
    [SerializeField] TextMeshProUGUI opportunityCounter;
    [SerializeField] TextMeshProUGUI receivedDamageCounter;

    public int unitMovementLimit;
    public int currentXCoordinate;
    public int currentYCoordinate;

    [Header("Rewards for the Player")]
    public float enemyCoinsReward;
    public float enemyExperienceReward;

    public delegate void ExperienceRewardDelegate(float applicableExperienceReward);
    public static event ExperienceRewardDelegate OnExperienceReward;

    public delegate void CoinsRewardDelegate(float applicableCoinReward);
    public static event CoinsRewardDelegate OnCoinsReward;

    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;

    public delegate void CheckEnemiesOnBattlefield();
    public static event CheckEnemiesOnBattlefield OnCheckEnemiesOnBattlefield;

    public UnityEvent<Transform> EnemyMeleeAttack;
    public UnityEvent EnemyTakingDamage;

    public void Start()
    {
        //UpdateEnemyHealthDisplay();
        //Need to create a new Display (Final Fantasy Tactics style)
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        //Irrelevant. Player is a Unit now.
        battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();
        //Irrelevant. BattleManager is a Singleton now.
        opportunityCounter.text = opportunity.ToString();
        enemyOriginalPosition = this.gameObject.transform.position;
        //Irrelevant. I'm using the Grid Manager for controlling positions.
    }

    //Receiving Damage Logic
    /*public void TakeDamage(float receivedDamage)
    {
        healthPoints -= receivedDamage;
        Debug.Log("Receiving Damage from Player");
        //Should take damage away from the Unit HP, not the EnemyAgent class
        Debug.Log("Enemy damage =" + receivedDamage);
        UpdateEnemyHealthDisplay();
        UpdateEnemyReceivedDamageDisplay(receivedDamage);
        EnemyTakingDamage.Invoke();
        Invoke("EnemyTakesDamage", 0.5f);
    }
 
    public void EnemyTakesDamage()
    {
        if (healthPoints <= 0)
        {
            enemyAnimator.SetInteger("animation", 5);
            healthPointsCounter.text = "Dead";
            this.gameObject.tag = "DeadEnemy";
            OnExperienceReward(enemyExperienceReward);
            OnCoinsReward(enemyCoinsReward);
            OnCheckEnemiesOnBattlefield();
        }
    }
    */

    //Enemy Turn Sequence
    public void EnemyTurnEvents()
    {
        Unit targetUnit = SelectTargetUnit();
        //Decide the next move based on the battlefield situation and character attitude
        if (EnemyMoveRoll() >= maxEnemyMoveRollRange / 2)
        {
            Attack(targetUnit);
            Debug.Log("Enemy uses Attack");
        }
        else if (EnemyMoveRoll() <= maxEnemyMoveRollRange / 2)
        {
            Debug.Log("Enemy Uses Stun Ability");
            StunAbility();
        }
        //Define Opportunity Spending for the Enemy consistently with the Player
        opportunity -= 1;
        UpdateOpportunityDisplay();
    }

    //Rolls the Move the Enemy is going to use
    public int EnemyMoveRoll()
    {
        Debug.Log("Rolling Enemy move");
        int enemyMoveRoll = Random.Range(minEnemyMoveRollRange, maxEnemyMoveRollRange);
        return enemyMoveRoll;
    }

    //Selects the Target the Enemy is going to use
    public Unit SelectTargetUnit()
    {
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;

        Unit unitWithHighestHP = playerUnitsOnBattlefield
        .Select(go => go.GetComponent<Unit>())
        .Where(unit => unit != null)
        .OrderByDescending(unit => unit.unitHealthPoints)
        .FirstOrDefault();
        return unitWithHighestHP;
    }

    //Enemy Base Moveset
    public void Attack(Unit targetUnit)
    {
        if (this.gameObject.tag != "DeadEnemy")
        {
            //const float targetUnitReductionFactor = 0.05f;
            //float damageReductionFactor = (1.0f - (targetUnit.unitAmorRating * targetUnitReductionFactor) / (1.0f + targetUnitReductionFactor * targetUnitReductionFactor));
            float reducedDamage = attackPower; //* damageReductionFactor//
            targetUnit.HealthPoints -= (reducedDamage);
            //targetUnit.UpdatePlayerHealthDisplay();
            //targetUnit.PlayHurtAnimation();
            GameObject localAttackVFXAnimator = Instantiate(attackVFXAnimator, targetUnit.transform);
            //Rework for VFX to spawn directly on the Player's tile position
            localAttackVFXAnimator.GetComponent<Animator>().SetTrigger("BaseAnimation");
            Destroy(localAttackVFXAnimator, 1);
            //EnemyMeleeAttack.Invoke(player.GetComponent<Player>().transform);
            OnCheckPlayer();
            Debug.Log("Enemy Attacking");
        }
    }
    public void StunAbility()
    {
        if (this.gameObject.tag != "DeadEnemy")
        {
            /*targetUnit.GetComponent<UnitStatusController>().unitCurrentStatus = UnitStatus.stun;
            targetUnit.GetComponent<UnitStatusController>().UnitStun.Invoke();
            targetUnit.PlayHurtAnimation();
            attackVFX.transform.position = player.transform.position;
            attackVFX.Play();
            */
            OnCheckPlayer();
        }
    }

    //I should create a separe class for controlling the Enemy Details UI
    /*public void UpdateEnemyHealthDisplay()
    {
        healthPointsCounter.text = healthPoints.ToString();
    }
    */

    public void UpdateEnemyReceivedDamageDisplay(float receivedDamage)
    {
        receivedDamageCounter.text = receivedDamage.ToString();
        StartCoroutine("ResetReceivedDamageDisplay");
    }

    IEnumerator ResetReceivedDamageDisplay()
    {
        yield return new WaitForSeconds(1);
        receivedDamageCounter.text = "";
    }

    public void UpdateOpportunityDisplay()
    {
        opportunityCounter.text = opportunity.ToString();
    }
}
