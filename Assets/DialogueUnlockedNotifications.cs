using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueUnlockedNotifications : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _title;
    private void OnEnable()
    {
        ConversationManager.OnDialogueUnlocked += ShowNotification;
    }
    private void OnDisable()
    {
        ConversationManager.OnDialogueUnlocked -= ShowNotification;
    }

    private void ShowNotification(string title)
    {
        _title.text = title;
    }
}
