using UnityEngine;
using UnityEngine.UI;
using static TurnController;

public class EndTurnButtonHelper : MonoBehaviour

{
    [SerializeField] private UnitSelectionController _unitSelectionController;
    [SerializeField] private Button _endTurnButton;
    [SerializeField] private TurnController _turnController;
    public void EndTurnViaButton()
    {
        _turnController.DecideTurn();

        if (_turnController.currentTurn == Turn.EnemyTurn)
            return;
        if (_unitSelectionController == null || _endTurnButton == null)
            return;

        _unitSelectionController.StopPlayerParty();
        _endTurnButton.interactable = false;

        // Clear Tiles Color
        GridManager.Instance.ClearTileColors();
    }
}
