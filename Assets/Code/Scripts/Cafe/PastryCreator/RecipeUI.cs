using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeUI : MonoBehaviour
{
    [SerializeField] private Image resultIcon;
    [SerializeField] private TextMeshProUGUI resultName;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private Button craftButton;

    [Header("Ingredients Container")]
    [SerializeField] private Transform ingredientsParent; // Transform holding the ingredient rows (e.g., with a Vertical Layout Group)
    [SerializeField] private GameObject ingredientRowPrefab; // Prefab with IngredientRowContainerHelper attached

    private Recipe currentRecipe;
    private PastryCreator creatorRef;

    public void Setup(Recipe recipe, bool canCraft)
    {
        currentRecipe = recipe;

        resultIcon.sprite = recipe.resultItem.foodIcon;
        resultName.text = recipe.resultItem.itemFoodName;
        typeText.text = $"{recipe.resultItem.itemFoodType} + {recipe.resultItem.recoveryAmount}";

        // Clear existing ingredient icons/rows
        foreach (Transform child in ingredientsParent)
        {
            Destroy(child.gameObject);
        }

        // Spawn a row prefab for each ingredient
        foreach (var ing in recipe.ingredients)
        {
            GameObject row = Instantiate(ingredientRowPrefab, ingredientsParent);
            
            if (row.TryGetComponent<IngredientRowContainerHelper>(out var rowHelper))
            {
                string text = $"{ing.ingredient.ingredientName} x{ing.quantity}";
                rowHelper.UpdateIngredientDetails(ing.ingredient.ingredientIcon, text);
            }
        }

        craftButton.interactable = canCraft;
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(OnCraftPressed);

        GetComponent<Image>().color = canCraft ? Color.white : new Color(1f, 1f, 1f, 0.5f);
    }

    public void SetCreator(PastryCreator creator)
    {
        creatorRef = creator;
    }

    private void OnCraftPressed()
    {
        if (creatorRef != null && currentRecipe != null)
        {
            creatorRef.TryCraft(currentRecipe);
        }
    }
}