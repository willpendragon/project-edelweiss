using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using System;

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
        // These conversations are connected to completing an achievement (in the demo, unlocking deities).
        // So they will fire typically when you kill all of the bounties required by a Deity AND the Deity apperance roll is successful.
        // (AKA, a Deity capture Battle).
        // Be mindful that this logic will always fire before any other overrides.
        // So if you place a dialogue at, say, Node 2, and the deity trigger unlocks, you won't see that dialogue.
        // This could be fine, but could also mean the Player will miss it.
        // Since it takes some times to farm enemy kills and trigger achievements, it's fine to force a dialogue at 1-2 still.

        CheckAndTriggerAchievementDialogues();
        if (_deityBanterPlayed)
            return;
        else
        {
            // If no achievements have been completed, follow the typical logic.
            StartMapConversation();
        }
    }

    public void StartMapConversation()
    {
        // This will fire only if the user has setup an override inside the Level scriptable object config.
        // Just be mindful that map types in Level SO config don't correspond to the generated nodes map type (for randomization purposes).
        // So in the demo, if you wish to make a dialogue appear in correspondance of a specific node in the flow, you need to
        // force the dialogue triggering by setting a convoTitle in the Level SO config. It requires a bit of back and forth
        // in-engine to have the dialogue flow make sense, since you want to see if the node configuration belongs to one type.
        // So always have the map generated at runtime, check the level number and type. Then go back to the level SO config and only then
        // Add the convo title. Of course generic banter dialogues can sit almost anywhere, but something really specific, you have to
        // Double-check manually at the moment. Sorry about that (-_-);
        if (GameManager.Instance.currentConversationTitle != null)
        {
            string conversationTitle = GameManager.Instance.currentConversationTitle;
            DialogueManager.StartConversation(conversationTitle);
        }
        else
        {
            // Reads from MapData and starts the conversation specified in the corresponding map config that has been created in the Edelweiss Map Editor.
            // Much more straightforward! Just add a title in the MapData config and it will be played in fight.
            // This is meant for maps that you're supposed to encounter only once (Boss Fight).
            // For other cases (example, puzzle maps that are rehashed with different enemies), just read from Level SO -> GameManager.
            // This is only a temporary solution. Ideally, all maps should be unique and have (or not!) their own dialogue.
            // Still, I want to be able to fire dialogue in specific moments in the Node flow.

            StartConversationFromMapData();
        }
    }

    private void StartConversationFromMapData()
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