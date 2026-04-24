using UnityEngine;

public class CharacterUnlockedNotifications : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The NotificationConfig asset defining the aesthetic for character unlocks.")]
    [SerializeField] private NotificationConfig _characterUnlockConfig;

    private void OnEnable()
    {
        CharacterUnlockManager.OnCharacterUnlocked += ShowUnlockNotification;
    }

    private void OnDisable()
    {
        CharacterUnlockManager.OnCharacterUnlocked -= ShowUnlockNotification;
    }

    private void ShowUnlockNotification(Unit unlockedCharacter)
    {
        if (OverworldUIManager.Instance != null && OverworldUIManager.Instance.EventsUIManager != null)
        {
            // Fallback to exactly prefab name if for some reason unitTemplate is missing
            string characterName = unlockedCharacter.unitTemplate != null 
                ? unlockedCharacter.unitTemplate.unitName 
                : unlockedCharacter.name;
            
            OverworldUIManager.Instance.EventsUIManager.AddNotification(
                _characterUnlockConfig,
                "New Ally Unlocked!",
                characterName
            );
        }
    }
}