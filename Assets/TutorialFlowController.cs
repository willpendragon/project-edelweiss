using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

enum TutorialState
{
    MovementActionTutorial,
    MeleeActionTutorial,
    SpellActionTutorial,
    MagnetActionTutorial,
    TrapActionTutorial
}
public class TutorialFlowController : MonoBehaviour
{
    [SerializeField] RectTransform tutorialPanel;
    [SerializeField] RectTransform tutorialPanelBackground;
    [SerializeField] Button closeTutorialCanvasButton;
    [SerializeField] Button endTutorialButton;
    [SerializeField] RectTransform endTurnButtonRectTransform;

    [SerializeField] TextMeshProUGUI tutorialText;
    [SerializeField] string[] tutorialTexts;
    [SerializeField] RectTransform movesContainer;
    [SerializeField] RectTransform actionInfoPanel;
    [SerializeField] EndTurnButtonHelper endTurnButtonHelper;

    [SerializeField] Unit unitEdel;
    [SerializeField] Unit unitAliza;
    [SerializeField] Unit unitViolet;

    //private bool movementActionTutorialCompleted = false;
    //private bool meleeActionTutorialCompleted = false;
    //private bool spellActionTutorialCompleted = false;
    //private bool magnetActionTutorialCompleted = false;
    //private bool trapActionTutorialCompleted = false;

    [SerializeField] TutorialState currentTutorialState;
    private string saveFilePath;


    // Add a class that inhibits the possibility to click for the Player
    private void OnEnable()
    {
        MovePlayerAction.OnUnitMovedToTile += CompleteMovementActionTutorial;
        MeleePlayerAction.OnUsedMeleeAction += CompleteMeleeActionTutorial;
        AOESpellPlayerAction.OnUsedSpell += CompleteSpellActionTutorial;
        MeleePlayerAction.OnUsedMagnet += CompleteMagnetTutorial;
        TrapPlayerAction.OnTrapPlaced += CompleteTrapActionTutorial;
        TrapController.OnTrapAction += CompleteBattleTutorial;
    }
    private void OnDisable()
    {
        MovePlayerAction.OnUnitMovedToTile -= CompleteMovementActionTutorial;
        MeleePlayerAction.OnUsedMeleeAction -= CompleteMeleeActionTutorial;
        AOESpellPlayerAction.OnUsedSpell -= CompleteSpellActionTutorial;
        MeleePlayerAction.OnUsedMagnet -= CompleteMagnetTutorial;
        TrapPlayerAction.OnTrapPlaced -= CompleteTrapActionTutorial;
        TrapController.OnTrapAction -= CompleteBattleTutorial;
    }

    private void Start()
    {
        IdentifyPartyMembers();
        StartCoroutine(StartMovementTutorial());
        ActivateEndTutorialButton(false);
        endTurnButtonRectTransform.localScale = Vector3.zero;
    }

    private void IdentifyPartyMembers()
    {
        unitEdel = GameManager.Instance.playerPartyMembersInstances[0];
        unitAliza = GameManager.Instance.playerPartyMembersInstances[1];
        unitViolet = GameManager.Instance.playerPartyMembersInstances[2];
    }

    IEnumerator StartMovementTutorial()
    {
        yield return new WaitForSeconds(0.5f);

        currentTutorialState = TutorialState.MovementActionTutorial;
        EnlargeTutorialPanel();
        if (tutorialText != null && currentTutorialState == TutorialState.MovementActionTutorial)
        {
            tutorialText.text = tutorialTexts[0];
        }
    }

