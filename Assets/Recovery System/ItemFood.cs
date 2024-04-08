using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemFoodType
{
    HPRecovery,
    manaRecovery
}
[CreateAssetMenu(fileName = "ItemFood", menuName = "Items/Food", order = 1)]

public class ItemFood : ScriptableObject

{
    public string itemFoodName;
    public float recoveryAmount;
    public ItemFoodType itemFoodType;
    public Sprite foodIcon;
}
