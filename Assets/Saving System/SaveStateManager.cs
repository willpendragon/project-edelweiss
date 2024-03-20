using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveStateManager : MonoBehaviour
{
    public static SaveStateManager Instance;

    private string saveFilePath;
    private string saveFilePathTest;


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

        saveFilePath = Path.Combine(Application.persistentDataPath, "achievements.json");
        saveFilePathTest = Path.Combine(Application.persistentDataPath, "gameSaveData.json");
    }
    public void SaveGame(GameSaveData saveData)
    {
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(saveFilePathTest, json);
        Debug.Log("Saving Game Data");
    }

    public GameSaveData LoadGame()
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
            return null;
        }
    }

    public void SaveAchievements(Dictionary<string, bool> achievements)
    {
        string json = JsonConvert.SerializeObject(achievements, Formatting.Indented);
        File.WriteAllText(saveFilePath, json);
    }

    public Dictionary<string, bool> LoadAchievements()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            return JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);
        }
        return new Dictionary<string, bool>();
    }
}
