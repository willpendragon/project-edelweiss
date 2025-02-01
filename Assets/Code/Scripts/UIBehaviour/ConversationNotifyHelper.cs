using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationNotifyHelper : MonoBehaviour
{
    public ConversationManager conversationManager; // Link in Inspector

    private void Start()
    {
        conversationManager = ConversationManager.Instance;
    }

    // Called when a conversation ends (via Dialogue System Events)
    public void OnConversationEnd()
    {
        // Ensure there is an active conversation
        if (DialogueManager.CurrentConversationState != null)
        {
            // Retrieve the current dialogue ID
            int currentConversationID = DialogueManager.lastConversationID;
            var conversation = DialogueManager.masterDatabase.GetConversation(currentConversationID);
            string conversationTitle = conversation.Title;
            // Mark the dialogue as read in the ConversationManager
            conversationManager.MarkDialogueAsRead(conversationTitle);
            Debug.Log("Retrieved" + conversationTitle);
        }
    }
}
