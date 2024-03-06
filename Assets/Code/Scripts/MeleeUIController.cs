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
        // It's good practice to unsubscribe from the event when the GameObject is destroyed.
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
        spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
    }


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