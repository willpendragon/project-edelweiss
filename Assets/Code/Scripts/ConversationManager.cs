using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance;

    [SerializeField]
    public List<ConversationData> conversations = new List<ConversationData>();

    private void Awake()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (Instance == null)
        {
            Instance = this;
            if (currentSceneName == "battle_prototype")
            {
                DontDestroyOnLoad(gameObject); // Ensures the manager persists across scenes
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadUnlockedConversation();
    }

    public void UnlockRandomConversation()
    {
        List<ConversationData> lockedConvos = conversations.FindAll(convo => !convo.isUnlocked);
        if (lockedConvos.Count > 0)
        {
            int randomIndex = Random.Range(0, lockedConvos.Count);
            lockedConvos[randomIndex].isUnlocked = true;
            Debug.Log($"Unlocked convo {lockedConvos[randomIndex].conversationID}");
            SaveUnlockedConversation();
        }
        else
        {
            Debug.Log("All conversations are already unlocked.");
        }
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
    }

    public void SaveUnlockedConversation()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;

        gameSaveData.unlockedConversations.Clear();

        foreach (var conversation in ConversationManager.Instance.conversations)
        {
            gameSaveData.unlockedConversations.Add(new ConversationData(conversation.conversationID, conversation.isUnlocked, conversation.isRead));
        }
        SaveStateManager.SaveGame(gameSaveData);
    }

    public void StartConversationByID(string conversationID)
    {
        var conversation = conversations.Find(c => c.conversationID == conversationID && c.isUnlocked);
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
    public void MarkDialogueAsRead(int id)
    {
        List<ConversationData> unreadConvos = conversations.FindAll(convo => !convo.isRead);
        if (unreadConvos.Count > 0)
        {
            unreadConvos[id].isRead = true;
            Debug.Log($"Read convo {unreadConvos[id]}");
            SaveUnlockedConversation();
        }
        else
        {
            Debug.Log("All conversations are already read.");
        }
        //conversations[id].isRead = true;
        Debug.Log(id);
    }
}
