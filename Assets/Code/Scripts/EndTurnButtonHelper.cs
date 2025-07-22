
using UnityEngine;
using UnityEngine.UI;
using static TurnController;


public class EndTurnButtonHelper : MonoBehaviour

{
    [SerializeField] private UnitSelectionController unitSelectionController;
    [SerializeField] private Button endTurnButton;
    public void EndTurnViaButton()
    {
        // It should be handled by a dedicated class for the End Turn Button.
        // Check if this is Player Turn and no Active Unit is in play.

        TurnController turnController = BattleManager.Instance?.GetComponent<TurnController>();

        if (turnController.currentTurn == Turn.EnemyTurn)
            return;
        GameObject[] playerUnitsOnBattlefield = turnController?.playerUnitsOnBattlefield;
        // I need to add new logic and feedback for stopping the Player Units.
        //foreach (var playerUnit in playerUnitsOnBattlefield)
        //{
        //    playerUnit?.GetComponent<UnitSelectionController>()?.StopUnitAction();
        //    playerUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitWaiting;
        //}
        Button endTurnButton = GameObject.FindGameObjectWithTag(Tags.END_TURN_BUTTON).GetComponent<Button>();
        endTurnButton.interactable = false;
        turnController.DecideTurn();
    }
}
