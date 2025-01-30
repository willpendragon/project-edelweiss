using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class DeityHuntingMenuController : MonoBehaviour
{
    [SerializeField] Achievement[] achievements;
    [SerializeField] GameObject achievementBlock;
    [SerializeField] Transform deityHuntingMenu;
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

        AchievementBlockHelper currentAchievementBlockHelper = newAchievementBlock.GetComponent<AchievementBlockHelper>();
        currentAchievementBlockHelper.PopulateTexts(achievementName, achievementDescription, spawnableDeityName, achievementRequirement);
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
}
