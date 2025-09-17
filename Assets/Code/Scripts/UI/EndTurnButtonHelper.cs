using UnityEngine;
using UnityEngine.UI;
using static TurnController;

public class EndTurnButtonHelper : MonoBehaviour

{
    [SerializeField] private UnitSelectionController _unitSelectionController;
    [SerializeField] private Button _endTurnButton;
    public void EndTurnViaButton()
    {
        // It should be handled by a dedicated class for the End Turn Button.
        // Check if this is Player Turn and no Active Unit is in play.
        // Logical error: if the sequence stops when the Enemy Turn is still active, I won't be able to end the turn again.
        // I need to reinstate the Player Turn before firing this sequence.

        TurnController turnController = BattleManager.Instance?.GetComponent<TurnController>();
        turnController.DecideTurn();

        if (turnController.currentTurn == Turn.EnemyTurn)
            return;
        if (_unitSelectionController == null || _endTurnButton == null)
            return;
        // I need to add new logic and feedback for stopping the Player Units.
        _unitSelectionController.StopPlayerParty();
        _endTurnButton.interactable = false;

        // Clear Enemy Selection Tiles
        var unitSelectionController = FindAnyObjectByType<UnitSelectionController>();
        unitSelectionController.ResetEnemyReachableTiles();
    }
}
