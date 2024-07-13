using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveStateManager
{
    //private static string saveFilePath
    //{
    //    get
    //    {
    //        return Path.Combine(Application.persistentDataPath, "achievements.json");
    //    }
    //}

    private static string saveFilePathTest
    {

        get
        {
            return Path.Combine(Application.persistentDataPath, "gameSaveData.json");
        }

    }

    private static GameSaveData gameSaveData = null;
    public static GameSaveData saveData

    {
        get
        {
            if (gameSaveData == null)
            {
                gameSaveData = LoadGame();
            }
            return gameSaveData;
        }
    }
    public static void SaveGame(GameSaveData saveData)
    {
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(saveFilePathTest, json);
        Debug.Log("Saving Game Data");
    }

    public static GameSaveData LoadGame()
    {
        // Check if the file exists
        if (File.Exists(saveFilePathTest))
        {
            // File exists, proceed to read it
            string json = File.ReadAllText(saveFilePathTest);
            GameSaveData loadedData = JsonConvert.DeserializeObject<GameSaveData>(json);
            return loadedData;
        }
        else
        {
            // File doesn't exist, return null or handle accordingly
            Debug.Log("Save file not found.");
            return new GameSaveData();
        }
    }
}
