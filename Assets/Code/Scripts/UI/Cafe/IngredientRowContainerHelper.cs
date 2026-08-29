using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientRowContainerHelper : MonoBehaviour
{
    [SerializeField] private Image _ingredientIcon;
    [SerializeField] private TextMeshProUGUI _ingredientNameText;

    public void UpdateIngredientDetails(Sprite iconSprite, string text)
    {
        if (_ingredientIcon != null)
            _ingredientIcon.sprite = iconSprite;

        if (_ingredientNameText != null)
            _ingredientNameText.text = text;
    }
}