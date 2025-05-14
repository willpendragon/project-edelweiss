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
        WipeGameSaveData();
        GameObject gameManagerInstance = GameObject.FindGameObjectWithTag("GameManager");
        Destroy(gameManagerInstance);
        StartCoroutine(LoadStartingScreen());
    }
    IEnumerator LoadStartingScreen()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("start_screen");
    }
    private void WipeGameSaveData()
    {
        GameSaveData gameSaveData = new GameSaveData();
        SaveStateManager.saveData.highestUnlockedLevel = 0;
        SaveStateManager.saveData.resourceData.warFunds = 0;
        SaveStateManager.SaveGame(gameSaveData);
        Debug.Log("Wiped Game Save Data");
    }
}
