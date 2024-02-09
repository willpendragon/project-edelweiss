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
        EnemyTurnManager.OnPlayerTurn += SetTurnTrackerText;
        Deity.OnPlayerTurn += SetTurnTrackerText;
    }
    public void OnDisable()
    {
        TurnController.OnEnemyTurn -= SetTurnTrackerText;
        EnemyTurnManager.OnPlayerTurn -= SetTurnTrackerText;
        Deity.OnPlayerTurn += SetTurnTrackerText;
    }
    public void SetTurnTrackerText(string turnText)
    {
        turnTrackerText.text = turnText;
    }


}
