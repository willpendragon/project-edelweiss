using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitIconHelper : MonoBehaviour
{
    [SerializeField] private Image _deityIcon;
    public Image UnitUIIcon;

    public void ShowDeityIcon(Sprite deitySprite)
    {
        // Add DoTween effect
        _deityIcon.sprite = deitySprite;
    }

    public void SetBuffDisplay(FoodBuff.FoodBuffType type, float value)
    {
        Debug.Log($"Displaying {type} buff with total value: {value}");
    }
}
