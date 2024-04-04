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
using UnityEditor.U2D.Animation;

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
        GameSaveData characterSaveData = SaveStateManager.saveData;

        foreach (var playerUnit in playerUnits)
        {
            Unit unitComponent = playerUnit.GetComponent<Unit>();
            CharacterData existingCharacterData = characterSaveData.characterData.Find(character => character.unitId == unitComponent.Id);

            if (existingCharacterData != null)
            {
                // Update existing character data
                existingCharacterData.unitHealthPoints = unitComponent.unitHealthPoints;
                // Update other stats as necessary
            }
            else
            {
                // Add new character data
                CharacterData newCharacterData = new CharacterData()
                {
                    unitId = unitComponent.Id,
                    unitHealthPoints = unitComponent.unitHealthPoints,
                    // Set other properties as necessary
                };
                characterSaveData.characterData.Add(newCharacterData);
            }
        }

        SaveStateManager.SaveGame(characterSaveData);
    }
    public void LoadCharacterData()
    {
        Debug.Log("Loading Player Character's Data");
        GameObject[] playerUnits = GameObject.FindGameObjectWithTag("BattleManager").GetComponentInChildren<TurnController>().playerUnitsOnBattlefield;
        if (playerUnits != null)
        {
            GameSaveData characterSaveData = SaveStateManager.saveData;
            foreach (var playerUnit in playerUnits)
            {
                Unit unitComponent = playerUnit.GetComponent<Unit>();
                CharacterData loadedCharacterData = characterSaveData.characterData.Find(character => character.unitId == unitComponent.Id);
                if (loadedCharacterData != null)
                {
                    unitComponent.unitHealthPoints = loadedCharacterData.unitHealthPoints;
                    // Set other stats as necessary
                    Debug.Log("Restoring Player Units HP and Mana");
                }
            }
        }
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
