using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MoveUIController : MonoBehaviour
{
    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public GameObject moveButtonPrefab;
    public Transform spellMenuContainer;
    private string buttonName;
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        if (scene.name == "battle_prototype")
        {
            spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
            //Unit unitReference = this.GetComponent<Unit>();
            //unitReference.unitOpportunityPoints = unitReference.unitTemplate.unitOpportunityPoints;
        }
    }

    public void AddMoveButton()
    {
        //Instantiates the Move Button
        GameObject moveButtonInstance = Instantiate(moveButtonPrefab, spellMenuContainer);
        Button currentMoveButton = moveButtonInstance.GetComponent<Button>();
        currentMoveButton.onClick.AddListener(() => SwitchTilesToMoveMode());
    }

    public void SwitchTilesToMoveMode()
    {
        //MovePlayerAction movePlayerActionInstance = new MovePlayerAction();
        //Creates a new instance of the Move Player Action
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new MovePlayerAction();
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Switching Tiles to Move Mode");
        }
        //After clicking the Melee Button, all of the Grid Map tiles switch to Selection Mode and switch to the Move Player Action
    }
}