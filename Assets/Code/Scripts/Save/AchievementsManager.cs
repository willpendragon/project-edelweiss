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
        if (achievement == null || achievement.spawnableDeity == null)
            return false;
            
        string deityName = achievement.spawnableDeity.GetComponent<Unit>().unitTemplate.unitName;
        string deityId = achievement.spawnableDeity.GetComponent<Deity>().Id;
        GameSaveData saveData = SaveStateManager.saveData;
        
        // Check if deity is killed
        bool isKilled = _deitySpawner.DeityIsKilled(deityName);
        
        // Check if deity is linked to any player
        bool isLinked = saveData.unitsLinkedToDeities.ContainsValue(deityId);
        
        // Check if deity is captured but unassigned
        bool isUnassigned = saveData.unassignedCapturedDeities.Contains(deityId);
        
        // Deity is available if not killed, linked, or unassigned, and passes spawn chance
        bool isAvailable = !isKilled && !isLinked && !isUnassigned;
        
        return isAvailable && localRandom.NextDouble() <= deitySpawnChance;
    }
    public BattleTypeController.BattleType TriggerDeityAchievementLogic()
    {
        // BLOCK: Do not upgrade to a capture battle if the Roaming Deity is forcing a clash
        if (BattleTypeController.isForcedRoamingDeity)
        {
            Debug.Log("Forced Roaming Deity Encounter. Bypassing Achievement logic.");
            return BattleTypeController.BattleType.RegularBattle;
        }

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