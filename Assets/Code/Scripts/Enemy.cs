using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    public void Start()
    {
        UpdateEnemyHealthDisplay();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();
        opportunityCounter.text = opportunity.ToString();
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
            player.GetComponent<UnitStatusController>().unitCurrentStatus = UnitStatus.stun;
            //I can create an event for this on the Player.
            player.UpdatePlayerHealthDisplay();
            player.PlayHurtAnimation();
            player.GetComponent<UnitStatusController>().UnitStun.Invoke();
            attackVFX.transform.position = player.transform.position;
            attackVFX.Play();
        }
    }

    public void EnemyTurnEvents()
    {
        //Decide the next move based on the battlefield situation and character attitude
        Attack();
        opportunity -= 1;
        UpdateOpportunityDisplay();
    }

    public void UpdateOpportunityDisplay()
    {
        opportunityCounter.text = opportunity.ToString();
    }
}
