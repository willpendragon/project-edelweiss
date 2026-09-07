using PixelCrushers.DialogueSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// This file is in the Dialogue System folder, currently in .gitignore.
// The dialogue unlock system and the dialogues in Cafe depends on this class.
public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance;

    [SerializeField] public List<ConversationData> conversations = new List<ConversationData>();

    private const string OVERWORLD_MAP = "overworld_map";
    [SerializeField] private string _lastUnlockedConversation;
    [SerializeField] private int _convoIndex = 0;

    public delegate void DialogueUnlocked(string title);
    public static event DialogueUnlocked OnDialogueUnlocked;

    private void OnEnable()
    {
        BattleFlowController.OnBattleEndDialogueUnlock += UnlockNewConversation;
    }

    private void OnDisable()
    {
        BattleFlowController.OnBattleEndDialogueUnlock -= UnlockNewConversation;
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (Instance == null)
        {
            Instance = this;
            //if (currentSceneName == "battle_prototype")
            //{
            //    DontDestroyOnLoad(gameObject);
            //}
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //GameSaveData conversationData = SaveStateManager.saveData;
        //var lastConvoIndex = conversationData.convoIndex;

        //if (scene.name == OVERWORLD_MAP)
        //{
        //    OnDialogueUnlocked(_lastUnlockedConversation);
        //}
    }

    private void Start()
    {
        LoadUnlockedConversation();
    }

    public void UnlockNewConversation()
    {

        if (_convoIndex >= conversations.Count)
        {
            Debug.Log("All convos already unlocked");
            return;
        }
        conversations[_convoIndex].isUnlocked = true;
        Debug.Log($"Unlocked convo {conversations[_convoIndex].conversationID}");
        _lastUnlockedConversation = $"{conversations[_convoIndex].conversationID}";

        IncreaseIndex();
        SaveUnlockedConversation();
    }

    private void IncreaseIndex()
    {
        //if (_convoIndex == 0)
        //    return;
        _convoIndex++;
    }

    public bool IsConversationUnlocked(string id)
    {
        return conversations.Exists(convo => convo.conversationID == id && convo.isUnlocked);
    }

    public void LoadUnlockedConversation()
    {
        GameSaveData conversationData = SaveStateManager.saveData;

        foreach (var savedConvo in conversationData.unlockedConversations)
        {
            var conversation = ConversationManager.Instance.conversations.Find(c => c.conversationID == savedConvo.conversationID);
            if (conversation != null)
            {
                conversation.isUnlocked = savedConvo.isUnlocked;
            }
        }
        _convoIndex = conversationData.convoIndex;
    }

    public void SaveUnlockedConversation()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;

        gameSaveData.unlockedConversations.Clear();

        foreach (var conversation in ConversationManager.Instance.conversations)
        {
            gameSaveData.unlockedConversations.Add(new ConversationData(conversation.conversationID, conversation.isUnlocked, conversation.isRead, conversation.conversationNumber));
        }

        gameSaveData.convoIndex = _convoIndex; // Save the current Index

        SaveStateManager.SaveGame(gameSaveData);
    }

    public void StartConversationByID(string conversationID)
    {
        GameSaveData conversationData = SaveStateManager.saveData;

        var conversation = conversationData.unlockedConversations.Find(c => c.conversationID == conversationID && c.isUnlocked);
        if (conversation != null)
        {
            // Start the conversation using the numeric ID.
            DialogueManager.StartConversation(conversationID);
        }
        else
        {
            Debug.LogWarning($"Conversation with ID {conversationID} is either not found or not unlocked.");
        }
    }
    // Method to mark a dialogue as read
    public void MarkDialogueAsRead(string conversationTitle)
    {
        foreach (var conversation in conversations)
        {
            if (conversation.conversationID == conversationTitle)
            {
                conversation.isRead = true;
            }
        }
        GameSaveData conversationData = SaveStateManager.saveData;
        foreach (var conversation in conversationData.unlockedConversations)
        {
            if (conversation.conversationID == conversationTitle && conversation.isRead == false)
            {
                conversation.isRead = true;
                Debug.Log(conversationTitle + "marked as Read");
            }
        }
        SaveStateManager.SaveGame(conversationData);
    }
}
