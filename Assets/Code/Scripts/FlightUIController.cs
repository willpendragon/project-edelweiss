using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FlightUIController : MonoBehaviour
{
    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public GameObject runButtonPrefab;
    public Transform spellMenuContainer;
    private string buttonName;

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        if (scene.name == "battle_prototype")
        {
            spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
        }
    }

    public void AddRunButton()
    {
        //Instantiates the Move Button
        GameObject moveButtonInstance = Instantiate(runButtonPrefab, spellMenuContainer);
        Button currentMoveButton = moveButtonInstance.GetComponent<Button>();
        currentMoveButton.onClick.AddListener(() => RunFromBattle());
    }

    public void RunFromBattle()
    {
        TurnController.Instance.RunFromBattle();
    }
}
