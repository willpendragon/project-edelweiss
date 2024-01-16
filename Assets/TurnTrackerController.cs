using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurnTrackerController : MonoBehaviour
{
    public TextMeshProUGUI turnTrackerText;
    public void OnEnable()
    {
        TurnController.OnEnemyTurn += SetTurnTrackerText;
    }
    public void OnDisable()
    {
        TurnController.OnEnemyTurn -= SetTurnTrackerText;
    }

    public void SetTurnTrackerText(string enemyTurnText)
    {
        turnTrackerText.text = enemyTurnText;
    }


}
