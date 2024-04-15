using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AchievementsManager : MonoBehaviour
{
    public List<Achievement> allAchievements; // Assign in editor or load at runtime

    private System.Random localRandom = new System.Random(); // Local random number generator

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
                // Tie-breaker: Random selection using System.Random
                Achievement achievementToTrigger = completedAchievements[localRandom.Next(completedAchievements.Count)];
                TriggerAchievementLogic(achievementToTrigger);
            }
        }
    }

    private void TriggerAchievementLogic(Achievement achievement)
    {
        if (achievement.spawnableDeity != null) //To add: && achievement is not obsolete
        {
            Debug.Log("Spawning corresponding Deity");
            //Trigger the actual Deity Encounter logic here
            GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>().SetBattleType(BattleType.battleWithDeity);
            GameObject.FindGameObjectWithTag("DeitySpawner").GetComponent<DeitySpawner>().InitiateBattleWithDeity(achievement.spawnableDeity);
            Debug.Log("Setting battle with Deity");
            //Instantiate(achievement.spawnableDeity, Vector3.zero, Quaternion.identity); // Example spawn position and rotation
        }
    }
}

