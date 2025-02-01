using UnityEngine;
using TMPro;

public class DeityHuntingMenuController : MonoBehaviour
{
    [SerializeField] Achievement[] achievements;
    [SerializeField] GameObject achievementBlock;
    [SerializeField] Transform deityHuntingMenu;
    [SerializeField] GameStatsManager gameStatsManager;
    private void Start()
    {
        if (achievements.Length > 0)

        {
            foreach (var achievement in achievements)
            {
                GameObject newAchievementBlock = Instantiate(achievementBlock, deityHuntingMenu);
                PopulateAchievementBlock(achievement, newAchievementBlock);
            }
        }
    }
    private void PopulateAchievementBlock(Achievement achievement, GameObject newAchievementBlock)
    {
        string achievementName = achievement.achievementName;
        string achievementDescription = achievement.achievementDescription;
        string spawnableDeityName = achievement.spawnableDeity.GetComponent<Unit>().unitTemplate.unitName;
        string achievementRequirement = RetrieveRequirement(achievement);
        string achievementProgress = RetrieveAchievementProgress(achievement);

        AchievementBlockHelper currentAchievementBlockHelper = newAchievementBlock.GetComponent<AchievementBlockHelper>();
        currentAchievementBlockHelper.PopulateTexts(achievementName, achievementDescription, spawnableDeityName, achievementRequirement, achievementProgress);
    }
    private string RetrieveRequirement(Achievement achievement)
    {
        if (achievement is KillBasedAchievement killBasedAchievement)
        {
            string achievementRequirement = killBasedAchievement.requiredKills.ToString();
            return achievementRequirement;
        }
        else if (achievement is MoveBasedAchievement moveBasedAchievement)
        {
            string achievementRequirement = moveBasedAchievement.requiredUsedMoves.ToString();
            return achievementRequirement;
        }
        else
        {
            return null;
        }
    }
    private string RetrieveAchievementProgress(Achievement achievement)
    {
        if (achievement is KillBasedAchievement killBasedAchievement)
        {
            string achievementProgress = gameStatsManager.enemiesKilled.ToString();
            return achievementProgress;
        }
        else if (achievement is MoveBasedAchievement moveBasedAchievement)
        {
            string achievementProgress = gameStatsManager.timesSingleTargetSpellWasUsed.ToString();
            return achievementProgress;
        }
        else
        {
            return null;
        }
    }
}
