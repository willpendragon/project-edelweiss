using UnityEngine;
using UnityEngine.UI;
using static TurnController;

public class EndTurnButtonHelper : MonoBehaviour

{
    [SerializeField] private UnitSelectionController _unitSelectionController;
    [SerializeField] private Button _endTurnButton;
    public void EndTurnViaButton()
    {
        TurnController turnController = BattleManager.Instance?.GetComponent<TurnController>();
        turnController.DecideTurn();

        if (turnController.currentTurn == Turn.EnemyTurn)
            return;
        if (_unitSelectionController == null || _endTurnButton == null)
            return;

        _unitSelectionController.StopPlayerParty();
        _endTurnButton.interactable = false;

        // Clear Tiles Color
        GridManager.Instance.ClearTileColors();
    }
}
