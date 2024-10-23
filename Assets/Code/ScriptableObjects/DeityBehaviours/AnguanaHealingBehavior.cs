using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnguanaHealingBehavior", menuName = "DeityBehavior/AnguanaHealing")]
public class AnguanaHealingBehavior : DeityBehavior
{
    public float bubbleBuffShieldPointsIncreaseAmount = 9900f;
    public string deityBuffName = "Motherly Embrace";
    public string deityBuffDescription = "";
    public override void ExecuteBehavior(Deity deity)
    {
        GameObject currentPlayerUnitGO = GridManager.Instance.currentPlayerUnit;

        currentPlayerUnitGO.GetComponent<Unit>().unitShieldPoints += bubbleBuffShieldPointsIncreaseAmount;
        currentPlayerUnitGO.GetComponentInChildren<TileShaderController>()?.AnimateFadeHeight(2.08f, 2, Color.cyan);

        Debug.Log("Testing Anguana Alternative Behaviour");
    }
}
