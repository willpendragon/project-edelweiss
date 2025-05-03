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
    private Transform spellMenuContainer;
    private string buttonName;

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "battle_prototype" || scene.name == "boss_battle_prototype")
        {
            GameObject spellMenuGameObject = GameObject.FindGameObjectWithTag("MovesPanel");
            if (spellMenuGameObject != null)
            {
                spellMenuContainer = spellMenuGameObject.transform;
            }
        }
    }

    public void AddRunButton()
    {
        //Instantiates the Move Button
        GameObject moveButtonInstance = Instantiate(runButtonPrefab, spellMenuContainer);
        Button currentMoveButton = moveButtonInstance.GetComponent<Button>();
        currentMoveButton.onClick.AddListener(() => RunFromBattle());
        currentMoveButton.onClick.AddListener(() => GridManager.Instance.ClearPath());
    }

    public void RunFromBattle()
    {
        TurnController.Instance.RunFromBattle();
    }
}
