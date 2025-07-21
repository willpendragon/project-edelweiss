using System;
using System.Collections.Generic;
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

    public GameObject waitButton;
    public UnitSelectionStatus currentUnitSelectionStatus;
    public SpellUIController unitSpellUIController;
    public SpriteRenderer unitSprite;
    public UnitIconsController unitIconsController;
    [SerializeField] private Unit _activePlayerUnit;
    [SerializeField] private GameObject _selectionIcon;
    [SerializeField] private GameObject _selectedUnitPanel;
    [SerializeField] private List<Unit> _playerUnits;

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
        SetPlayerUnits();
    }
    private void SetPlayerUnits()
    {
        _playerUnits = GameManager.Instance.playerPartyMembersInstances;
    }

    public void SelectPlayerUnit(Unit playerUnit)
    {
        if (playerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
            return;
        if (playerUnit.gameObject.tag == "Enemy" || playerUnit.gameObject.tag == "Deity")
            return;
        if (playerUnit.unitStatusController.unitCurrentStatus == UnitStatus.Faithless)
            return;
        // Play Feedback for invalid selection. Include negative statuses as invalid as well (such as paralysis).
        // Add icons that convey the Player Unit status.
        ClearPreviousSelection();
        SetAsActivePlayer(playerUnit);
        SpawnSelectionIcon(playerUnit.gameObject);
        SpawnUnitInfoPanel(playerUnit);
        PlaySelectionFeedback(playerUnit);
    }

    private void ClearPreviousSelection()
    {
        if (_selectionIcon == null)
            return;
        if (_selectedUnitPanel == null)
            return;
        if (_activePlayerUnit == null)
            return;

        foreach (var playerUnit in _playerUnits)
        {
            playerUnit.tag = "Player";
        }
        Destroy(_selectionIcon);
        Destroy(_selectedUnitPanel);
    }

    private void SetAsActivePlayer(Unit playerUnit)
    {
        playerUnit.gameObject.tag = "ActivePlayerUnit";
        _activePlayerUnit = playerUnit;
        Debug.Log($"{playerUnit.unitTemplate.unitName} is now the ActivePlayerUnit");
    }
    private void SpawnSelectionIcon(GameObject playerUnit)
    {
        _selectionIcon = Instantiate(Resources.Load("PlayerCharacterSelectorIcon") as GameObject, playerUnit.gameObject.transform);
        Vector3 playerSelectionInstanceOffset = new Vector3(0, 2.5f, 0);
        _selectionIcon.transform.localPosition += playerSelectionInstanceOffset;
    }

    private void SpawnUnitInfoPanel(Unit playerUnit)
    {
        _selectedUnitPanel = Instantiate(Resources.Load("CurrentlySelectedUnit") as GameObject, GameObject.FindGameObjectWithTag("BattleInterfaceCanvas").transform);
        _selectedUnitPanel.tag = "ActiveCharacterUnitProfile";
        _selectedUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
        playerUnit.unitProfilePanel = _selectedUnitPanel;
        _selectedUnitPanel.GetComponent<PlayerProfileController>().activeCharacterName.text = playerUnit.unitTemplate.unitName;
        Debug.Log($"Spawned {_selectedUnitPanel} belonging to {playerUnit.unitTemplate.unitName}");
    }

    private void PlaySelectionFeedback(Unit playerUnit)
    {
        BattleFeedbackController battleFeedbackController = playerUnit.GetComponent<BattleFeedbackController>();
        battleFeedbackController.PlaySelectionSFX.Invoke();
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
    }
}
