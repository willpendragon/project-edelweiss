using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AchievementUnlockedNotifications : DialogueUnlockedNotifications
{
    [SerializeField] private NotificationConfig _achievementNotificationConfig;
    [SerializeField] private DeityHuntingMenuController _deityHuntingMenuController;

    private void Start()
    {
        StartCoroutine(CheckForNewAchievementsCoroutine());
    }

    private IEnumerator CheckForNewAchievementsCoroutine()
    {
        // Wait 1 frame so UI components fully init
        yield return null;

        GameSaveData saveData = SaveStateManager.saveData;

        // Ensure list exists (failsafe for old saves)
        if (saveData.notifiedAchievements == null)
            saveData.notifiedAchievements = new List<string>();

        // Check our DeityHuntingMenuController for all currently unlocked achievements
        if (_deityHuntingMenuController != null && _deityHuntingMenuController.achievements != null)
        {
            foreach (Achievement achievement in _deityHuntingMenuController.achievements)
            {
                if (achievement.AchievementIsUnlocked())
                {
                    // If we haven't notified the player yet, queue it up!
                    if (!saveData.notifiedAchievements.Contains(achievement.achievementName))
                    {
                        ShowAchievementNotification(achievement);

                        // Mark as notified and save
                        saveData.notifiedAchievements.Add(achievement.achievementName);
                        SaveStateManager.SaveGame(saveData);
                    }
                }
            }
        }
    }

    private void ShowAchievementNotification(Achievement achievement)
    {
        OverworldUIManager.Instance.EventsUIManager.AddNotification(
            _achievementNotificationConfig,
            achievement.achievementName,
            achievement.GetDescription(),
            _achievementNotificationConfig.categoryName
        );
    }
}
