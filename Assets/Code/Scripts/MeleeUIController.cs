using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MeleeUIController : MonoBehaviour
{
    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //public MeleeController meleeController;
    public GameObject meleeButtonPrefab;
    public Transform spellMenuContainer;
    private string buttonName;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        if (scene.name == "battle_prototype" || scene.name == "boss_battle_prototype")
        {
            spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
        }
    }


    public void AddMeleeButton()
    {
        //Instantiates the Melee Button.
        GameObject meleeButtonInstance = Instantiate(meleeButtonPrefab, spellMenuContainer);
        Button currentMeleeButton = meleeButtonInstance.GetComponent<Button>();
        currentMeleeButton.onClick.AddListener(() => SwitchTilesToSelectionMode());
    }

    public void SwitchTilesToSelectionMode()
    {
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new MeleePlayerAction();
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Switching tiles to Melee Mode");
        }
        //After clicking the Melee Button, all of the Grid Map tiles switch to Selection Mode and switch to the Melee Player Action
    }
}