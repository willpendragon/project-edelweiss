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

        if (conversationData.unlockedConversations == null ||
            conversationData.unlockedConversations.Count == 0 ||
            conversationData.convoIndex == 0)
        {
            return;
        }

        int convoIndex = conversationData.convoIndex;
        int lastConvoNumber = conversationData.lastConvoNumber;

        int targetIndex = UnityEngine.Mathf.Clamp(convoIndex - 1, 0, conversationData.unlockedConversations.Count - 1);

        int currentConvoNumber = conversationData.unlockedConversations[targetIndex].conversationNumber;

        if (lastConvoNumber == currentConvoNumber)
        {
            return;
        }

        string dialogueTitle = conversationData.unlockedConversations[targetIndex].conversationID;
        ShowNotification(dialogueTitle);
        SaveLastConvoNumber();
    }

    public void SaveLastConvoNumber()
    {
        GameSaveData conversationData = SaveStateManager.saveData;
        if (conversationData.unlockedConversations == null || conversationData.unlockedConversations.Count == 0)
        {
            return;
        }
        int targetIndex = conversationData.convoIndex - 1;
        int maxIndex = conversationData.unlockedConversations.Count - 1;
        targetIndex = UnityEngine.Mathf.Clamp(targetIndex, 0, maxIndex);
        conversationData.lastConvoNumber = conversationData.unlockedConversations[targetIndex].conversationNumber;
        Debug.Log($"Saved Last Convo Number: {conversationData.lastConvoNumber}");
    }

    private void ShowNotification(string title)
    {
        OverworldUIManager.Instance.EventsUIManager.AddNotification(_dialogueNotificationConfig, title, "test", _dialogueNotificationConfig.categoryName);
    }
}
