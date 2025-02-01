using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

[System.Serializable]
public struct CharacterDialogueData
{
    public Sprite portrait;                  // Character portrait
    [TextArea]                               // Makes it easier to edit multiline dialogues in the inspector
    public List<string> dialogues;           // List of possible dialogues for this character
}

public class CharacterDialogueManager : MonoBehaviour
{
    [Header("Character Related Data")]
    public List<CharacterDialogueData> characters;   // List of characters, each with portrait and dialogues

    [Header("UI Elements")]
    public Image portraitImage;              // UI Image component for the portrait
    public Text dialogueText;                // UI Text component for the dialogue
    public GameObject dialogueBox;           // Dialogue box parent GameObject for easy animations

    [Header("Animation Settings")]
    public float fadeDuration = 0.5f;
    public float scaleDuration = 0.5f;
    public Vector3 targetScale;
    public float displayDuration = 3f;       // Time for which the dialogue stays on screen

    private System.Random localRandom;       // Local random instance for random selection

    private void Start()
    {
        localRandom = new System.Random(System.DateTime.Now.Millisecond);
        ShowRandomDialogue();
    }

    public void ShowRandomDialogue()
    {
        targetScale = new Vector3(1.7f, 1.7f, 1.7f);
        // Randomly select a character
        CharacterDialogueData selectedCharacter = characters[localRandom.Next(characters.Count)];

        // Set the portrait and randomly select a dialogue from the selected character's dialogues
        portraitImage.sprite = selectedCharacter.portrait;
        dialogueText.text = selectedCharacter.dialogues[localRandom.Next(selectedCharacter.dialogues.Count)];

        // Set initial scale and alpha for animations
        dialogueBox.transform.localScale = Vector3.zero;
        portraitImage.color = new Color(1, 1, 1, 0); // Make portrait transparent

        // Animate the dialogue box to appear with a "juicy" effect
        dialogueBox.transform.DOScale(targetScale, scaleDuration).SetEase(Ease.OutBack);
        portraitImage.DOFade(1, fadeDuration).SetEase(Ease.InOutQuad); // Fade in the portrait

        // Hide the dialogue after a set duration
        Invoke("HideDialogue", displayDuration);
    }

    private void HideDialogue()
    {
        // Animate the dialogue box and portrait to disappear
        dialogueBox.transform.DOScale(Vector3.zero, scaleDuration).SetEase(Ease.InBack);
        portraitImage.DOFade(0, fadeDuration).SetEase(Ease.InOutQuad);
    }
}
