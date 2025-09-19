using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectEdelweiss.Utils;

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

    private const int ATTACKABLE_TILE_RANGE = 2;

    public GameObject waitButton;
    public UnitSelectionStatus currentUnitSelectionStatus;
    //public SpellUIController unitSpellUIController;
    public SpriteRenderer unitSprite;
    public UnitIconsController unitIconsController;
    [SerializeField] private Unit _activePlayerUnit;
    [SerializeField] private GameObject _selectionIcon;
    [SerializeField] private GameObject _selectedUnitPanel;
    [SerializeField] private GameObject _enemyUnitPanel;
    [SerializeField] private List<Unit> _playerUnits;
    [SerializeField] private List<TileController> _reachableEnemyTiles = new List<TileController>();

    private BattleInterface _battleUI;
    private GridMovementController _gridMovementController;

    private void Awake()
    {
        _battleUI = GameObject.FindGameObjectWithTag(GameTags.BattleInterfaceCanvas)?.GetComponent<BattleInterface>();
        _gridMovementController = GameObject.FindGameObjectWithTag(GameTags.GridMovementController)?.GetComponent<GridMovementController>();
    }

    private void OnEnable()
    {
        MovePlayerAction.OnUnitMovedToTile += OutlineAttackableEnemiesWrapper;
    }
    private void OnDisable()
    {
        MovePlayerAction.OnUnitMovedToTile -= OutlineAttackableEnemiesWrapper;
    }
    private void Start()
    {
        SetPlayerUnits();
    }
    private void SetPlayerUnits()
    {
        _playerUnits = GameManager.Instance.playerPartyMembersInstances;
    }

    public void SelectPlayerUnit(Unit playerUnit)
    {
        if (playerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead
            || playerUnit.currentUnitPhase == Unit.UnitPhase.Waiting
            || playerUnit.unitStatusController.unitCurrentStatus == UnitStatus.Faithless)
            return;
        if (playerUnit.gameObject.tag == GameTags.Enemy || playerUnit.gameObject.tag == GameTags.Deity)
            return;
        // Play Feedback for invalid selection. Include negative statuses as invalid as well (such as paralysis).
        // Add icons that convey the Player Unit status.
        ResetEnemyReachableTiles();
        ClearPreviousSelection();
        SetAsActivePlayer(playerUnit);
        SpawnSelectionIcon(playerUnit.gameObject);
        SpawnUnitInfoPanel(playerUnit);
        PlaySelectionFeedback(playerUnit);
        var reachableTilesVisualizer = FindAnyObjectByType<ReachableTilesVisualizer>();
        reachableTilesVisualizer.ShowReachableTiles();

        OutlineAttackableEnemies(playerUnit);
    }
    private void OutlineAttackableEnemiesWrapper(TileController tile)
    {
        OutlineAttackableEnemies(tile.detectedUnit.GetComponent<Unit>());
    }

    public void OutlineAttackableEnemies(Unit playerUnit)
    {
        ResetEnemyReachableTiles();
        _reachableEnemyTiles.Clear();
        // Outline Attackable Enemies
        _reachableEnemyTiles = _gridMovementController.GetMultipleTiles(playerUnit.ownedTile, ATTACKABLE_TILE_RANGE);

        foreach (var tile in _reachableEnemyTiles)
        {
            if (tile.detectedUnit != null && tile.detectedUnit.CompareTag(GameTags.Enemy))
            {
                var sprite = tile.detectedUnit.GetComponentInChildren<SpriteRenderer>();
                sprite.material.SetFloat("_OutlineThickness", 1f);
                tile.tileShaderController.EnemyTileFeedback(1f, 0.2f, Color.red);
            }
        }
    }

    public void ResetEnemyReachableTiles()
    {
        if (_reachableEnemyTiles.Count == 0)
            return;
        foreach (var tile in _reachableEnemyTiles)
        {
            if (tile.detectedUnit != null && tile.detectedUnit.CompareTag(GameTags.Enemy))
            {
                SpriteRenderer sprite = tile.detectedUnit.GetComponentInChildren<SpriteRenderer>();
                sprite.material.SetFloat("_OutlineThickness", 0f);
                tile.tileShaderController.ResetEnemyTileFeedback(0f, 0.2f, Color.white);
            }
        }
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
            playerUnit.tag = GameTags.Player;
        }
        Destroy(_selectionIcon);
        Destroy(_selectedUnitPanel);
    }

    private void SetAsActivePlayer(Unit playerUnit)
    {
        playerUnit.gameObject.tag = GameTags.ActivePlayerUnit;
        _activePlayerUnit = playerUnit;
        Debug.Log($"{playerUnit.unitTemplate.unitName} is now the {GameTags.ActivePlayerUnit}");
    }
    public void SpawnSelectionIcon(GameObject playerUnit)
    {
        DestroySelectionIcons();
        _selectionIcon = Instantiate(Resources.Load(GameTags.PlayerCharacterSelectorIcon) as GameObject, playerUnit.gameObject.transform);
        Vector3 playerSelectionInstanceOffset = new Vector3(0, 2.5f, 0);
        _selectionIcon.transform.localPosition += playerSelectionInstanceOffset;
    }

    public void SpawnUnitInfoPanel(Unit playerUnit)
    {
        ClearExistingPanels();
        _selectedUnitPanel = Instantiate(Resources.Load(GameTags.CurrentlySelectedUnit) as GameObject, _battleUI.battleDetails.transform);
        _selectedUnitPanel.tag = GameTags.ActiveCharacterUnitProfile;
        _selectedUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
        playerUnit.unitProfilePanel = _selectedUnitPanel;
        FillPanelDetails(playerUnit);

    }

    public void SelectEnemy(Unit enemyUnit)
    {
        // Clear Existing Enemy Panels

        GameObject[] existingInfoPanels = GameObject.FindGameObjectsWithTag(GameTags.EnemyUnitProfile);
        foreach (var existingPanel in existingInfoPanels)
        {
            Destroy(existingPanel);
        }

        _enemyUnitPanel = Instantiate(Resources.Load(GameTags.CurrentlySelectedUnit) as GameObject, _battleUI.battleDetails.transform);
        _enemyUnitPanel.tag = GameTags.EnemyUnitProfile;
        _enemyUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerRight;
        enemyUnit.unitProfilePanel = _enemyUnitPanel;
        UnitProfileController unitProfileController = _enemyUnitPanel.GetComponent<UnitProfileController>();
        unitProfileController.ApplyProfileChanges(enemyUnit.gameObject);
        Debug.Log($"Spawned {_selectedUnitPanel} belonging to {enemyUnit.unitTemplate.unitName}");
    }

    private void ClearExistingPanels()
    {
        GameObject[] existingInfoPanels = GameObject.FindGameObjectsWithTag(GameTags.ActiveCharacterUnitProfile);
        foreach (var existingPanel in existingInfoPanels)
        {
            Destroy(existingPanel);
        }
    }

    public void FillPanelDetails(Unit unit)
    {
        UnitProfileController unitProfileController = _selectedUnitPanel.GetComponent<UnitProfileController>();
        unitProfileController.ApplyProfileChanges(unit.gameObject);
        Debug.Log($"Spawned {_selectedUnitPanel} belonging to {unit.unitTemplate.unitName}");
    }

    private void PlaySelectionFeedback(Unit playerUnit)
    {
        BattleFeedbackController battleFeedbackController = playerUnit.GetComponent<BattleFeedbackController>();
        battleFeedbackController.PlaySelectionSFX.Invoke();
    }

    public void ResetUnitSelection()
    {
        DestroySelectionIcons();
        //unitSpellUIController.ResetCharacterSpellsMenu();
        this.gameObject.tag = GameTags.Player;
        GridManager.Instance.currentPlayerUnit = null;
    }

    public void GenerateWaitButton()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        //if (unitSpellUIController != null && sceneName != GameTags.BattleTutorialScene)
        //{
        //    GameObject newWaitButton = Instantiate(waitButton, unitSpellUIController.spellMenuContainer);
        //}
    }
    public void StopPlayerParty()
    {
        DestroySelectionIcons();
        unitIconsController?.DisplayWaitingIcon();
        GridManager.Instance.currentPlayerUnit = null;
        ClearPanelsByTag(GameTags.ActiveCharacterUnitProfile);
        foreach (var unitGO in _playerUnits)
        {
            unitGO.GetComponent<Unit>().currentUnitPhase = Unit.UnitPhase.Waiting;
        }
        OnUnitTurnEnded?.Invoke();
    }

    private void DestroySelectionIcons()
    {
        GameObject[] icons = GameObject.FindGameObjectsWithTag(GameTags.ActivePlayerUnitIcon);
        foreach (var icon in icons)
        {
            Destroy(icon);
        }
    }

    private void ClearPanelsByTag(string tag)
    {
        foreach (var panel in GameObject.FindGameObjectsWithTag(tag))
            Destroy(panel);
    }
}