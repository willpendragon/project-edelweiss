using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedingHelper : MonoBehaviour
{

    public void ResetPartyFeedingStats()
    {
        ResetPartyFullness();
        // Clear FoodSlots
        ClearFoodSlots();
    }

    private void ClearFoodSlots()
    {
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.eatenPastriesHistory.Clear();
    }

    public void ResetPartyFullness()
    {
        List<Unit> playerParty = GameManager.Instance.playerPartyMembersInstances;

        foreach (var unit in playerParty)
        {
            unit.unitOccupiedFoodSlots = 0;
        }
    }
}
