using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PixelCrushers.DialogueSystem;

[System.Serializable]
public struct AchievementDialogueMapping
{
    [Tooltip("Must match the achievementId in the Achievement ScriptableObject")]
    public string achievementId;
    
    [Tooltip("The title of the conversation inside the PixelCrushers database")]
    [ConversationPopup] // Use this attribute to get a handy dropdown of your dialogues in the Inspector!
    public string conversationTitle;
}

public class BattleDialogueController : MonoBehaviour
{
    [SerializeField] private DialogueSystemController _dialogueSystemController;
    [SerializeField] private AchievementsManager _achievementsManager; // Reference the manager dynamically via inspector

    [Header("Achievement Dialogues")]
    public List<AchievementDialogueMapping> achievementDialogues = new List<AchievementDialogueMapping>();

    private void Start()
    {
        CheckAndTriggerAchievementDialogues();
    }

    private void CheckAndTriggerAchievementDialogues()
    {
        if (_achievementsManager == null || _achievementsManager.allAchievements == null) return;

        // Hook into existing GameSaveData
        GameSaveData saveData = SaveStateManager.saveData;

        // Failsafe for older saves
        if (saveData.triggeredAchievementDialogues == null)
        {
            saveData.triggeredAchievementDialogues = new List<string>();
        }

        foreach (var achievement in _achievementsManager.allAchievements)
        {
            // If the achievement is completed AND the dialogue hasn't been shown yet
            if (achievement.AchievementIsUnlocked() && !saveData.triggeredAchievementDialogues.Contains(achievement.achievementId))
            {
                // Check if we defined a specific dialogue mapping in the Inspector for this ID
                var mapping = achievementDialogues.FirstOrDefault(m => m.achievementId == achievement.achievementId);
                
                if (!string.IsNullOrEmpty(mapping.conversationTitle))
                {
                    // Add to lists and immediately save to prevent repeating!
                    saveData.triggeredAchievementDialogues.Add(achievement.achievementId);
                    SaveStateManager.SaveGame(saveData);

                    // Trigger the PixelCrushers Dialogue, which natively blocks player action.
                    DialogueManager.StartConversation(mapping.conversationTitle);
                    
                    // We break out of the loop because we only want ONE dialogue popping up right now.
                    // If multiple unlocked at once, the next one will queue up on the next battle!
                    break;
                }
            }
        }
    }
}
