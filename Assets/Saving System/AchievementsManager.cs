using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementsManager : MonoBehaviour

{
    public static AchievementsManager Instance;
    public List<Achievement> allAchievements; // Assign in editor or load at runtime
    private Dictionary<string, bool> achievementStatus = new Dictionary<string, bool>();
    //public Achievement currentAchievement;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        LoadAchievements();
    }


    private void Start()
    {
        CheckAndTriggerAchievements();
    }

    // This method marks the achievement as completed at the end of the battle.
    public void CompleteAchievement(string achievementName)
    {
        if (!achievementStatus.ContainsKey(achievementName))
        {
            achievementStatus[achievementName] = true;
        }
        SaveStateManager.Instance.SaveAchievements(achievementStatus);
    }

    public bool IsAchievementCompleted(string achievementName)
    {
        return achievementStatus.TryGetValue(achievementName, out bool completed) && completed;
    }

    private void LoadAchievements()
    {
        // Load achievements from SaveGameStateManager
        achievementStatus = SaveStateManager.Instance.LoadAchievements();
    }

    // This method looKs up the achievements list and executes a Deity-triggering logic based on the completed Achievement.


    private void CheckAndTriggerAchievements()
    {
        List<Achievement> completedAchievements = allAchievements.Where(a => IsAchievementCompleted(a.achievementName) && a.spawnableDeity != null).ToList();

        if (completedAchievements.Count > 0)
        {
            // Tie-breaker: Random selection
            Achievement achievementToTrigger = completedAchievements[UnityEngine.Random.Range(0, completedAchievements.Count)];
            TriggerAchievementLogic(achievementToTrigger);
        }
    }

    public void CheckForAchievements(int value)
    {
        foreach (var achievement in allAchievements)
        //Looks up in the Saved List of Achievements 
        {
            //Validates only if the Achievement was not completed and the requirement is met 
            if (!IsAchievementCompleted(achievement.achievementName) && value >= achievement.requirement)
            {
                //Calls the method to mark the achievement as completed
                CompleteAchievement(achievement.achievementName);
                // Optionally trigger an event or notification about the achievement completion
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
            //Instantiate(achievement.spawnableDeity, Vector3.zero, Quaternion.identity); // Example spawn position and rotation
        }
    }

    //If Deity was Captured, mark currentAchievement as OBSOLETE;

}

