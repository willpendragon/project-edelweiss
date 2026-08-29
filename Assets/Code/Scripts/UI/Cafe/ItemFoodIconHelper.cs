using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemFoodIconHelper : MonoBehaviour
{
    [SerializeField] private GameObject _orderButton;
    // References to text fields
    [SerializeField] private TextMeshProUGUI _foodItemName;
    [SerializeField] private TextMeshProUGUI _recoveryAmount;
    [SerializeField] private TextMeshProUGUI _foodItemPrice;
    [SerializeField] private TextMeshProUGUI _foodItemAvailability;
    [SerializeField] private TextMeshProUGUI _foodDescription;

    // Reference to Pastry Icon
    [SerializeField] private Image _pastryImage;
    [SerializeField] private Image _pastryBackground;

    private Vector2 _originalImagePosition;
    private Vector2 _originalAnchorMin;
    private Vector2 _originalAnchorMax;
    private Vector2 _originalPivot;

    public void Start()
    {
        Color color = _pastryBackground.color;
        color.a = 1;
        _pastryBackground.color = color;
    }
    public void ActivateOrderButton(bool flag)
    {
        if (flag == true)
        {
            _orderButton.SetActive(true);
        }
        else
        {
            _orderButton.SetActive(false);
        }
    }

    public void PopulateItemFoodDetails(
        string foodItemName,
        string recoveryAmount,
        string foodItemPrice,
        string foodItemAvailability,
        string foodDescription)
    {
        _foodItemName.text = foodItemName;
        _recoveryAmount.text = recoveryAmount;
        _foodItemPrice.text = foodItemPrice;
        _foodItemAvailability.text = foodItemAvailability;
        _foodDescription.text = foodDescription;
    }

    public void UpdatePastryIcon(Sprite pastryIcon)
    {
        _pastryImage.sprite = pastryIcon;
    }

    public void CenterPastryIcon()
    {
        RectTransform pastryRT = _pastryImage.rectTransform;

        _originalImagePosition = pastryRT.anchoredPosition;
        _originalAnchorMin = pastryRT.anchorMin;
        _originalAnchorMax = pastryRT.anchorMax;
        _originalPivot = pastryRT.pivot;

        // Hook into this method to center the Pastry icon when dragging it.
        pastryRT.anchorMin = new Vector2(0.5f, 0.5f);
        pastryRT.anchorMax = new Vector2(0.5f, 0.5f);
        pastryRT.pivot = new Vector2(0.5f, 0.5f);
        pastryRT.anchoredPosition = Vector2.zero;
        // !! This doesn't work actually, check how to change position of RectTransforms correctly (also remember to restore position)
        // after the item is returned on the "shelf".

        Color color = _pastryBackground.color;
        color.a = 0;
        _pastryBackground.color = color;
    }

    public void RestorePastryIcon()
    {
        RectTransform pastryRT = _pastryImage.rectTransform;

        pastryRT.anchorMin = _originalAnchorMin;
        pastryRT.anchorMax = _originalAnchorMax;
        pastryRT.pivot = _originalPivot;
        pastryRT.anchoredPosition = _originalImagePosition;

        Color color = _pastryBackground.color;
        color.a = 1;
        _pastryBackground.color = color;
    }

    public void ShowTexts()
    {
        _foodItemName.enabled = true;
        _recoveryAmount.enabled = true;
        _foodItemPrice.enabled = true;
        _foodItemAvailability.enabled = true;
        _foodDescription.enabled = true;
    }

    public void HideTexts()
    {
        _foodItemName.enabled = false;
        _recoveryAmount.enabled = false;
        _foodItemPrice.enabled = false;
        _foodItemAvailability.enabled = false;
        _foodDescription.enabled = false;
    }
}
