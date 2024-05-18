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

    public delegate void StunnerEnemyAttack(string attackName, string attackerName);
    public static event StunnerEnemyAttack OnStunnerEnemyAttack;

    [SerializeField] private GameObject attackVFXAnimator;

    private System.Random localRandom = new System.Random(); // Local random number generator

    public override void ExecuteBehavior(EnemyAgent enemyAgent)
    {
        if (enemyAgent.gameObject.tag != "DeadEnemy" && enemyAgent.gameObject.GetComponentInParent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        {
            Unit targetUnit = SelectTargetUnit();
            if (targetUnit != null)
            {
                if (EnemyMoveRoll() >= maxEnemyMoveRollRange / 2)
                {
                    StunAbility(targetUnit);
                    Debug.Log("Stun Ability Chance Roll Successful");
                }
                else
                {
                    OnStunnerEnemyAttack("Failed Stun", "Godling");
                }
            }
            else
            {
                Debug.Log("No valid target available. Passing turn.");
                // Optionally trigger an event here if other systems need to respond to a turn pass
                OnCheckPlayer?.Invoke(); // Assuming OnCheckPlayer might be repurposed for notifying pass turn
            }
            opportunity -= 1;
        }
        else
        {
            Debug.Log("Enemy Unit is dead and can't attack anymore");
        }
    }

    public int EnemyMoveRoll()
    {
        Debug.Log("Rolling Enemy move");
        int enemyMoveRoll = localRandom.Next(minEnemyMoveRollRange, maxEnemyMoveRollRange);
        return enemyMoveRoll;
    }

    public Unit SelectTargetUnit()
    {
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;

        Unit unitWithHighestHP = playerUnitsOnBattlefield
            .Select(go => go.GetComponent<Unit>())
            .Where(unit => unit != null && unit.GetComponentInChildren<UnitStatusController>().unitCurrentStatus != UnitStatus.stun)
            .OrderByDescending(unit => unit.unitHealthPoints)
            .FirstOrDefault();

        return unitWithHighestHP;
    }

    public void StunAbility(Unit targetUnit)
    {
        OnStunnerEnemyAttack("Stun", "Godling");

        targetUnit.GetComponentInChildren<UnitStatusController>().unitCurrentStatus = UnitStatus.stun;
        targetUnit.GetComponentInChildren<UnitStatusController>().UnitStun.Invoke();
        GameObject stunIconInstance = Instantiate(Resources.Load<GameObject>("StunIcon"), targetUnit.transform);
        GridManager.Instance.statusIcons.Add(stunIconInstance);
        Debug.Log("Testing Stunner Enemy Behaviour");
    }
}