    private void CompleteMovementActionTutorial(TileController tile)
    {
        if (tutorialText != null && currentTutorialState == TutorialState.MovementActionTutorial)
        {
            if (currentTutorialState != TutorialState.MovementActionTutorial) return;
            EnlargeTutorialPanel();
            tutorialText.text = tutorialTexts[1];

            // Movement Action Tutorial is over, state changed to Melee Action Tutorial.
            currentTutorialState = TutorialState.MeleeActionTutorial;

            Debug.Log("Movement Action Tutorial Completed");
        }
    }
    private void CompleteMeleeActionTutorial(string moveName, string attackerName)
    {
        if (tutorialText.text != null && currentTutorialState == TutorialState.MeleeActionTutorial)
        {
            EnlargeTutorialPanel();
            tutorialText.text = tutorialTexts[2];

            // Melee Action Tutorial is over, state changed to Spell Action Tutorial.
            currentTutorialState = TutorialState.SpellActionTutorial;

            Debug.Log("Melee Action Tutorial Completed");
        }
    }
    private void CompleteSpellActionTutorial(string moveName, string attackerName)
    {
        if (tutorialText.text != null && currentTutorialState == TutorialState.SpellActionTutorial)
        {
            EnlargeTutorialPanel();
            tutorialText.text = tutorialTexts[3];

            // Spell Action Tutorial is over, state changed to Trap Action Tutorial
            currentTutorialState = TutorialState.TrapActionTutorial;
            DisableUnit(unitEdel);
            DisableUnit(unitViolet);
            Debug.Log("Spell Action Tutorial Completed");
            RevertGridMapToSelectionMode();
        }
    }
    private void CompleteTrapActionTutorial()
    {
        if (tutorialText.text != null && currentTutorialState == TutorialState.TrapActionTutorial)
        {
            EnlargeTutorialPanel();
            tutorialText.text = tutorialTexts[4];
            // Trap Action Tutorial is over, tutorial is over.
            currentTutorialState = TutorialState.MagnetActionTutorial;
            Debug.Log("Trap Action Tutorial Completed");
        }
    }
    private void CompleteMagnetTutorial()
    {

        if (tutorialText.text != null && currentTutorialState == TutorialState.MagnetActionTutorial)
        {
            //EnlargeTutorialPanel();
            //tutorialText.text = tutorialTexts[5];
            StartCoroutine(TutorialEndTurn());
            //StartCoroutine(TutorialEnd());
            Debug.Log("Magnet Action Tutorial Completed");
        }
    }
    IEnumerator TutorialEndTurn()
    {
        yield return new WaitForSeconds(1.5f);
        endTurnButtonHelper.EndTurnViaButton();
    }
    private void CompleteBattleTutorial()
    {
        //yield return new WaitForSeconds(5f);
        EnlargeTutorialPanel();
        tutorialText.text = tutorialTexts[6];
        if (closeTutorialCanvasButton != null)
        {
            closeTutorialCanvasButton.interactable = false;
        }
        ActivateEndTutorialButton(true);
    }

    public void CloseTutorial()
    {
        saveFilePath = Application.persistentDataPath + "/gameSaveData.json";
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Deleted Saved Game Data");
        }
        else
        {
            Debug.LogWarning("No Saved Game Data found.");
        }
        GameObject gameManagerInstance = GameObject.FindGameObjectWithTag("GameManager");
        Destroy(gameManagerInstance);
        DestroyPlayerPartyUnitsInstances();
        SceneManager.LoadScene("overworld_map");
    }

    private void DestroyPlayerPartyUnitsInstances()
    {
        Destroy(unitEdel?.gameObject);
        Destroy(unitAliza?.gameObject);
        Destroy(unitViolet?.gameObject);
    }

    private void RevertGridMapToSelectionMode()
    {
        TileController[] tileControllers = GridManager.Instance.gridTileControllers;
        foreach (var tileController in tileControllers)
        {
            tileController.currentPlayerAction = new SelectUnitPlayerAction();
            tileController.currentSingleTileStatus = SingleTileStatus.selectionMode;
        }
        movesContainer.localScale = new Vector3(0, 0, 0);
        actionInfoPanel.localScale = new Vector3(0, 0, 0);
    }

    public void MinimizeTutorialPanel()
    {
        tutorialPanel.localScale = Vector3.zero;
        tutorialPanelBackground.localScale = Vector3.zero;
    }

    private void EnlargeTutorialPanel()
    {
        tutorialPanel.localScale = Vector3.one;
        tutorialPanelBackground.localScale = Vector3.one;
    }

    private void DisableUnit(Unit playerUnit)
    {
        playerUnit.gameObject.GetComponent<UnitSelectionController>().StopUnitAction();
    }
    void ActivateEndTutorialButton(bool endTutorialButtonCondition)
    {
        if (endTutorialButtonCondition == true && endTutorialButton != null)
        {
            endTutorialButton.gameObject.SetActive(true);
        }
        else if (endTutorialButtonCondition == false && endTutorialButton != null)
        {
            endTutorialButton.gameObject.SetActive(false);
        }
    }
}