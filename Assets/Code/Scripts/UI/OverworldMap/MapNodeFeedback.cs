using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MapNodeFeedback : MonoBehaviour
{
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.2f;

    private Vector3 originalScale;
    private Tween pulseTween;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnHoverEnter()
    {
        pulseTween?.Kill();

        transform.DOScale(originalScale * pulseScale, pulseDuration).SetEase(Ease.OutQuad);
    }

    public void OnHoverExit()
    {
        pulseTween?.Kill();
        transform.DOScale(originalScale, pulseDuration).SetEase(Ease.OutQuad);
    }
}
