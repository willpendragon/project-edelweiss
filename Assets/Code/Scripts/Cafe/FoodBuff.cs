using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemFood", menuName = "Items/FoodBuff", order = 2)]

public class FoodBuff : ScriptableObject
{
    public enum FoodBuffType
    {
        Attack,
        Defense,
    }

    public enum FoodBuffAlignment
    {
        Red,
        Blue,
        Yellow
    }

    public FoodBuffType foodBuffType;
    public float foodBuffAmount;
    public int foodBuffDurationDays;
    public FoodBuffAlignment alignment;
}
