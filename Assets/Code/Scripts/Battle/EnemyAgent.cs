using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Linq;
public class EnemyAgent : MonoBehaviour
{

    [Header("Unit Statistics")]
    // To be reworked. Should uniform and make the Enemy take the statistics from its own Scriptable Object template.
    public int speed;
    public float attackPower = 60;

    [Header("Gameplay Logic")]
    // To be reworked. Player is a Unit now.
    public int opportunity;
    [SerializeField] BattleManager battleManager;
    [SerializeField] int minEnemyMoveRollRange;
    [SerializeField] int maxEnemyMoveRollRange;
    public EnemyBehavior enemyBehavior;

    [Header("Presentation")]
    [SerializeField] float enemyMoveElapsingTime;
    [SerializeField] Animator enemyAnimator;
    [SerializeField] GameObject attackVFXAnimator;
    public Vector3 enemyOriginalPosition;

    [Header("Enemy UI")]
    [SerializeField] TextMeshProUGUI healthPointsCounter;
    [SerializeField] TextMeshProUGUI opportunityCounter;
    [SerializeField] TextMeshProUGUI receivedDamageCounter;

    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;

    public delegate void CheckEnemiesOnBattlefield();
    public static event CheckEnemiesOnBattlefield OnCheckEnemiesOnBattlefield;

    public GameObject unitStunStatusIcon;

    public void Start()
    {
        //opportunityCounter.text = opportunity.ToString();
    }

    // Starts the Enemy Turn Sequence contained in the Scriptable Object.
    public void EnemyTurnEvents()
    {
        enemyBehavior.ExecuteBehavior(this);
    }
}