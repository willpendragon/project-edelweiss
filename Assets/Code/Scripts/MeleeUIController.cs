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
        //currentSpellButton.GetComponentInChildren<Text>().text = buttonName;
        //currentMeleeButton.onClick.AddListener(() => meleeController.StartMeleeAttack());
        currentMeleeButton.onClick.AddListener(() => SwitchTilesToMeleeMode());
        //Better use this line to Change the currentPlayerAction for all Tiles to Melee Action
    }

    public void SwitchTilesToMeleeMode()
    {
        //After clicking the Melee Button, all of the Grid Map tiles switch to Selection Mode
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new MeleePlayerAction();
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Switching tiles to Melee Mode");
        }

    }
}