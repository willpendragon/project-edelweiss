using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveUIController : MonoBehaviour
{
    public GameObject moveButtonPrefab;
    public Transform spellMenuContainer;
    private string buttonName;
    // Start is called before the first frame update
    public void AddMoveButton()
    {
        //Instantiates the Move Button
        GameObject moveButtonInstance = Instantiate(moveButtonPrefab, spellMenuContainer);
        Button currentMoveButton = moveButtonInstance.GetComponent<Button>();
        currentMoveButton.onClick.AddListener(() => SwitchTilesToMoveMode());
    }

    public void SwitchTilesToMoveMode()
    {
        MovePlayerAction movePlayerActionInstance = new MovePlayerAction();
        //Creates a new instance of the Move Player Action
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = movePlayerActionInstance;
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Switching Tiles to Move Mode");
        }
        //After clicking the Melee Button, all of the Grid Map tiles switch to Selection Mode and switch to the Move Player Action
    }
}