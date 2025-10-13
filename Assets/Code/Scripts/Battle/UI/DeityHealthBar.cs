using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeityHealthBar : MonoBehaviour
{
    [SerializeField] CanvasGroup _deityHealthBar;
    [SerializeField] CanvasGroup _deityEnmityBar;
    public void HideHealthBar()
    {
        _deityHealthBar.alpha = 0;
    }

    public void HideEnmityBar()
    {
        _deityEnmityBar.alpha = 0;
    }

    public void HideBars()
    {
        HideHealthBar();
        HideEnmityBar();
    }
}
