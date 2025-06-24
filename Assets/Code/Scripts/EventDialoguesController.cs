using PixelCrushers.DialogueSystem;
using UnityEngine;

public class EventDialoguesController : MonoBehaviour
{
    // Hard-coded, demo-only logic.
    // Unlocks deity-specific conversations based on fixed achievement thresholds.
    // Triggers only one conversation, with Anguana taking priority in case of a tie.
    // Uses Lua variables to prevent repeat conversations.
    // DialogueSystem state is saved with PersistentDataManager and stored via PlayerPrefs.
    // To be replaced in the future with a dynamic, data-driven system.

    [SerializeField] private KillBasedAchievement deityAnguanaAchievement;
    [SerializeField] private MoveBasedAchievement deityKingLaurinusAchievement;

    [SerializeField] private string deityAnguanaUnlockConversation = "Deity Anguana Unlock Conversation";
    [SerializeField] private string deityKingLaurinusUnlockConversation = "King Laurinus Unlock Conversation";

    private const string DialogueDataKey = "DialogueSystem_SaveData";

    private void Start()
    {
        LoadDialogueSystemData();

        if (deityAnguanaAchievement == null || deityKingLaurinusAchievement == null)
        {
            Debug.LogError("EventDialoguesController: One or more deity achievements are not assigned.");
            return;
        }

        if (SaveStateManager.saveData == null)
        {
            Debug.LogError("EventDialoguesController: Save data not loaded.");
            return;
        }

        StartDeityUnlockConversation();
    }

    private void StartDeityUnlockConversation()
    {
        var saveData = SaveStateManager.saveData;

        int currentKills = saveData.enemiesKilled;
        int currentSpellUses = saveData.timesSingleTargetSpellWasUsed;

        bool isAnguanaUnlocked = currentKills >= deityAnguanaAchievement.requiredKills;
        bool isLaurinusUnlocked = currentSpellUses >= deityKingLaurinusAchievement.requiredUsedMoves;

        if (isAnguanaUnlocked && isLaurinusUnlocked)
        {
            StartDeityAnguanaUnlockConversation(); // Anguana has priority
        }
        else if (isAnguanaUnlocked)
        {
            StartDeityAnguanaUnlockConversation();
        }
        else if (isLaurinusUnlocked)
        {
            StartDeityKingLaurinusUnlockConversation();
        }
    }

    private void StartDeityAnguanaUnlockConversation()
    {
        const string variableName = "DeityAnguanaUnlockConversationRead";

        if (!DialogueLua.GetVariable(variableName).asBool)
        {
            DialogueManager.StartConversation(deityAnguanaUnlockConversation);
            DialogueLua.SetVariable(variableName, true);
            SaveDialogueSystemData();
        }
    }

    private void StartDeityKingLaurinusUnlockConversation()
    {
        const string variableName = "DeityKingLaurinusUnlockConversationRead";

        if (!DialogueLua.GetVariable(variableName).asBool)
        {
            DialogueManager.StartConversation(deityKingLaurinusUnlockConversation);
            DialogueLua.SetVariable(variableName, true);
            SaveDialogueSystemData();
        }
    }

    private void SaveDialogueSystemData()
    {
        string saveData = PersistentDataManager.GetSaveData();
        PlayerPrefs.SetString(DialogueDataKey, saveData);
        PlayerPrefs.Save();
    }

    private void LoadDialogueSystemData()
    {
        if (PlayerPrefs.HasKey(DialogueDataKey))
        {
            string savedData = PlayerPrefs.GetString(DialogueDataKey);
            PersistentDataManager.ApplySaveData(savedData);
        }
    }
}
