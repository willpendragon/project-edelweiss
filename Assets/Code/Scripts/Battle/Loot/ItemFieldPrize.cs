using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemFieldPrizeType
{
    attackPowerUp,
    magicPowerUp,
    PuzzleLevelKey
}
[CreateAssetMenu(fileName = "ItemFieldPrize", menuName = "Items/FieldPrize", order = 1)]

public class ItemFieldPrize : ScriptableObject

{
    public string itemFieldPrizeName;
    public string itemFieldPrizeDescription;
    public ItemFieldPrizeType itemFieldPrizeType;
    public float powerUpAmount;

    //public Sprite foodIcon;
    //public float itemFoodPrice;
}
