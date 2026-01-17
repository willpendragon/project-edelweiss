using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueBackgroundHelper : MonoBehaviour
{
    [SerializeField] private DialogueBackgroundUIManager _dialogueBackgroundManager;
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cache the Dialogue Background Manager.
        _dialogueBackgroundManager = FindAnyObjectByType<DialogueBackgroundUIManager>();
    }

    public void DisplayBackgroundWrapper()
    {
        _dialogueBackgroundManager.DisplayDialogueBackground();
    }

    public void HideBackgroundWrapper()
    {
        _dialogueBackgroundManager.HideDialogueBackground();
    }
}
