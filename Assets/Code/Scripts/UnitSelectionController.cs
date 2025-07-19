using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UnitSelectionController : MonoBehaviour
{
    public enum UnitSelectionStatus
    {
        unitSelected,
        unitDeselected,
        unitTemporarilySelected,
        unitAttacking,
        unitWaiting
    }

    public delegate void UnitTurnEnded();
    public static event UnitTurnEnded OnUnitTurnEnded;

    public GameObject activeCharacterSelectorIcon;
    public GameObject moveButton;
    public GameObject waitButton;
    public UnitSelectionStatus currentUnitSelectionStatus;
    public SpellUIController unitSpellUIController;
    public SpriteRenderer unitSprite;
    public UnitIconsController unitIconsController;

    public const string reachableTilesVisualizer = "ReachableTilesVisualizer";

    private void OnEnable()
    {
        PlayableUnitSelectionHelper.OnPlayableUnitSelected += SelectPlayerUnit;
    }
    private void OnDisable()
    {
        PlayableUnitSelectionHelper.OnPlayableUnitSelected -= SelectPlayerUnit;
    }

    private void Start()
    {
        currentUnitSelectionStatus = UnitSelectionStatus.unitDeselected;
    }

    public void SelectPlayerUnit(Unit playerUnit)
    {
        if (playerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
            return;
        if (playerUnit.gameObject.tag == "Enemy" || playerUnit.gameObject.tag == "Deity")
            return;
        if (playerUnit.unitStatusController.unitCurrentStatus == UnitStatus.Faithless)
            return;
        // Play Feedback for invalid selection. Add icons that convey the Player Unit status
        SetAsActivePlayer(playerUnit);
        GameObject playerSelectorIconIstance = Instantiate(Resources.Load("PlayerCharacterSelectorIcon") as GameObject, playerUnit.gameObject.transform);

        Vector3 playerSelectionInstanceOffset = new Vector3(0, 2.5f, 0);
        playerSelectorIconIstance.transform.localPosition += playerSelectionInstanceOffset;
        PlaySelectionFeedback(playerUnit);
    }
    private void PlaySelectionFeedback(Unit playerUnit)
    {
        BattleFeedbackController battleFeedbackController = playerUnit.GetComponent<BattleFeedbackController>();
        battleFeedbackController.PlaySelectionSFX.Invoke();
    }
    private void SetAsActivePlayer(Unit playerUnit)
    {
        playerUnit.gameObject.tag = "ActivePlayerUnit";
        Debug.Log($"{playerUnit.unitTemplate.unitName} is now the ActivePlayerUnit");
    }

    public void ResetUnitSelection()
    {
        Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
        unitSpellUIController.ResetCharacterSpellsMenu();
        this.gameObject.tag = "Player";
        GridManager.Instance.currentPlayerUnit = null;
        currentUnitSelectionStatus = UnitSelectionStatus.unitDeselected;
    }

    public void GenerateWaitButton()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (unitSpellUIController != null && sceneName != "battle_tutorial")
        {
            GameObject newWaitButton = Instantiate(waitButton, unitSpellUIController.spellMenuContainer);
        }
    }
    public void StopUnitAction()
    {
        Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
        unitIconsController?.DisplayWaitingIcon();
        Debug.Log("Display Waiting Icon on Unit");
        this.gameObject.tag = "Player";

        GridManager.Instance.currentPlayerUnit = null;
        Destroy(GameObject.FindGameObjectWithTag("ActiveCharacterUnitProfile"));
        OnUnitTurnEnded();
        Button endTurnButton = GameObject.FindGameObjectWithTag("EndTurnButton").GetComponent<Button>();
        endTurnButton.interactable = true;

        //GameObject.FindGameObjectWithTag(reachableTilesVisualizer).GetComponent<ReachableTilesVisualizer>().ClearReachableTiles(0, 0.2F, Color.white);
    }
}
