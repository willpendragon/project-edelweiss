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
        GameSaveData freshData = new GameSaveData();
        SaveStateManager.saveData = freshData;
        SaveStateManager.SaveGame(freshData);
        Debug.Log("Game data reset to blank slate.");
    }
}
