using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationNotifyHelper : MonoBehaviour
{
    public ConversationManager conversationManager; // Link in Inspector

    // Called when a conversation ends (via Dialogue System Events)
    public void OnConversationEnd()
    {
        // Ensure there is an active conversation
        if (DialogueManager.CurrentConversationState != null)
        {
            // Retrieve the current dialogue ID
            int currentConversationID = DialogueManager.lastConversationID;

            // Mark the dialogue as read in the ConversationManager
            conversationManager.MarkDialogueAsRead(currentConversationID);
            Debug.Log("Retrieved" + currentConversationID);
        }
    }
}
