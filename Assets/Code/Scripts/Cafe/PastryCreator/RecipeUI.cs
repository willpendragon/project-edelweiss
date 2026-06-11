using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RecipeUI : MonoBehaviour
{
    [SerializeField] private Image resultIcon;
    [SerializeField] private TextMeshProUGUI resultName;
    [SerializeField] private TextMeshProUGUI requirementText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private Button craftButton;

    private Recipe currentRecipe;
    private PastryCreator creatorRef;

    public void Setup(Recipe recipe, bool canCraft)
    {
        currentRecipe = recipe;

        resultIcon.sprite = recipe.resultItem.foodIcon;
        resultName.text = recipe.resultItem.itemFoodName;
        typeText.text = recipe.resultItem.itemFoodType.ToString();

        requirementText.text = "";
        foreach (var ing in recipe.ingredients)
        {
            requirementText.text += $"{ing.ingredient.ingredientName} x{ing.quantity}\n";
        }

        craftButton.interactable = canCraft;
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(OnCraftPressed);

        // Optional: gray out the background if not craftable
        GetComponent<Image>().color = canCraft ? Color.white : new Color(1, 1, 1, 0.5f);
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
