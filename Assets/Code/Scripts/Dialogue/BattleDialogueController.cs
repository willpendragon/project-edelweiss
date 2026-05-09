using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PixelCrushers.DialogueSystem;

[System.Serializable]
public struct AchievementDialogueMapping
{
    [Tooltip("Must match the achievementId in the Achievement ScriptableObject")]
    public string achievementId;

    [Tooltip("The title of the conversation inside the PixelCrushers database")] [ConversationPopup]
    // Use this attribute to get a handy dropdown of your dialogues in the Inspector!
    public string conversationTitle;
}

public class BattleDialogueController : MonoBehaviour
{
    [SerializeField] private DialogueSystemController _dialogueSystemController;
    [SerializeField] private AchievementsManager _achievementsManager;

    [Header("Achievement Dialogues")]
    public List<AchievementDialogueMapping> achievementDialogues = new List<AchievementDialogueMapping>();

    private bool _deityBanterPlayed;

    private void Start()
    {
        CheckAndTriggerAchievementDialogues();
        if (_deityBanterPlayed)
            return;
        else
        {
            StartMapConversation();
        }
    }

    public void StartMapConversation()
    {
        MapData mapData = GridManager.Instance.currentMapData;
        if (mapData == null)
            return;
        if (mapData.BattleDialogue == null)
            return;
        DialogueManager.StartConversation(mapData.BattleDialogue);
    }

    private void CheckAndTriggerAchievementDialogues()
    {
        // Cancel out entirely if there isn't a deity spawned via an Achievement this battle!
        if (_achievementsManager == null) return;
        if (_achievementsManager.currentAchievement == null) return;

        // The Deity HAS spawned! Let's get the specific achievement!
        Achievement activeAchievement = _achievementsManager.currentAchievement;

        GameSaveData saveData = SaveStateManager.saveData;

        // Failsafe for older saves
        if (saveData.triggeredAchievementDialogues == null)
        {
            saveData.triggeredAchievementDialogues = new List<string>();
        }

        // We know it's unlocked and successfully spawned. Check if we've seen the dialogue yet.
        if (!saveData.triggeredAchievementDialogues.Contains(activeAchievement.achievementId))
        {
            var mapping = achievementDialogues.FirstOrDefault(m => m.achievementId == activeAchievement.achievementId);

            if (!string.IsNullOrEmpty(mapping.conversationTitle))
            {
                // Add to list and immediately save to prevent repeating
                saveData.triggeredAchievementDialogues.Add(activeAchievement.achievementId);
                SaveStateManager.SaveGame(saveData);

                // Trigger the PixelCrushers Dialogue, which natively blocks player action.
                DialogueManager.StartConversation(mapping.conversationTitle);
                _deityBanterPlayed = true;
            }
        }
    }
}