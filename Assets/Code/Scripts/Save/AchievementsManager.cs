using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementsManager : MonoBehaviour
{
    public List<Achievement> allAchievements; // Assign in editor or load at runtime
    private System.Random localRandom = new System.Random(); // Local random number generator

    public Achievement currentAchievement;
    public DeitySpawner _deitySpawner;

    private Achievement SelectAchievementUnlocked()
    {
        var completedAchievements = allAchievements.Where(a => a.AchievementIsUnlocked()).ToList();

        if (completedAchievements.Count > 0)
        {
            // Tie-breaker: Random selection using localRandom
            Achievement achievementToTrigger = completedAchievements[localRandom.Next(completedAchievements.Count)];
            return achievementToTrigger;
        }
        else
        {
            return null;
        }
    }

    private bool CanTriggerDeityBattle(Achievement achievement, double deitySpawnChance)
    {
        return achievement != null
            && achievement.spawnableDeity != null
            && !_deitySpawner.DeityIsKilled(achievement.spawnableDeity.GetComponent<Unit>().unitTemplate.unitName)
            && localRandom.NextDouble() <= deitySpawnChance;
    }
    public BattleTypeController.BattleType TriggerDeityAchievementLogic()
    {
        Achievement achievement = SelectAchievementUnlocked();
        double spawnChance = 0.5;

        // Check if there is a deity to spawn and make a probability roll
        if (CanTriggerDeityBattle(achievement, spawnChance))
        {
            // NEW: store the specific achievement that spawned!
            currentAchievement = achievement;
            
            GameObject.FindGameObjectWithTag("DeitySpawner").GetComponent<DeitySpawner>().InitiateBattleWithDeity(achievement.spawnableDeity);
            Debug.Log("Started Battle with Deity");
            return BattleTypeController.BattleType.BattleWithDeity;
        }
        else
        {
            currentAchievement = null;
            Debug.Log("Deity spawn chance not met or Achievement already met.");
            return BattleTypeController.BattleType.RegularBattle;
        }
    }
}