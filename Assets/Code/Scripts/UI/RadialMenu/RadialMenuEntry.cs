using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenuEntry : MonoBehaviour
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

    public void SetLabel(string labelText)
    {
        _actionLabel.text = labelText;
    }

    public void FireAction()
    {
        FindAnyObjectByType<CursorController>().ChangeCursorMode(actionType);
    }

}
