using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameController : MonoBehaviour
{
    public SceneLoader sceneLoader;

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
