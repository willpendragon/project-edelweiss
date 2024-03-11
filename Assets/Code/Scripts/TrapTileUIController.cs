using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrapTileUIController : MonoBehaviour
{
    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        // Unsubscribe from the event when the GameObject is destroyed.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public TrapTileController trapTileController;
    public GameObject trapButtonPrefab;
    public Transform spellMenuContainer;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        trapTileController = GameObject.FindGameObjectWithTag("TrapTileController").GetComponent<TrapTileController>();
        spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
    }

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
