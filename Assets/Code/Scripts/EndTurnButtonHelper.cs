using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TurnController;
using UnityEngine.UI;

public class EndTurnButtonHelper : MonoBehaviour
{
    public void EndTurnViaButton()
    {
        // It should be handled by a dedicated class for the End Turn Button.
        // Check if it's Player Turn and no Active Unit is in play.

        TurnController turnController = BattleManager.Instance?.GetComponent<TurnController>();

        if (turnController.currentTurn == Turn.playerTurn)
        {
            GameObject[] playerUnitsOnBattlefield = turnController?.playerUnitsOnBattlefield;
            foreach (var playerUnit in playerUnitsOnBattlefield)
            {
                playerUnit?.GetComponent<UnitSelectionController>()?.StopUnitAction();
                playerUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitWaiting;
            }

            Button endTurnButton = GameObject.FindGameObjectWithTag(Tags.END_TURN_BUTTON).GetComponent<Button>();
            endTurnButton.interactable = false;
            turnController.SwapTurns();
        }
    }

}
