using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemFoodType
{
    HPRecovery,
    ManaRecovery,
    FaithRecovery,
    DeityTribute
}

// Buff Alignment - prototype names.


[CreateAssetMenu(fileName = "ItemFood", menuName = "Items/Food", order = 1)]

public class ItemFood : ScriptableObject
{
    public ItemFoodType itemFoodType;
    public FoodBuff foodBuff;

    public string itemFoodName;
    public string itemFoodDescription;
    public float itemFoodPrice;
    public float recoveryAmount;
    public Sprite foodIcon;
}
