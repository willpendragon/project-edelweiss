using UnityEngine;

public enum IngredientType
{
    Common,
    Medium,
    Rare
}

[CreateAssetMenu(fileName = "Ingredient", menuName = "Items/Ingredient", order = 0)]

public class Ingredient : ScriptableObject
{
    public string ingredientName;
    public string ingredientDescription;
    public IngredientType ingredientType;
    public Sprite ingredientIcon;
    public float baseValue;
}