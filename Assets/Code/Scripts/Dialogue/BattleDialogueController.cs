using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Linq;

[System.Serializable]
public struct AchievementDialogueMapping
{
    [Tooltip("Must match the achievementId in the Achievement ScriptableObject")]
    public string achievementId;
    [Tooltip("The title of the conversation inside the PixelCrushers database")]
    [ConversationPopup] // Optional generic Dialogue System attribute that creates a dropdown
    public string conversationTitle;
}

public class BattleDialogueController : MonoBehaviour
{
    [SerializeField] private DialogueSystemController _dialogueSystemController;
    [SerializeField] private AchievementsManager _achievementsManager;

    [Header("Achievement Dialogues")]
    public List<AchievementDialogueMapping> achievementDialogues = new List<AchievementDialogueMapping>();

    private void Start()
    {
        CheckAndTriggerAchievementDialogues();
    }

    private void CheckAndTriggerAchievementDialogues()
    {
        if (_achievementsManager == null || _achievementsManager.allAchievements == null) return;

        GameSaveData saveData = SaveStateManager.saveData;

        // Failsafe for older saves
        if (saveData.triggeredAchievementDialogues == null)
        {
            saveData.triggeredAchievementDialogues = new List<string>();
        }

        foreach (var achievement in _achievementsManager.allAchievements)
        {
            // If completed AND the dialogue hasn't been shown yet
            if (achievement.AchievementIsUnlocked() && !saveData.triggeredAchievementDialogues.Contains(achievement.achievementId))
            {
                // Check if we defined a dialogue for this specific achievement in the Inspector
                var mapping = achievementDialogues.FirstOrDefault(m => m.achievementId == achievement.achievementId);
                
                if (!string.IsNullOrEmpty(mapping.conversationTitle))
                {
                    // Remember that we've played it so it never plays again!
                    saveData.triggeredAchievementDialogues.Add(achievement.achievementId);
                    SaveStateManager.SaveGame(saveData);

                    // Trigger the PixelCrushers Dialogue
                    DialogueManager.StartConversation(mapping.conversationTitle);
                    
                    // We break out of the loop because we only want ONE dialogue popping up at the start of battle.
                    // If multiple unlocked at once, the next one will queue up on the next battle!
                    break;
                }
            }
        }
    }
}
