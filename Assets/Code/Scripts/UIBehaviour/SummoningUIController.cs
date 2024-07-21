using System.Collections;
using System.Collections.Generic;
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
        SceneManager.sceneLoaded += OnSceneLoaded;
        TurnController.OnResetUnitUI += ResetButtonToSummonMode;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        TurnController.OnResetUnitUI -= ResetButtonToSummonMode;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "battle_prototype" || scene.name == "boss_battle_prototype")
        {
            spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
        }
    }

    public enum SummonPhase
    {
        summoning,
        praying
    }

    public SummonPhase currentSummonPhase = SummonPhase.summoning;

    public void AddSummonButton()

    {
        if (currentSummonPhase == SummonPhase.summoning)
        {
            GameObject summonButtonInstance = Instantiate(summonButtonPrefab, spellMenuContainer);
            currentButton = summonButtonInstance.GetComponent<Button>();
            currentButton.GetComponentInChildren<Text>().text = "Summon";
            currentButton.onClick.AddListener(() => SwitchTilesToSummonMode());
        }
        else if (currentSummonPhase == SummonPhase.praying)
        {
            GameObject summonButtonInstance = Instantiate(summonButtonPrefab, spellMenuContainer);
            currentButton = summonButtonInstance.GetComponent<Button>();
            currentButton.GetComponentInChildren<Text>().text = "Pray";
            currentButton.onClick.AddListener(() => SwitchTilesToPrayMode());
        }
    }
    public void SwitchTilesToSummonMode()
    {
        //Creates a new instance of the Melee Player Action
        SummonPlayerAction summonPlayerActionInstance = new SummonPlayerAction();

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
        currentSummonPhase = SummonPhase.praying;
        currentButton.GetComponentInChildren<Text>().text = "Pray";
        currentButton.onClick.AddListener(() => SwitchTilesToPrayMode());
    }
    public void ResetButtonToSummonMode()
    {
        Debug.Log("Reset Button to Summon Mode");
        currentSummonPhase = SummonPhase.summoning;
    }

    public void SwitchTilesToPrayMode()
    {
        //Creates a new instance of the Pray Player Action
        PrayPlayerAction prayPlayerActionInstance = new PrayPlayerAction();
        currentPlayerAction = prayPlayerActionInstance;

        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = prayPlayerActionInstance;
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Switching tiles to Pray Mode");
        }
    }
}
