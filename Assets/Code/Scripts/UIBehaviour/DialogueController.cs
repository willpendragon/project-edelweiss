using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DialogueController : MonoBehaviour
{
    //public DialogueSystemTrigger dialogueSystemTrigger;
    public bool dialoguesAreActive;
    public void Start()
    {
        if (dialoguesAreActive)
        {
            GameSaveData gameSaveData = SaveStateManager.LoadGame();
            int highestUnlockedLevel = gameSaveData.highestUnlockedLevel;

            switch (highestUnlockedLevel)
            {
                case 1:
                    DialogueManager.StartConversation("After the Battle");
                    break;
                case 2:
                    DialogueManager.StartConversation("Test_Conversation");
                    break;

                case 4:
                    //dialogueSystemTrigger.enabled = true;
                    break;

                // You can add more cases here if needed.

                default:
                    // Handle cases where highestUnlockedLevel is not 2 or 4, if necessary.
                    break;
            }
        }

    }
}
