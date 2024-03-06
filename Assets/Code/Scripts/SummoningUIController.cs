using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SummoningUIController : MonoBehaviour
{
    public SummoningController summoningCastingController;
    public GameObject summonButtonPrefab;
    public Transform spellMenuContainer;
    public IPlayerAction currentPlayerAction;
    public Button currentButton;

    void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        // It's good practice to unsubscribe from the event when the GameObject is destroyed.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        summoningCastingController = GameObject.FindGameObjectWithTag("SummoningController").GetComponent<SummoningController>();
        spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
    }

    public enum SummonPhase
    {
        summoning,
        praying
    }

    public SummonPhase currentSummonPhase;

    public void AddSummonButton()
    //I will need to create a Pokémon-Style menu to summon the collected deities.
    {
        GameObject summonButtonInstance = Instantiate(summonButtonPrefab, spellMenuContainer);
        currentButton = summonButtonInstance.GetComponent<Button>();
        currentButton.GetComponentInChildren<Text>().text = "Summon";
        currentButton.onClick.AddListener(() => SwitchTilesToSummonMode());
    }
    public void SwitchTilesToSummonMode()
    {
        SummonPlayerAction summonPlayerActionInstance = new SummonPlayerAction();
        //Creates a new instance of the Melee Player Action
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = summonPlayerActionInstance;
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Switching tiles to Summon Mode");
        }
        //After clicking the Summon Button, all of the Grid Map tiles switch to Selection Mode and switch to the Summon Player Action
    }

    public void SwitchButtonToPrayMode()
    {
        currentButton.GetComponentInChildren<Text>().text = "Pray";
        currentButton.onClick.AddListener(() => SwitchTilesToPrayMode());
        Debug.Log("Switching Summon Button to Pray Mode");
    }

    public void SwitchTilesToPrayMode()
    {
        PrayPlayerAction prayPlayerActionInstance = new PrayPlayerAction();
        //Creates a new instance of the Pray Player Action
        currentPlayerAction = prayPlayerActionInstance;

        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = prayPlayerActionInstance;
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Switching tiles to Pray Mode");
        }
    }
}
