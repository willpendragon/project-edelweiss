using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsSaver : MonoBehaviour
{
    public float defaultPlayerHealth = 400;
    public void Start()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.healthPoints = RestorePlayerHealth();
    }
    public void OnEnable()
    {
        //Listens to the signals sent at the end of the Battle after Enemy defeat
        BattleManager.OnBattleEnd += SavePlayerHealth;
    }
    public void OnDisable()
    {
        BattleManager.OnBattleEnd -= SavePlayerHealth;
    }
    public void SavePlayerHealth(float finalPlayerHealth)
    {
        //This method saves the Player's Health at the end of the battle.
        PlayerPrefs.SetFloat("PlayerHealth", finalPlayerHealth);
        PlayerPrefs.Save();
        Debug.Log("Saving Player Health");
    }
    float RestorePlayerHealth()
    {
        //This method restores the Player's health at the start of the battle.
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
}
