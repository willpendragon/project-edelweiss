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
        currentTrapTileButton.onClick.AddListener(() => trapTileController.StartTrapTile());
    }
}
