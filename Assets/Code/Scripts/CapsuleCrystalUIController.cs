using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CapsuleCrystalUIController : MonoBehaviour
{
    public GameObject capsuleCrystalButtonPrefab;
    public Transform spellMenuContainer;

    public void AddPlaceCaptureCrystalButton()
    {
        //Instantiate the Trap Button.
        GameObject captureCrystalButtonInstance = Instantiate(capsuleCrystalButtonPrefab, spellMenuContainer);
        Button currentCaptureCrystalButton = captureCrystalButtonInstance.GetComponent<Button>();
        currentCaptureCrystalButton.onClick.AddListener(() => SwitchTilesToPlaceCaptureCrystal());
    }

    public void SwitchTilesToPlaceCaptureCrystal()
    {
        //After clicking the Spell Button, all of the Grid Map tiles switch to Selection Mode and the Tile Controller current Action to Trap
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new PlaceCrystalPlayerAction();
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Switching tiles to Place Crystal Mode");
        }

    }
}
