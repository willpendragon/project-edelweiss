using ExternPropertyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBackgroundUIManager : MonoBehaviour
{
    [SerializeField] private List<Sprite> _backgroundIllustrations;
    [SerializeField] private Image _backgroundSprite;
    [SerializeField] private CanvasGroup _dialogueBackgroundCanvasGroup;

    private void Start()
    {
        _dialogueBackgroundCanvasGroup.alpha = 0;
    }

    public void DisplayDialogueBackground()
    {
        // Set Background Image.
        _backgroundSprite.sprite = _backgroundIllustrations[0];
        // Show Dialogue Background.
        _dialogueBackgroundCanvasGroup.alpha = 1;
    }

    public void HideDialogueBackground()
    {
        // Clear Background Image.
        _backgroundSprite.sprite = null;
        _dialogueBackgroundCanvasGroup.alpha = 0;
    }
}