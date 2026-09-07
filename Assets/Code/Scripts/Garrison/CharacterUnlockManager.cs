using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUnlockManager : MonoBehaviour
{
    public static event Action<Unit> OnCharacterUnlocked;

    [Header("Configuration")]
    [Tooltip("List of all potential character unlocks configured as ScriptableObjects.")]
    public List<CharacterUnlockCondition> unlockConditions = new List<CharacterUnlockCondition>();

    private void Start()
    {
        CheckForNewUnlocks();
    }

    public void CheckForNewUnlocks()
    {
        GameSaveData saveData = SaveStateManager.saveData;
        if (saveData == null) return;

        bool hasNewUnlocks = false;

        foreach (CharacterUnlockCondition condition in unlockConditions)
        {
            if (condition.IsConditionMet(saveData))
            {
                if (condition.characterToUnlock == null) continue;
                string newUnitId = condition.characterToUnlock.Id;

                if (!saveData.activePartyUnitIds.Contains(newUnitId) && !saveData.availablePartyUnitIds.Contains(newUnitId))
                {
                    Debug.Log($"[CharacterUnlockManager] Unlocking: {condition.characterToUnlock.name}");

                    hasNewUnlocks = true;

                    if (GameManager.Instance != null && !GameManager.Instance.playerPartyMembers.Exists(u => u.Id == newUnitId))
                    {
                        GameManager.Instance.playerPartyMembers.Add(condition.characterToUnlock);
                    }

                    // --- NEW: Broadcast the unlock to UI/Dialogue controllers ---
                    OnCharacterUnlocked?.Invoke(condition.characterToUnlock);
                }
            }
        }

        if (hasNewUnlocks)
        {
            if (GameManager.Instance != null) GameManager.Instance.SyncPartyListsToSaveData();
            SaveStateManager.SaveGame(saveData);
            Debug.Log("Saved new character unlocks to file.");
        }
    }
}