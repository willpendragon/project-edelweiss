using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
//using static Enemy;
using UnityEditor.SearchService;
using System.Linq;
//using static GridTargetingController;
using Unity.VisualScripting;

public class EnemyAgent : MonoBehaviour
{

    [Header("Unit Statistics")]
    //To be reworked. Should uniform and make the Enemy take the statistics from its own Scriptable Object template.
    /*public float healthPoints;
    */
    public int speed;
    public float attackPower = 60;

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

    //[Header("Rewards for the Player")]
    //public float enemyCoinsReward;
    //public float enemyExperiencePointsReward;

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

    public GameObject unitStunStatusIcon;

    public void Start()
    {
        opportunityCounter.text = opportunity.ToString();
    }

    //Enemy Turn Sequence
    public void EnemyTurnEvents()
    {
        if (this.gameObject.tag != "DeadEnemy" && this.gameObject.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        //The Enemy Unit doesn't evaluate the next move if it's dead.
        {
            Unit targetUnit = SelectTargetUnit();
            //Decide the next move based on the battlefield situation and character attitude
            if (EnemyMoveRoll() >= maxEnemyMoveRollRange / 2)
            {
                Attack(targetUnit);
                Debug.Log("Enemy Attack Roll");
            }
            else if (EnemyMoveRoll() <= maxEnemyMoveRollRange / 2)
            {
                Debug.Log("Enemy Stun Ability Roll");
                StunAbility(targetUnit);
            }
            //Define Opportunity Spending for the Enemy consistently with the Player
            opportunity -= 1;
        }
        else
        {
            Debug.Log("Enemy Unit is dead and can't attack anymore");
        }
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
    public void StunAbility(Unit targetUnit)
    {
        targetUnit.GetComponentInChildren<UnitStatusController>().unitCurrentStatus = UnitStatus.stun;
        targetUnit.GetComponentInChildren<UnitStatusController>().UnitStun.Invoke();
        Instantiate(unitStunStatusIcon, targetUnit.transform);
        //targetUnit.PlayHurtAnimation();
        //attackVFX.transform.position = player.transform.position;
        //attackVFX.Play();


    }

}
