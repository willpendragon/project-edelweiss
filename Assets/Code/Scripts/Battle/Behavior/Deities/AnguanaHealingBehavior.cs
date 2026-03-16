using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnguanaHealingBehavior", menuName = "DeityBehavior/AnguanaHealing")]
public class AnguanaHealingBehavior : DeityBehavior
{
    public float bubbleBuffShieldPointsIncreaseAmount = 9900f;
    public string deityBuffName = "Motherly Embrace";
    public string deityBuffDescription = "";
    public override void ExecuteBuffBehaviour(Deity deity, Unit linkedUnit)
    {
        //GameObject currentPlayerUnitGO = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        //currentPlayerUnitGO.GetComponent<Unit>().unitShieldPoints += bubbleBuffShieldPointsIncreaseAmount;
        //currentPlayerUnitGO.GetComponentInChildren<BuffVFX>()?.TriggerVFX();
        linkedUnit.unitShieldPoints += bubbleBuffShieldPointsIncreaseAmount;
        linkedUnit.gameObject.GetComponentInChildren<BuffVFX>()?.TriggerVFX();
    }

    public override void ExecuteBehavior(Deity deity)
    {
        //
    }
}
