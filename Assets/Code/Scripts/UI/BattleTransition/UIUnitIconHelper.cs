using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UIUnitIconHelper : MonoBehaviour
{
    [SerializeField] private Image _deityIcon;
    public Image UnitUIIcon;
    [SerializeField] TextMeshProUGUI _buffTextDisplay;

    public void Start()
    {
        _deityIcon.DOFade(0, 0.1f);
    }
    public void ShowDeityIcon(Sprite deitySprite)
    {
        // Add DoTween effect
        _deityIcon.sprite = deitySprite;
        _deityIcon.DOFade(1, 1f);
    }

    public void SetBuffDisplay(FoodBuff.FoodBuffType type, float value)
    {
        _buffTextDisplay.text = $"{type}+ { value}";
        Debug.Log($"Displaying {type} buff with total value: {value}");
    }
}
