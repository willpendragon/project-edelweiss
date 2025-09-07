using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System;

public class MovesCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _remainingMoves;
    [SerializeField] GameObject _activePlayerUnit;
    [SerializeField] RectTransform _rectTransform;
    [Header("Tween Settings")]
    public float pulseScale = 1.2f;
    public float pulseDuration = 0.5f;
    public Ease easeType = Ease.InOutSine;
    private Tween pulseTween;

    private void Update()
    {
        _activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        if (_activePlayerUnit != null)
        {
            int op = _activePlayerUnit.GetComponent<Unit>().unitOpportunityPoints;
            int maxOp = _activePlayerUnit.GetComponent<Unit>().unitTemplate.unitOpportunityPoints;
            _remainingMoves.text = $"{op} / {maxOp}";
        }
    }
}