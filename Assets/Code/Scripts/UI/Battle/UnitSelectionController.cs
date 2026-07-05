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

    public UnitSelectionStatus currentUnitSelectionStatus;
    public SpriteRenderer unitSprite;
    public UnitIconsController unitIconsController;
    public GameObject selectedTileInstance;
    public ReachableTilesVisualizer tileVisualizer;

    [SerializeField] private Unit _activePlayerUnit;
    [SerializeField] private GameObject _selectionIcon;
    [SerializeField] private GameObject _selectedUnitPanel;
    [SerializeField] private GameObject _enemyUnitPanel;
    [SerializeField] private List<Unit> _playerUnits;
    [SerializeField] private List<TileController> _reachableEnemyTiles = new List<TileController>();
    [SerializeField] private GameObject _selectedTile;

    private BattleInterface _battleUI;
    private GridMovementController _gridMovementController;
    private PlayerPartyProfilesUIManager _playerPartyProfilesUIManager;

    public delegate void FaithlessUnit(string notification);

    public static event FaithlessUnit OnFaithlessUnit;

    private void Awake()
    {
        _battleUI = GameObject.FindGameObjectWithTag(GameTags.BattleInterfaceCanvas)?.GetComponent<BattleInterface>();
        _gridMovementController = GameObject.FindGameObjectWithTag(GameTags.GridMovementController)
            ?.GetComponent<GridMovementController>();
        _playerPartyProfilesUIManager = FindAnyObjectByType<PlayerPartyProfilesUIManager>();
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
        _playerUnits = new List<Unit>();

        // Grab the freshly spawned dynamic units from the updated Party Controller!
        GameObject partyControllerObj = GameObject.FindGameObjectWithTag("PlayerPartyController");
        if (partyControllerObj != null)
        {
            var partyController = partyControllerObj.GetComponent<PlayerPartyController>();
            if (partyController != null && partyController.playerUnitsOnBattlefield != null)
            {
                foreach (var unitObj in partyController.playerUnitsOnBattlefield)
                {
                    if (unitObj != null)
                    {
                        Unit u = unitObj.GetComponent<Unit>();
                        if (u != null) _playerUnits.Add(u);
                    }
                }
            }
        }
    }

    public void SelectPlayerUnit(Unit playerUnit)
    {
        if (playerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead
            || playerUnit.currentUnitPhase == Unit.UnitPhase.Waiting)
            return;
        if (playerUnit.unitStatusController.unitCurrentStatus == UnitStatus.Faithless)
        {
            OnFaithlessUnit($"{playerUnit.unitTemplate.unitName} became Faithless...");
            return;
        }

        if (playerUnit.gameObject.tag == GameTags.Enemy || playerUnit.gameObject.tag == GameTags.Deity)
            return;
        // Play Feedback for invalid selection. Include negative statuses as invalid as well (such as paralysis).
        // Add icons that convey the Player Unit status.
        ResetEnemyReachableTiles();
        ClearPreviousSelection();
        SetAsActivePlayer(playerUnit);
        //SpawnSelectionIcon(playerUnit.gameObject);
        ChangeActivePlayerUnitTile(playerUnit);
        SpawnUnitInfoPanel(playerUnit);
        PlaySelectionFeedback(playerUnit);
        _playerPartyProfilesUIManager?.HighlightSelectedUnitProfile(playerUnit.unitTemplate.unitName);
        var reachableTilesVisualizer = FindAnyObjectByType<ReachableTilesVisualizer>();
        reachableTilesVisualizer.ShowReachableTiles();

        OutlineAttackableEnemies(playerUnit);

        // Pan camera to selected unit with smooth transition
        var cameraController = FindAnyObjectByType<CameraController>();
        if (cameraController != null)
        {
            cameraController.PanCameraToActiveUnit();
        }
    }

    public void ChangeActivePlayerUnitTile(Unit playerUnit)
    {
        // Feedback to identify the currently selected Player Unit.
        var tile = playerUnit.ownedTile;
        tile.tileShaderController.SetTileColor(1f, Color.green);
        // Display the selected player unit cursor on the tile.
        selectedTileInstance = Instantiate(_selectedTile, tile.transform);

        Collider tileCollider = tile.GetComponentInChildren<Collider>();
        float cursorYOffset = 0.07f; // Piccolo offset per evitare compenetrazione visiva

        if (tileCollider != null)
        {
            // Troviamo il tetto in coordinate World, poi lo trasformiamo in coordinata Local
            Vector3 worldTopPoint = new Vector3(tile.transform.position.x, tileCollider.bounds.max.y + cursorYOffset,
                tile.transform.position.z);
            selectedTileInstance.transform.position = worldTopPoint;
        }
        else
        {
            // Fallback se il tile non avesse un collider
            selectedTileInstance.transform.localPosition = new Vector3(0, 0.57f, 0);
        }
    }

    private void OutlineAttackableEnemiesWrapper(TileController tile)
    {
        OutlineAttackableEnemies(tile.detectedUnit.GetComponent<Unit>());
    }

    public void OutlineAttackableEnemies(Unit playerUnit)
    {
        ResetEnemyReachableTiles();
        _reachableEnemyTiles.Clear();

        _reachableEnemyTiles = _gridMovementController.GetMultipleTiles(playerUnit.ownedTile, ATTACKABLE_TILE_RANGE);

        foreach (var tile in _reachableEnemyTiles)
        {
            if (tile.detectedUnit != null && tile.detectedUnit.CompareTag(GameTags.Enemy))
            {
                var sprite = tile.detectedUnit.GetComponentInChildren<SpriteRenderer>();
                if (sprite != null)
                {
                    sprite.material.SetFloat("_OutlineThickness", 1f);
                    tile.tileShaderController.EnemyTileFeedback();
                }
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
                if (sprite != null)
                {
                    sprite.material.SetFloat("_OutlineThickness", 0f);
                    tile.tileShaderController.ResetEnemyTileFeedback();
                }
            }
        }
    }

    private void ClearPreviousSelection()
    {
        // Reset tile color
        if (GridManager.Instance != null)
            GridManager.Instance.ClearTileColors();

        foreach (var playerUnit in _playerUnits)
        {
            // Null-check prevents any remaining destroyed artifacts from crashing the loop
            if (playerUnit != null && playerUnit.gameObject != null)
            {
                playerUnit.tag = GameTags.Player;
            }
        }

        if (selectedTileInstance != null)
        {
            Destroy(selectedTileInstance);
        }

        if (_selectedUnitPanel != null)
        {
            Destroy(_selectedUnitPanel);
        }

        _activePlayerUnit = null;
    }

    private void SetAsActivePlayer(Unit playerUnit)
    {
        playerUnit.gameObject.tag = GameTags.ActivePlayerUnit;
        _activePlayerUnit = playerUnit;
        Debug.Log($"{playerUnit.unitTemplate.unitName} is now the {GameTags.ActivePlayerUnit}");

        // Destroy Enemy Info Panels
        var enemyPanels = GameObject.FindGameObjectsWithTag(GameTags.EnemyUnitProfile);

        foreach (var enemyPanel in enemyPanels)
        {
            // Destroy Enemy Info Panels.
            Destroy(enemyPanel);
        }
    }

    public void SpawnSelectionIcon(GameObject playerUnit)
    {
        DestroySelectionIcons();
        _selectionIcon = Instantiate(Resources.Load(GameTags.PlayerCharacterSelectorIcon) as GameObject,
            playerUnit.gameObject.transform);
        Vector3 playerSelectionInstanceOffset = new Vector3(0, 2.5f, 0);
        _selectionIcon.transform.localPosition += playerSelectionInstanceOffset;
    }

    public void SpawnUnitInfoPanel(Unit playerUnit)
    {
        ClearExistingPanels();
        _selectedUnitPanel = Instantiate(Resources.Load(GameTags.CurrentlySelectedUnit) as GameObject,
            _battleUI.battleDetails.transform);
        _selectedUnitPanel.tag = GameTags.ActiveCharacterUnitProfile;
        _selectedUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
        playerUnit.unitProfilePanel = _selectedUnitPanel;
        FillPanelDetails(playerUnit);
    }

    public void SelectEnemy(Unit enemyUnit)
    {
        if (enemyUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
            return;
        // Clear Existing Enemy Panels
        ClearPanelsByTag(GameTags.EnemyUnitProfile);

        _enemyUnitPanel = Instantiate(Resources.Load(GameTags.ENEMY_PROFILE) as GameObject,
            _battleUI.battleDetails.transform);
        _enemyUnitPanel.tag = GameTags.EnemyUnitProfile;
        _enemyUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
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
        BattleSFXManager.PlaySound(SoundType.SELECT, 1);
    }

    public void ResetUnitSelection()
    {
        DestroySelectionIcons();
        this.gameObject.tag = GameTags.Player;
        GridManager.Instance.currentPlayerUnit = null;
    }

    public void StopPlayerParty()
    {
        DestroySelectionIcons();
        unitIconsController?.DisplayWaitingIcon();
        GridManager.Instance.currentPlayerUnit = null;
        ClearPanelsByTag(GameTags.ActiveCharacterUnitProfile);
        ClearPanelsByTag(GameTags.EnemyUnitProfile);
        _playerPartyProfilesUIManager?.ClearProfileHighlights();

        foreach (var unitGO in _playerUnits)
        {
            if (unitGO != null) // Null-check safety
            {
                unitGO.GetComponent<Unit>().currentUnitPhase = Unit.UnitPhase.Waiting;
            }
        }

        OnUnitTurnEnded?.Invoke();
        // Hides the tiles where the Active Player Unit could move to.
        //tileVisualizer.ClearReachableTiles();
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