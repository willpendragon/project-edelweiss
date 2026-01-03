using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CafeBuffController : MonoBehaviour
{
    [SerializeField] PastrySlotUIController _pastrySlotUIController;
    int buffComboThreshold = 2;
    float stackedBuffsMultiplier = 0.01f; // Can be reinforced using permanent upgrades via Deity links.


    public void ApplyFoodBuff(FoodBuff foodBuff, Unit fedUnit)
    {
        // Removed for Playtest demo
        CheckBuffCombinations(fedUnit);
    }

    private void CheckBuffCombinations(Unit fedUnit)
    {
        // Reset the character's stats to baseline.

        fedUnit.unitAttackPower = fedUnit.unitTemplate.meleeAttackPower;
        fedUnit.unitShieldPoints = fedUnit.unitTemplate.unitShieldPoints;

        // Wipe the existing buffs list.
        fedUnit.GetComponent<UnitBuffController>().ClearAppliedBuffs();

        List<ItemFood> eatenFood = _pastrySlotUIController.GetHistory(fedUnit);

        var groupedFood = eatenFood
            .GroupBy(food => new { food.foodBuff.alignment, food.foodBuff.foodBuffType });

        foreach (var group in groupedFood)
        {
            int count = group.Count();
            float totalBaseValue = group.Sum(food => food.foodBuff.foodBuffAmount);

            float finalBuffValue = totalBaseValue;

            if (count >= buffComboThreshold)
            {
                // Fixed percentage bonus. Total sum + flat % bonus.
                float bonusValue = totalBaseValue * stackedBuffsMultiplier;
                finalBuffValue = totalBaseValue + bonusValue;
            }

            int totalDuration = group.Sum(food => food.foodBuff.foodBuffDurationDays);

            ApplyFoodBuffCombo(fedUnit, finalBuffValue, totalDuration, group.First().foodBuff);
        }
    }
    private void ApplyFoodBuffCombo(Unit fedUnit, float resultingBuffValue, int buffsDuration, FoodBuff foodBuff)
    {
        switch (foodBuff.foodBuffType)
        {
            case FoodBuff.FoodBuffType.Attack:
                fedUnit.unitAttackPower += resultingBuffValue;
                Debug.Log($"Applied {resultingBuffValue} Attack Power. Current Total: {fedUnit.unitAttackPower}");
                break;

            case FoodBuff.FoodBuffType.Defense:
                fedUnit.unitShieldPoints += (int)resultingBuffValue;
                break;
        }

        // Records the entry so we can clear it next time or handle expiration
        fedUnit.GetComponent<UnitBuffController>().CreateAppliedBuffEntry(resultingBuffValue, buffsDuration, foodBuff.foodBuffType);
    }

    // Save Applied Buffs

    // Load/Re-apply Applied Buffs

    // Consume Buffs Lifetime
}
