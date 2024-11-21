using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class NewRunHelper : MonoBehaviour


{
    private string saveFilePath;
    public void StartNewGameRun()
    {
        saveFilePath = Application.persistentDataPath + "/gameSaveData.json";
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Deleted Saved Game Data");
        }
        else
        {
            Debug.LogWarning("No Saved Game Data found.");
        }
        SceneManager.LoadScene("start_screen");
    }
}
