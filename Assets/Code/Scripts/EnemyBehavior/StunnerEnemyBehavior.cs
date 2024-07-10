using DG.Tweening;
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
        // Define the Y offset for the VFX spawn position
        float yOffset = 1.0f;

        // Calculate the new spawn position with the Y offset
        Vector3 stunVFXSpawnPosition = targetUnit.transform.position + new Vector3(0, yOffset, 0);

        // Instantiate the VFX at the new position
        GameObject stunVFX = Instantiate(Resources.Load<GameObject>("StunAttackVFX"), stunVFXSpawnPosition, Quaternion.identity);

        // Get the duration of the VFX animation (you can set this to the actual duration of your VFX animation)
        float vfxDuration = 1.0f; // replace with the actual duration

        // Create a sequence
        Sequence sequence = DOTween.Sequence();

        // Add a delay to the sequence equal to the duration of the VFX
        sequence.AppendInterval(vfxDuration);

        // Add a callback to the sequence to instantiate the StunIcon after the delay
        sequence.AppendCallback(() =>
        {
            // Instantiate the StunIcon
            GameObject stunIconInstance = Instantiate(Resources.Load<GameObject>("StunIcon"), targetUnit.transform);
            GridManager.Instance.statusIcons.Add(stunIconInstance);

            // Create a sequence for the StunIcon animations
            Sequence iconSequence = DOTween.Sequence();

            // Add a scale up animation for the pop effect
            iconSequence.Append(stunIconInstance.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.2f).SetEase(Ease.OutBack));

            // Add a scale back to normal size
            iconSequence.Append(stunIconInstance.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));

            // Add a shake animation
            iconSequence.Append(stunIconInstance.transform.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0), 10, 90, false, true));

            // Play the icon sequence
            iconSequence.Play();
        });

        float stunVFXDestroyCountdown = 1.5f;
        Destroy(stunVFX, stunVFXDestroyCountdown);

        Debug.Log("Stunner Enemy Behaviour");
    }
}