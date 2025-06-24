using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TomodachickTutorialHelperClass : MonoBehaviour
{
    public void StartTutorialConversation()
    {
        DialogueManager.StartConversation("Gameplay Fundamentals");
    }

}
