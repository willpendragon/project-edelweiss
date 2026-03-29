using TMPro;
using UnityEngine;

public class DialogueUnlockedNotifications : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _title;
    [SerializeField] RectTransform _popupTransform;
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] NotificationConfig _dialogueNotificationConfig;
    private void OnEnable()
    {
        ConversationManager.OnDialogueUnlocked += ShowNotification;
    }
    private void OnDisable()
    {
        ConversationManager.OnDialogueUnlocked -= ShowNotification;
    }

    private void Start()
    {
        GameSaveData conversationData = SaveStateManager.saveData;
        // Load Convo Current Index and Latest Convo Number
        int convoIndex = conversationData.convoIndex;
        int lastConvoNumber = conversationData.lastConvoNumber; // It was previously saved via this class.
        if (convoIndex == lastConvoNumber) // Blocks execution if no new convo unlocks. 
            return;
        string dialogueTitle = conversationData.unlockedConversations[convoIndex - 1].conversationID;
        ShowNotification(dialogueTitle);
        SaveLastConvoNumber();
    }

    public void SaveLastConvoNumber()
    {
        GameSaveData conversationData = SaveStateManager.saveData;
        int convoIndex = conversationData.convoIndex;
        conversationData.lastConvoNumber = conversationData.unlockedConversations[convoIndex].conversationNumber;
    }

    private void ShowNotification(string title)
    {
        OverworldUIManager.Instance.EventsUIManager.AddNotification(_dialogueNotificationConfig, title, "test");
    }
}
