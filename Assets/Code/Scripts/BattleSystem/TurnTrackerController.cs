using UnityEngine;
using TMPro;

public class TurnTrackerController : MonoBehaviour
{
    public TextMeshProUGUI turnTrackerText;
    public int turnCounter;

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

    public delegate void IncreaseTurnCounter();
    public static event IncreaseTurnCounter OnIncreaseTurnCounter;
    public void SetTurnTrackerText(string turnText)
    {
        if (turnTrackerText != null)
        {
            turnTrackerText.text = turnText;
        }
        if (BattleManager.Instance.currentBattleType == BattleType.BossBattle)
        {
            OnIncreaseTurnCounter();
        }
        turnCounter++;
        //Change the name of the method
    }

    public void IncreaseLunarPhasesCounter()
    {
        // Send message to MoonPhaseController
    }
}
