using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CafeBuffController : MonoBehaviour
{
    [SerializeField] PastrySlotUIController _pastrySlotUIController;
    int buffComboThreshold = 2;
    float stackedBuffsMultiplier = 0.2f; // Can be reinforced using permanent upgrades via Deity links.


    public void ApplyFoodBuff(FoodBuff foodBuff, Unit fedUnit)
    {
        // Removed for Playtest demo
        //CheckBuffCombinations(fedUnit);
    }

    private void CheckBuffCombinations(Unit fedUnit)
    {
        // Remove Existing Buffs
        List<ItemFood> eatenFood = new List<ItemFood>();
        // Use the PastrySlotController.GetHistory method (Unit param) to retrieve what a Unit has eaten.

        eatenFood = _pastrySlotUIController.GetHistory(fedUnit);

        // Group all of the Food with the same alignment and same type.

        var groupedFood = eatenFood
            .GroupBy(food => new { food.foodBuff.alignment, food.foodBuff.foodBuffType });

        foreach (var group in groupedFood)
        {
            var alignment = group.Key;

            int foundBuffs = group.Count();

            // If the number of aligned buffs is more than a certain threshold,
            // add a % added effect * number of aligned buffs on the effect of the aligned buffs.

            if (foundBuffs >= buffComboThreshold)
            {
                float totalBaseValue = group.Sum(food => food.foodBuff.foodBuffAmount);

                float multiplier = stackedBuffsMultiplier * foundBuffs;
                float comboBuffValue = totalBaseValue * (1 + multiplier);

                int buffsDuration = group.Sum(food => food.foodBuff.foodBuffDurationDays);
                ApplyFoodBuffCombo(fedUnit, comboBuffValue, buffsDuration, group.First().foodBuff);
            }
            else
            {
                float totalBaseValue = group.Sum(food => food.foodBuff.foodBuffAmount);
                int buffsDuration = group.Sum(food => food.foodBuff.foodBuffDurationDays);
                ApplyFoodBuffCombo(fedUnit, totalBaseValue, buffsDuration, group.First().foodBuff);
            }
        }
    }

    private void ApplyFoodBuffCombo(Unit fedUnit, float resultingBuffValue, int buffsDuration, FoodBuff foodBuff)
    {
        // Applies the buffs depending on its type.
        switch (foodBuff.foodBuffType)
        {
            case FoodBuff.FoodBuffType.Attack:
                fedUnit.unitAttackPower = fedUnit.unitTemplate.meleeAttackPower += resultingBuffValue;
                Debug.Log($"Applied {resultingBuffValue} Attack Power Buff on {fedUnit}");

                break;
            case FoodBuff.FoodBuffType.Defense:
                fedUnit.unitShieldPoints = fedUnit.unitTemplate.unitShieldPoints += (int)resultingBuffValue;
                Debug.Log($"Applied {resultingBuffValue} Defense Buff on {fedUnit}");
                break;
        }

        // Add 1 entry to the total of applied buffs on the Character and pair it with the duration days.
        fedUnit.GetComponent<UnitBuffController>().CreateAppliedBuffEntry(resultingBuffValue, buffsDuration, foodBuff.foodBuffType);
    }

    // Save Applied Buffs

    // Load/Re-apply Applied Buffs

    // Consume Buffs Lifetime
}
