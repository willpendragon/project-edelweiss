using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public float healthPoints;
    public float attackPower = 5;
    public Player player;
    public int opportunity;
    public int speed;
    [SerializeField] float enemyMoveElapsingTime;
    [SerializeField] TextMeshProUGUI healthPointsCounter;
    [SerializeField] BattleManager battleManager;
    [SerializeField] TextMeshProUGUI opportunityCounter;
    [SerializeField] Animator enemyAnimator;
    [SerializeField] ParticleSystem attackVFX;
    [SerializeField] int minEnemyMoveRollRange;
    [SerializeField] int maxEnemyMoveRollRange;
    public Vector3 enemyOriginalPosition;

    public UnityEvent<Transform> EnemyMeleeAttack;

    public void Start()
    {
        UpdateEnemyHealthDisplay();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();
        opportunityCounter.text = opportunity.ToString();
        enemyOriginalPosition = this.gameObject.transform.position;
    }

    public void Update()
    {
        if (healthPoints <= 0)
        {
            enemyAnimator.SetInteger("animation", 5);
            healthPointsCounter.text = "Dead";
            this.gameObject.tag = "DeadEnemy";
        }
    }

    public void TakeDamage(float receivedDamage)
    {
        healthPoints -= receivedDamage;
        Debug.Log("Enemy damage =" + receivedDamage);
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
