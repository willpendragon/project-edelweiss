using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrapTileUIController : MonoBehaviour
{
    public TrapTileController trapTileController;
    public GameObject trapButtonPrefab;
    public Transform spellMenuContainer;

    public void AddTrapButton()
    {
        //Instantiate the Trap Button.
        GameObject trapTileButtonInstance = Instantiate(trapButtonPrefab, spellMenuContainer);
        Button currentTrapTileButton = trapTileButtonInstance.GetComponent<Button>();
        currentTrapTileButton.onClick.AddListener(() => SwitchTilesToTrapMode());
    }

    public void SwitchTilesToTrapMode()
    {
        //After clicking the Spell Button, all of the Grid Map tiles switch to Selection Mode and the Tile Controller current Action to Trap
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new TrapPlayerAction();
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Switching tiles to Trap Mode");
        }

    }
}
