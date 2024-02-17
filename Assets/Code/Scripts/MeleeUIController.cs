using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeleeUIController : MonoBehaviour

{
    public MeleeController meleeController;
    public GameObject meleeButtonPrefab;
    public Transform spellMenuContainer;
    private string buttonName;

    public void AddMeleeButton()
    {
        //Instantiates the Melee Button.
        GameObject meleeButtonInstance = Instantiate(meleeButtonPrefab, spellMenuContainer);
        Button currentMeleeButton = meleeButtonInstance.GetComponent<Button>();
        currentMeleeButton.onClick.AddListener(() => SwitchTilesToMeleeMode());
    }

    public void SwitchTilesToMeleeMode()
    {
        MeleePlayerAction meleePlayerActionInstance = new MeleePlayerAction();
        //Creates a new instance of the Melee Player Action
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = meleePlayerActionInstance;
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Switching tiles to Melee Mode");
        }
        //After clicking the Melee Button, all of the Grid Map tiles switch to Selection Mode and switch to the Melee Player Action
    }
}