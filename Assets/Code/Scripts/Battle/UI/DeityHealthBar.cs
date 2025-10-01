using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeityHealthBar : MonoBehaviour
{
    [SerializeField] CanvasGroup _deityHealthBar;
    public void HideHealthBar()
    {
        _deityHealthBar.alpha = 0;
    }
}
