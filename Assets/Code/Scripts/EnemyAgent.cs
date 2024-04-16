using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
//using static Enemy;
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

    public EnemyBehavior enemyBehavior;

    public void Start()
    {
        opportunityCounter.text = opportunity.ToString();
    }

    //Enemy Turn Sequence
    public void EnemyTurnEvents()
    {
        enemyBehavior.ExecuteBehavior(this);
    }

}
