using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using static Enemy;

public class Enemy : MonoBehaviour
{
    [Header("Unit Statistics")]
    public float healthPoints;
    public float attackPower = 5;
    public int speed;

    [Header("Gameplay Logic")]
    public Player player;
    public int opportunity;
    [SerializeField] BattleManager battleManager;
    [SerializeField] int minEnemyMoveRollRange;
    [SerializeField] int maxEnemyMoveRollRange;

    [Header("Presentation")]
    [SerializeField] float enemyMoveElapsingTime;
    [SerializeField] Animator enemyAnimator;
    [SerializeField] ParticleSystem attackVFX;
    public Vector3 enemyOriginalPosition;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI healthPointsCounter;
    [SerializeField] TextMeshProUGUI opportunityCounter;

    [Header("Rewards")]
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
        UpdateEnemyHealthDisplay();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();
        opportunityCounter.text = opportunity.ToString();
        enemyOriginalPosition = this.gameObject.transform.position;
    }
    public void TakeDamage(float receivedDamage)
    {
        healthPoints -= receivedDamage;
        Debug.Log("Enemy damage =" + receivedDamage);
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

    public void UpdateEnemyHealthDisplay()
    {
        healthPointsCounter.text = healthPoints.ToString();
    }

    public void Attack()
    {
        if (this.gameObject.tag != "DeadEnemy")
        {
            player.healthPoints -= attackPower;
            //I can create an event for this on the Player.
            player.UpdatePlayerHealthDisplay();
            player.PlayHurtAnimation();
            attackVFX.transform.position = player.GetComponent<Player>().unitCurrentTile.transform.position;
            attackVFX.Play();
            EnemyMeleeAttack.Invoke(player.GetComponent<Player>().unitCurrentTile.transform);
            OnCheckPlayer();
            Debug.Log("Enemy Attacking");
        }
    }

    public void StunAbility()
    {
        if (this.gameObject.tag != "DeadEnemy")
        {
            player.GetComponent<UnitStatusController>().unitCurrentStatus = UnitStatus.stun;
            player.GetComponent<UnitStatusController>().UnitStun.Invoke();
            //I can create an event for this on the Player.
            //player.UpdatePlayerHealthDisplay();
            player.PlayHurtAnimation();
            attackVFX.transform.position = player.transform.position;
            attackVFX.Play();
            OnCheckPlayer();
        }
    }

    public void EnemyTurnEvents()
    {
        //Decide the next move based on the battlefield situation and character attitude
        if (EnemyMoveRoll() <= 6)
        {
            Attack();
            Debug.Log("Enemy Attack Roll");
        }
        else if (EnemyMoveRoll() <= 12)
        {
            Debug.Log("Enemy Stun Ability Roll");
            StunAbility();
        }
        opportunity -= 1;
        UpdateOpportunityDisplay();
    }

    public void UpdateOpportunityDisplay()
    {
        opportunityCounter.text = opportunity.ToString();
    }

    public int EnemyMoveRoll()
    {
        Debug.Log("Rolling Enemy move");
        int enemyMoveRoll = Random.Range(minEnemyMoveRollRange, maxEnemyMoveRollRange);
        return enemyMoveRoll;
    }
}
