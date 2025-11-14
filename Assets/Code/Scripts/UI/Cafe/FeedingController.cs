using System;
using Unity.VisualScripting;
using UnityEngine;

public class FeedingController : MonoBehaviour
{
    [SerializeField] private CafeMenuUIController _cafeMenuUIController;
    [SerializeField] private CafeBuffController _cafeBuffController;
    public bool HandleFeeding(ItemFood foodItem, Unit fedUnit)
    {
        if (fedUnit.unitFoodSlots == fedUnit.unitTemplate.unitMaxFoodSlots)
        {
            _cafeMenuUIController.HandleNotifications($"{fedUnit.unitTemplate.unitName} is not hungry!");
            return false;
        }
        else
        {
            return ApplyFoodPrimaryEffect(foodItem, fedUnit);
            // If the character was successfully fed, will return true.
        }
    }

    private bool ApplyFoodPrimaryEffect(ItemFood foodItem, Unit fedUnit)
    {
        switch (foodItem.itemFoodType)
        {
            case ItemFoodType.HPRecovery:
                if (StatNotFull(fedUnit.unitHealthPoints, fedUnit.unitMaxHealthPoints))
                {
                    fedUnit.unitHealthPoints = Mathf.Clamp(fedUnit.unitHealthPoints + foodItem.recoveryAmount, 0, fedUnit.unitMaxHealthPoints);
                    _cafeMenuUIController.HandleNotifications($"{fedUnit.unitTemplate.unitName} recovered {foodItem.recoveryAmount} HP");
                    ApplyFoodBuff(foodItem.foodBuff, fedUnit);

                    return true;
                }
                else
                {
                    _cafeMenuUIController.HandleNotifications($"{fedUnit.unitTemplate.unitName} is already at full HP");
                    return false;
                }
            case ItemFoodType.ManaRecovery:
                if (StatNotFull(fedUnit.unitManaPoints, fedUnit.unitMaxManaPoints))
                {
                    fedUnit.unitManaPoints = Mathf.Clamp(fedUnit.unitManaPoints + foodItem.recoveryAmount, 0, fedUnit.unitMaxManaPoints);
                    _cafeMenuUIController.HandleNotifications($"{fedUnit.unitTemplate.unitName} recovered {foodItem.recoveryAmount} MP");
                    ApplyFoodBuff(foodItem.foodBuff, fedUnit);

                    return true;
                }
                else
                {
                    _cafeMenuUIController.HandleNotifications($"{fedUnit.unitTemplate.unitName} is already at full MP");
                    return false;
                }
            case ItemFoodType.FaithRecovery:
                if (fedUnit.unitFaithPoints >= 0)
                {
                    fedUnit.unitFaithPoints += (int)foodItem.recoveryAmount;
                    ApplyFoodBuff(foodItem.foodBuff, fedUnit);

                    return true;
                }
                else
                {
                    return false;
                }

            default:
                return false;
        }
    }

    private void ApplyFoodBuff(FoodBuff foodBuff, Unit fedUnit)
    {
        _cafeBuffController.ApplyFoodBuff(foodBuff, fedUnit);
    }


    public bool StatNotFull(float valueA, float valueB)
    {
        // Checks if the current stat value is less than max stat value.
        if (valueA < valueB)
        {
            return true;
        }
        else
        {
            // The Stat is already at max capacity!
            return false;
        }
    }
}