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
        if (allAchievements == null)
        {
            Debug.LogError("[AchievementsManager] allAchievements list is not assigned.");
            return null;
        }

        var nullEntryCount = allAchievements.Count(a => a == null);
        if (nullEntryCount > 0)
        {
            Debug.LogError($"[AchievementsManager] allAchievements contains {nullEntryCount} null/missing entry(ies).");
        }

        var completedAchievements = allAchievements.Where(a => a != null && a.AchievementIsUnlocked()).ToList();

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

        // Pinpoint exactly which reference is missing instead of letting a bad prefab crash battle setup.
        Unit spawnableUnit = achievement.spawnableDeity.GetComponent<Unit>();
        if (spawnableUnit == null || spawnableUnit.unitTemplate == null)
        {
            Debug.LogError($"[AchievementsManager] Achievement '{achievement.name}' spawnableDeity '{achievement.spawnableDeity.name}' is missing a Unit component or unitTemplate.");
            return false;
        }

        Deity spawnableDeityComponent = achievement.spawnableDeity.GetComponent<Deity>();
        if (spawnableDeityComponent == null)
        {
            Debug.LogError($"[AchievementsManager] Achievement '{achievement.name}' spawnableDeity '{achievement.spawnableDeity.name}' is missing a Deity component.");
            return false;
        }

        if (_deitySpawner == null)
        {
            Debug.LogError("[AchievementsManager] _deitySpawner reference is not assigned.");
            return false;
        }

        string deityName = spawnableUnit.unitTemplate.unitName;
        string deityId = spawnableDeityComponent.Id;
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
        try
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

                GameObject deitySpawnerObj = GameObject.FindGameObjectWithTag("DeitySpawner");
                if (deitySpawnerObj == null)
                {
                    Debug.LogError("[AchievementsManager] No GameObject tagged 'DeitySpawner' found in scene.");
                    currentAchievement = null;
                    return BattleTypeController.BattleType.RegularBattle;
                }

                DeitySpawner deitySpawnerComponent = deitySpawnerObj.GetComponent<DeitySpawner>();
                if (deitySpawnerComponent == null)
                {
                    Debug.LogError("[AchievementsManager] GameObject tagged 'DeitySpawner' has no DeitySpawner component.");
                    currentAchievement = null;
                    return BattleTypeController.BattleType.RegularBattle;
                }

                deitySpawnerComponent.InitiateBattleWithDeity(achievement.spawnableDeity);
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
        catch (System.Exception ex)
        {
            // Falling back here instead of letting this throw keeps BattleSelection() reaching OnBattleTypeInitialized,
            // which EnemyPoolController/DeityBattleUIController depend on to spawn enemies and show the Deity UI at all.
            Debug.LogError($"[AchievementsManager] TriggerDeityAchievementLogic failed, falling back to RegularBattle: {ex}");
            currentAchievement = null;
            return BattleTypeController.BattleType.RegularBattle;
        }
    }
}