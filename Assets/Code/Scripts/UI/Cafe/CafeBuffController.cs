using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CafeBuffController : MonoBehaviour
{
    [SerializeField] PastrySlotUIController _pastrySlotUIController;
    [SerializeField] private CafeSaveManager _cafeSaveManager;
    int buffComboThreshold = 2;
    float stackedBuffsMultiplier = 0.01f; // Can be reinforced using permanent upgrades via Deity links.


    public void ApplyFoodBuff(FoodBuff foodBuff, Unit fedUnit)
    {
        // Removed for Playtest demo
        CheckBuffCombinations(fedUnit);
        _cafeSaveManager.GameStatsManager.SaveCharacterData();
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
            float bonusMultiplier = (count >= buffComboThreshold) ? stackedBuffsMultiplier : 0f;

            foreach (var item in group)
            {
                // Each item gets its own duration and its own share of the alignment bonus
                float individualValue = item.foodBuff.foodBuffAmount + (item.foodBuff.foodBuffAmount * bonusMultiplier);
                int duration = item.foodBuff.foodBuffDurationDays;

                ApplyFoodBuffCombo(fedUnit, individualValue, duration, item.foodBuff);
            }
        }
    }
    private void ApplyFoodBuffCombo(Unit fedUnit, float resultingBuffValue, int buffsDuration, FoodBuff foodBuff)
    {
        switch (foodBuff.foodBuffType)
        {
            case FoodBuff.FoodBuffType.Attack:
                fedUnit.unitAttackPower += resultingBuffValue; // Use += to stack individual items
                Debug.Log($"Resulting Buff Value {resultingBuffValue}");
                break;

            case FoodBuff.FoodBuffType.Defense:
                fedUnit.unitShieldPoints += (int)resultingBuffValue;
                break;
        }
        fedUnit.GetComponent<UnitBuffController>().CreateAppliedBuffEntry(resultingBuffValue, buffsDuration, foodBuff.foodBuffType);

    }
}
