using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TurnController;

public class EndTurnButtonHelper : MonoBehaviour

{
    [SerializeField] private UnitSelectionController _unitSelectionController;
    [SerializeField] private Button _endTurnButton;
    [SerializeField] private TurnController _turnController;
    [SerializeField] private RectTransform _playerPartyProfilesContainer;
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
        DisablePlayerPartyProfiles();
    }
    private void DisablePlayerPartyProfiles()
    {
        PlayerPartyProfileHelper[] playersPartyProfiles = _playerPartyProfilesContainer.GetComponentsInChildren<PlayerPartyProfileHelper>();
        foreach (var profile in playersPartyProfiles)
        {
            profile.SetProfileAlpha(0.16f);
            profile.ResetAvailableMovesCounter();

        }
    }
}
