using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DialogueController : MonoBehaviour
{
    public DialogueSystemTrigger dialogueSystemTrigger;
    public void Start()
    {
        GameSaveData gameSaveData = SaveStateManager.LoadGame();
        int highestUnlockedLevel = gameSaveData.highestUnlockedLevel;
        if (highestUnlockedLevel > 1)
        {
            dialogueSystemTrigger.enabled = true;
        }
    }
}
