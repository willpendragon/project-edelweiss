using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PastryFeedbackController : MonoBehaviour
{
    [SerializeField] private GameObject _iconObject;
    [SerializeField] private RectTransform _iconSpawnPoint;
    [SerializeField] private RectTransform _destinationCanvas;
    [SerializeField] private RectTransform _iconDestination;
    [SerializeField] private float _movementDuration = 1f;
    [SerializeField] private float _fadeDuration = 2.5f;
    [SerializeField] TextMeshProUGUI _textNotification;

    public Recipe testRecipe;

    private void TestCookingFeedback()
    {
        var foodIcon = testRecipe.resultItem.foodIcon;
        // Spawn a GameObject Icon
        var iconObjectInstance = Instantiate(_iconObject, _destinationCanvas);
        iconObjectInstance.GetComponent<RectTransform>().anchoredPosition = _iconSpawnPoint.anchoredPosition;
        // Add the icon as a sprite
        iconObjectInstance.GetComponent<Image>().sprite = foodIcon;

        // Move and fade the sprite across the menu
        iconObjectInstance.GetComponent<RectTransform>().DOAnchorPos(_iconDestination.anchoredPosition, _movementDuration).SetEase(Ease.Linear);
        iconObjectInstance.GetComponent<Image>().DOFade(0, _fadeDuration);

        _textNotification.text = $"Baked {testRecipe.resultItem.itemFoodName}!";
        StartCoroutine("ResetText");
    }

    public void CookingFeedback(Recipe recipe)
    {
        var foodIcon = recipe.resultItem.foodIcon;
        // Spawn a GameObject Icon
        var iconObjectInstance = Instantiate(_iconObject, _destinationCanvas);
        iconObjectInstance.GetComponent<RectTransform>().anchoredPosition = _iconSpawnPoint.anchoredPosition;
        // Add the icon as a sprite
        iconObjectInstance.GetComponent<Image>().sprite = foodIcon;

        // DoTween sequence (move and fade the sprite across the menu)

        Sequence foodIconSequence = DOTween.Sequence();

        foodIconSequence.Append(iconObjectInstance.GetComponent<RectTransform>().DOAnchorPos(_iconDestination.anchoredPosition, _movementDuration).SetEase(Ease.Linear));
        foodIconSequence.Append(iconObjectInstance.GetComponent<Image>().DOFade(0, _fadeDuration));
        _textNotification.text = $"Baked {testRecipe.resultItem.itemFoodName}!";
        StartCoroutine("ResetText");
    }
    IEnumerator ResetText()
    {
        yield return new WaitForSeconds(1);
        _textNotification.text = "";
    }
}
