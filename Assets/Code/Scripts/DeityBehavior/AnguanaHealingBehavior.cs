using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnguanaHealingBehavior", menuName = "DeityBehavior/AnguanaHealing")]
public class AnguanaHealingBehavior : DeityBehavior
{
    public override void ExecuteBehavior(Deity deity)
    {
        Debug.Log("Testing Anguana Alternative Behaviour");
    }
}
