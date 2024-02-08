using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerStatsSaver : MonoBehaviour
{
    public void Start()
    {
        LoadCharacterData();
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
        PlayerPrefs.SetInt("killedEnemies", deityAchievementsController.killedEnemies);
        PlayerPrefs.Save();
    }

    public void LoadCharacterData()
    {
        Debug.Log("Loading Player Character's Data");
        GameObject[] playerUnits = GameObject.FindGameObjectWithTag("BattleManager").GetComponentInChildren<TurnController>().playerUnitsOnBattlefield;
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
        var deityAchievementsController = GameObject.FindGameObjectWithTag("DeityAchievementsController").GetComponent<DeityAchievementsController>();
        int killedEnemies = PlayerPrefs.GetInt("killedEnemies", 0);
        deityAchievementsController.killedEnemies = killedEnemies;
    }
}
