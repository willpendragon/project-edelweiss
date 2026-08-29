using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemFieldPrizeType
{
    attackPowerUp,
    magicPowerUp,
    PuzzleLevelKey,
    MinibossKey,
    BossKey,
    Ingredient // Appended at end to preserve serialized int values of existing prizes
}
[CreateAssetMenu(fileName = "ItemFieldPrize", menuName = "Items/FieldPrize", order = 1)]

public class ItemFieldPrize : ScriptableObject

{
    public GameObject prizeGraphics;
    public string itemFieldPrizeName;
    public string itemFieldPrizeDescription;
    public string itemFieldPrizeLabel;

    public ItemFieldPrizeType itemFieldPrizeType;
    public float powerUpAmount;

    public Ingredient ingredientReward; // Hardcoded ingredient dropped when itemFieldPrizeType is Ingredient

    //public Sprite foodIcon;
    //public float itemFoodPrice;
}
