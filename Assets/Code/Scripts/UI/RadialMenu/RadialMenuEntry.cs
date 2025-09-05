using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class RadialMenuEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public enum ActionType
    {
        Move,
        Melee,
        Spell,
        Trap,
        Pray,
        Summon,
        Crystal,
        Run
    }


    [SerializeField] private TextMeshProUGUI _actionLabel;
    public Image icon;

    public ActionType actionType;
    public int priority;

    public delegate void NoPointsNotification(string message);
    public static event NoPointsNotification OnPointsDepleted;

    public void SetLabel(string labelText)
    {
        _actionLabel.text = labelText;
    }

    public void FireAction()
    {
        var activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit.unitOpportunityPoints <= 0)
        {
            OnPointsDepleted("Not enough Energy...");
        }
        else
        {
            FindAnyObjectByType<CursorController>().ChangeCursorMode(actionType);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(1.5f, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1f, 0.5f);
    }
}
