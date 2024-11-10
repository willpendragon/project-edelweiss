using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CapsuleCrystalUIController : MonoBehaviour
{
    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        if (scene.name == "battle_prototype" || scene.name == "boss_battle_prototype")
        {
            spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
        }
    }

    public GameObject capsuleCrystalButtonPrefab;
    public Transform spellMenuContainer;

    public void AddPlaceCaptureCrystalButton()
    {
        //Instantiates the Trap Button.
        GameObject captureCrystalButtonInstance = Instantiate(capsuleCrystalButtonPrefab, spellMenuContainer);
        Button currentCaptureCrystalButton = captureCrystalButtonInstance.GetComponent<Button>();
        currentCaptureCrystalButton.onClick.AddListener(() => SwitchTilesToPlaceCaptureCrystal());
    }

    public void SwitchTilesToPlaceCaptureCrystal()
    {
        MoveInfoController.Instance.HideMoveInfoPanel();

        // After clicking the Spell Button, all of the Grid Map tiles switch to Selection Mode and the Tile Controller current Action to Trap
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new PlaceCrystalPlayerAction();
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            tile.gameObject.GetComponentInChildren<TileShaderController>().AnimateFadeHeight(0, 0.2f, Color.white);
            Debug.Log("Switching tiles to Place Crystal Mode");
        }

    }
}
