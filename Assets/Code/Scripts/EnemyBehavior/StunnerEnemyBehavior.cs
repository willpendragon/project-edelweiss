using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "StunnerEnemyBehavior", menuName = "EnemyBehavior/StunnerEnemy")]
public class StunnerEnemyBehavior : EnemyBehavior
{
    [SerializeField] private int minEnemyMoveRollRange;
    [SerializeField] private int maxEnemyMoveRollRange;
    public int opportunity;

    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;

    [SerializeField] private GameObject attackVFXAnimator;

    private System.Random localRandom = new System.Random(); // Local random number generator

    public override void ExecuteBehavior(EnemyAgent enemyAgent)
    {
        if (enemyAgent.gameObject.tag != "DeadEnemy" && enemyAgent.gameObject.GetComponentInParent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        {
            Unit targetUnit = SelectTargetUnit();
            if (EnemyMoveRoll() >= maxEnemyMoveRollRange / 2)
            {
                StunAbility(targetUnit);
                Debug.Log("Stun Ability Chance Roll Successful");
            }
            opportunity -= 1;
        }
        else
        {
            Debug.Log("Enemy Unit is dead and can't attack anymore");
        }
    }

    // Rolls the Move the Enemy is going to use using the local random instance
    public int EnemyMoveRoll()
    {
        Debug.Log("Rolling Enemy move");
        int enemyMoveRoll = localRandom.Next(minEnemyMoveRollRange, maxEnemyMoveRollRange);
        return enemyMoveRoll;
    }

    // Selects the Target the Enemy is going to use
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

    public void StunAbility(Unit targetUnit)
    {
        targetUnit.GetComponentInChildren<UnitStatusController>().unitCurrentStatus = UnitStatus.stun;
        targetUnit.GetComponentInChildren<UnitStatusController>().UnitStun.Invoke();
        Instantiate(Resources.Load("StunIcon"), targetUnit.transform);
        Debug.Log("Testing Stunner Enemy Behaviour");
        // targetUnit.PlayHurtAnimation();
        // attackVFX.Play();
    }
}