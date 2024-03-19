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
// Rename to Stats Manager
{
    public int enemiesKilled;

    public List<DeityData> serializedCapturedDeitiesList = new List<DeityData>();

    public void Start()
    {
        LoadCharacterData();
        //ApplyDeityData();

    }
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
        //PlayerPrefs.SetInt("killedEnemies", deityAchievementsController.killedEnemies);
        PlayerPrefs.Save();
    }
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
        //deityAchievementsController.killedEnemies = killedEnemies;

        //LoadDeityData();
    }

    public void SaveDeityData(List<Deity> capturedDeities)
    {
        // Converts Deity MonoBehaviour instances to DeityData instances for serialization
        List<DeityData> deityDataList = capturedDeities.Select(deity => new DeityData { Id = deity.Id, specialAttackPower = deity.deitySpecialAttackPower }).ToList();

        // Wrap the list in the wrapper object
        DeityListWrapper wrapper = new DeityListWrapper { serializedCapturedDeitiesList = deityDataList };

        // Serialize the wrapper object to JSON
        string json = JsonUtility.ToJson(wrapper);
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);

    }
    public void ApplyDeityData()
    {
        GameManager.Instance.capturedDeities = LoadDeityData();
    }
    public List<Deity> LoadDeityData()
    {
        string filePath = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            DeityListWrapper wrapper = JsonUtility.FromJson<DeityListWrapper>(json);

            // Convert the DeityData instances back to Deity MonoBehaviour instances
            List<Deity> deities = new List<Deity>();
            foreach (DeityData data in wrapper.serializedCapturedDeitiesList)
            {
                //This line calls a method to create a new Empty Deity and populated with data from the saved file.
                //Deity deity = CreateDeity(data.Id, data.specialAttackPower);
                // Set any other data you have saved
                //deities.Add(deity);
            }

            return deities;
        }
        else
        {
            Debug.LogError("Save file not found in " + filePath);
            return null;
        }
    }

    public void IncrementEnemiesKilled()
    {
        enemiesKilled++;
        Debug.Log("Increasing Enemies Killed");
        AchievementsManager.Instance.CheckForAchievements(enemiesKilled);
        // Prepare the save data
        GameSaveData saveData = new GameSaveData { enemiesKilled = this.enemiesKilled };
        // Add other data to saveData as needed

        SaveStateManager.Instance.SaveGame(saveData);
    }
}
