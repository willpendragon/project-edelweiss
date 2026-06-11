using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class StartGameController : MonoBehaviour
{
    public SceneLoader sceneLoader;
    public Button loadGameButton;
    private string saveFilePath;

    private void Start()
    {
        ActivateLoadGameButton();
    }
    private void ActivateLoadGameButton()
    {
        if (CheckSaveFile())
        {
            loadGameButton.interactable = true;
        }
        else
        {
            loadGameButton.interactable = false;
        }
    }

    private bool CheckSaveFile()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "gameSaveData.json");
        if (File.Exists(saveFilePath))
        {
            GameSaveData gameSaveData = SaveStateManager.LoadGame();
            
            // Validate that we have actual progress to return to rather than a fresh save file.
            if (gameSaveData.highestUnlockedLevel > 0 || gameSaveData.currentNodeId > 0 || gameSaveData.clearedNodesId.Count > 0)
            {
                return true;
            }
        }
        
        return false;
    }

    public void NewGame()
    {
        //Go to Overworld Map
        ResetGame();
        sceneLoader.ChangeScene();
    }
    public void LoadGame()
    {
        GameSaveData gameSaveData = SaveStateManager.LoadGame();
        int highestUnlockedLevel = gameSaveData.highestUnlockedLevel;
        sceneLoader.ChangeScene();
    }
    public void ResetGame()
    {
        GameSaveData gameSaveData = new GameSaveData();
        SaveStateManager.SaveGame(gameSaveData);
    }
}
