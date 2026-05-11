using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // Added for the delay/cycle management

[CreateAssetMenu(fileName = "AnguanaSummonBehavior", menuName = "DeityBehavior/AnguanaSummonBehavior")]
public class DeityAnguanaSummoningBehavior : DeityBehavior
{
    public float baseDamage = 20f;
    public string moveName = "Deity Summon Move";

    public override void ExecuteBehavior(Deity deity)
    {
        GameObject[] enemies = TurnController.instance.enemyUnitsOnBattlefield;

        // Roll between 1 and 12
        int roll = Random.Range(1, 13);
        int cycleDuration = (roll <= 6) ? 6 : 12;

        StartFreezeCycle(enemies, cycleDuration, deity);
    }

    public override void ExecuteBuffBehaviour(Deity deity, Unit linkedUnit)
    {
    }

    private void StartFreezeCycle(GameObject[] enemies, int duration, Deity deity)
    {
        // Apply Frozen status
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                Unit unit = enemy.GetComponent<Unit>();
                if (unit != null && unit.unitStatusController != null)
                {
                    // Freeze the unit
                    unit.unitStatusController.unitCurrentStatus = UnitStatus.stun;
                    unit.TakeDamage(baseDamage);
                    // Add Freeze feedback
                    Debug.Log($"{unit.unitTemplate.unitName} was frozen");
                }
            }
        }
        BattleInterface.Instance.SetDeityNotification(
            $"{deity.gameObject.GetComponent<Unit>().unitTemplate.unitName} used Frozen Punishment");
    }
}