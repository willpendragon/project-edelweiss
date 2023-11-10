using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsSaver : MonoBehaviour
{
    public float defaultPlayerHealth;
    public float defaultCoins;
    public float defaultPlayerExperiencePoints;
    public void Awake()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.healthPoints = RestorePlayerHealth();
        player.coins = RestoreCoins();
        player.playerExperiencePoints = RestorePlayerExperiencePoints();
    }
    public void OnEnable()
    {
        //Listens to the signals sent at the end of the Battle after Enemy defeat
        BattleManager.OnSavePlayerHealth += SavePlayerHealth;
        BattleManager.OnSavePlayerCoinsReward += SaveCoins;
        BattleManager.OnSavePlayerExperienceReward += SavePlayerExperience;
    }
    public void OnDisable()
    {
        BattleManager.OnSavePlayerHealth -= SavePlayerHealth;
        BattleManager.OnSavePlayerCoinsReward -= SaveCoins;
        BattleManager.OnSavePlayerExperienceReward -= SavePlayerExperience;
    }
    public void SavePlayerHealth(float finalPlayerHealth)
    {
        //This method saves the Player's Health.
        PlayerPrefs.SetFloat("PlayerHealth", finalPlayerHealth);
        PlayerPrefs.Save();
        Debug.Log("Saving Player Health");
    }
    public void SaveCoins(float gainedCoins)
    {
        //This method saves the Coins resource.
        PlayerPrefs.SetFloat("Coins", gainedCoins);
        PlayerPrefs.Save();
        Debug.Log("Saving Coins");
    }
    public void SavePlayerExperience(float gainedPlayerExperience)
    {
        //This method saves the Player's Experience Points.
        PlayerPrefs.SetFloat("PlayerExperiencePoints", gainedPlayerExperience);
        PlayerPrefs.Save();
        Debug.Log("Saving Experience Points");
    }
    float RestorePlayerHealth()
    {
        //This method restores the Player's Health.
        Debug.Log("Restoring Player Health");
        if (PlayerPrefs.HasKey("PlayerHealth"))
        {
            return PlayerPrefs.GetFloat("PlayerHealth");
        }
        else
        {
            return defaultPlayerHealth;
        }
    }
    float RestoreCoins()
    {
        //This method restores the Player's coins.
        Debug.Log("Restoring Coins");
        if (PlayerPrefs.HasKey("Coins"))
        {
            return PlayerPrefs.GetFloat("Coins");
        }
        else
        {
            return defaultCoins;
        }
    }
    float RestorePlayerExperiencePoints()
    {
        //This method restores the Player's Experience Points.
        Debug.Log("Restoring Experience Points");
        if (PlayerPrefs.HasKey("PlayerExperiencePoints"))
        {
            return PlayerPrefs.GetFloat("PlayerExperiencePoints");
        }
        else
        {
            return defaultPlayerExperiencePoints;
        }
    }
}
