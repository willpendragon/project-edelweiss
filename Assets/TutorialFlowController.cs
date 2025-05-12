using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;

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
    [SerializeField] TextMeshProUGUI tutorialText;
    [SerializeField] string[] tutorialTexts;

    [SerializeField] Unit unitEdel;
    [SerializeField] Unit unitAliza;
    [SerializeField] Unit unitViolet;

    //private bool movementActionTutorialCompleted = false;
    //private bool meleeActionTutorialCompleted = false;
    //private bool spellActionTutorialCompleted = false;
    //private bool magnetActionTutorialCompleted = false;
    //private bool trapActionTutorialCompleted = false;

    [SerializeField] TutorialState currentTutorialState;

    // Add a class that inhibits the possibility to click for the Player
    private void OnEnable()
    {
        MovePlayerAction.OnUnitMovedToTile += CompleteMovementActionTutorial;
        MeleePlayerAction.OnUsedMeleeAction += CompleteMeleeActionTutorial;
        AOESpellPlayerAction.OnUsedSpell += CompleteSpellActionTutorial;
        MeleePlayerAction.OnUsedMagnet += CompleteMagnetTutorial;
    }
    private void OnDisable()
    {
        MovePlayerAction.OnUnitMovedToTile -= CompleteMovementActionTutorial;
        MeleePlayerAction.OnUsedMeleeAction -= CompleteMeleeActionTutorial;
        AOESpellPlayerAction.OnUsedSpell -= CompleteSpellActionTutorial;
        MeleePlayerAction.OnUsedMagnet -= CompleteMagnetTutorial;

    }

    private void Start()
    {
        //Transform spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;

        IdentifyPartyMembers();
        StartCoroutine(StartMovementTutorial());
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

            // Spell Action Tutorial is over, state changed to Magnet Action Tutorial
            currentTutorialState = TutorialState.MagnetActionTutorial;
            DisableUnit(unitEdel);
            DisableUnit(unitViolet);
            Debug.Log("Spell Action Tutorial Completed");
        }
    }
    private void CompleteMagnetTutorial()
    {

        if (tutorialText.text != null && currentTutorialState == TutorialState.MagnetActionTutorial)
        {
            EnlargeTutorialPanel();
            tutorialText.text = tutorialTexts[4];

            // Magnet Action Tutorial is over, state changed to Trap Action Tutorial
            currentTutorialState = TutorialState.TrapActionTutorial;
            Debug.Log("Magnet Action Tutorial Completed");

        }
    }
    private void CompleteTrapActionTutorial(string moveName, string attackerName)
    {
        if (tutorialText.text != null && currentTutorialState == TutorialState.TrapActionTutorial)
        {
            EnlargeTutorialPanel();
            tutorialText.text = tutorialTexts[5];

            // Trap Action Tutorial is over, tutorial is over.

            Debug.Log("Trap Action Tutorial Completed");
        }
    }

    public void MinimizeTutorialPanel()
    {
        tutorialPanel.localScale = Vector3.zero;
    }

    private void EnlargeTutorialPanel()
    {
        tutorialPanel.localScale = Vector3.one;
    }

    private void DisableUnit(Unit playerUnit)
    {
        playerUnit.gameObject.GetComponent<UnitSelectionController>().StopUnitAction();
    }
}