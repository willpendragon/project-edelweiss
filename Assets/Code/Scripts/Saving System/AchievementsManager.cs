using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AchievementsManager : MonoBehaviour
{
    public List<Achievement> allAchievements; // Assign in editor or load at runtime
    private System.Random localRandom = new System.Random(); // Local random number generator

    public Achievement currentAchievement;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Always unsubscribe from events when the object is destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded: " + scene.name);
        if (scene.name == "battle_prototype")
        {
            var completedAchievements = allAchievements.Where(a => a.AchievementIsUnlocked()).ToList();

            if (completedAchievements.Count > 0)
            {
                // Tie-breaker: Random selection using localRandom
                Achievement achievementToTrigger = completedAchievements[localRandom.Next(completedAchievements.Count)];
                TriggerAchievementLogic(achievementToTrigger);
            }
        }
    }

    private void TriggerAchievementLogic(Achievement achievement)
    {
        // Define the chance of spawning the deity (e.g., 10% chance)
        // Beware, Magic Number
        double spawnChance = 0.5;

        // Check if there is a deity to spawn and make a probability roll
        if (achievement.spawnableDeity != null && localRandom.NextDouble() <= spawnChance)
        {
            Debug.Log("Spawning corresponding Deity");
            //Trigger the actual Deity Encounter logic here
            GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>().SetBattleType(BattleType.battleWithDeity);
            GameObject.FindGameObjectWithTag("DeitySpawner").GetComponent<DeitySpawner>().InitiateBattleWithDeity(achievement.spawnableDeity);

            Debug.Log("Setting battle with Deity");
            //Instantiate(achievement.spawnableDeity, Vector3.zero, Quaternion.identity); // Example spawn position and rotation
        }
        else
        {
            Debug.Log("Deity spawn chance not met or achievement already met.");
        }
    }
}

