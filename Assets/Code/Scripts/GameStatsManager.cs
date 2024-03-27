using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Newtonsoft.Json;
using System.IO;
using static UnityEditor.Progress;
using System.Linq;
using JetBrains.Annotations;

public class GameStatsManager : MonoBehaviour

{
    public int enemiesKilled;

    private CharacterData characterData;

    public void Start()
    {
        LoadCharacterData();
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData != null)
        {
            enemiesKilled = gameSaveData.enemiesKilled;
            Debug.Log("Loading the number of killed Enemies");
        }

    }

    // 22032024 To be changed to JSON-based system and moved to Save State Manager class
    public void SaveCharacterData()
    {
        GameObject[] playerUnits = GameObject.FindGameObjectWithTag("BattleManager").GetComponentInChildren<TurnController>().playerUnitsOnBattlefield;
        foreach (var playerUnit in playerUnits)
        {
            Unit unit = playerUnit.GetComponent<Unit>();
            string baseKey = unit.unitTemplate.unitName;
            PlayerPrefs.SetFloat(baseKey + "_XP", unit.unitExperiencePoints);
            PlayerPrefs.SetFloat(baseKey + "_Coins", unit.unitCoins);
            PlayerPrefs.SetFloat(baseKey + "_HealthPoints", unit.unitHealthPoints);
        }
        var deityAchievementsController = GameObject.FindGameObjectWithTag("DeityAchievementsController").GetComponent<DeityAchievementsController>();
        PlayerPrefs.Save();
    }

    // 22032024 To be changed to JSON-based system and moved to Save State Manager class
    public void LoadCharacterData()
    {
        Debug.Log("Loading Player Character's Data");
        GameObject[] playerUnits = GameObject.FindGameObjectWithTag("BattleManager").GetComponentInChildren<TurnController>().playerUnitsOnBattlefield;
        if (playerUnits != null)
        {
            foreach (var playerUnit in playerUnits)
            {
                Unit unit = playerUnit.GetComponent<Unit>();
                string baseKey = unit.unitTemplate.unitName;

                // Assuming default values if keys don't exist. Adjust as necessary.
                float xp = PlayerPrefs.GetFloat(baseKey + "_XP", 0); // Default to 0 if not found
                float coins = PlayerPrefs.GetFloat(baseKey + "_Coins", 0); // Default to 0 if not found
                float healthPoints = PlayerPrefs.GetFloat(baseKey + "_HealthPoints", unit.unitTemplate.unitMaxHealthPoints); // Default to max health points if not found

                unit.unitExperiencePoints = xp;
                unit.unitCoins = coins; // Assuming unitCoins is the correct field for storing coins
                unit.unitHealthPoints = healthPoints;
            }
        }
        var deityAchievementsController = GameObject.FindGameObjectWithTag("DeityAchievementsController").GetComponent<DeityAchievementsController>();
        int killedEnemies = PlayerPrefs.GetInt("killedEnemies", 0);
    }

    public void SaveEnemiesKilled()
    {
        Debug.Log("Increasing Enemies Killed");

        // Prepare the save data
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.enemiesKilled = enemiesKilled;

        SaveStateManager.SaveGame(saveData);
    }
}
