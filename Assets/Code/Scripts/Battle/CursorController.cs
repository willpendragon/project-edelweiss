using Edelweiss.Core;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public enum CursorState
    {
        Basic,
        Select,
        Melee,
        Spell,
        Summon,
        Pray,
        Move,
        Run,
        Crystal
    }

    [SerializeField] TileController tileController;
    [SerializeField] public CursorState state;
    [SerializeField] IPlayerAction<TileController> actionType;
    [SerializeField] GameObject actionButtonPrefab;
    [SerializeField] private GameObject _moveButtonPrefabInstance;
    [SerializeField] private GameObject _meleeButtonPrefabInstance;
    [SerializeField] private GameObject _spellButtonPrefabInstance;
    [SerializeField] private GameObject _trapButtonPrefabInstance;
    [SerializeField] private GameObject _runButtonPrefabInstance;
    [SerializeField] RectTransform radialMenu;
    [SerializeField] private TileController _tileController;
    private int _hazardsLimit = 1;
    private int _meleeRange = 2; // Fallback value
    private int _spellRange = 3; // Fallback value
    [SerializeField] private TurnController _turnController;
    [SerializeField] private Unit _targetedUnit;

    public Unit TargetedUnit => _targetedUnit;

    // Icons
    [SerializeField] private Sprite _moveIcon;
    [SerializeField] private Sprite _meleeIcon;
    [SerializeField] private Sprite _spellIcon;
    [SerializeField] private Sprite _runIcon;
    [SerializeField] private Sprite _trapIcon;
    [SerializeField] private Sprite _crystalIcon;
    [SerializeField] private Sprite _summonIcon;
    [SerializeField] private Sprite _prayIcon;
    [SerializeField] private Sprite _magnetIcon;

    private bool _isRadialMenuOpen;

    private List<Button> _actionButtons = new List<Button>();

    private GameObject CreateActionButton(
    RadialMenuEntry.ActionType actionType,
    Sprite icon,
    string label,
    int priority = 0) // Priority determines the position of the Action Button in the Radial Menu (clock-style).
    {
        var button = Instantiate(actionButtonPrefab, radialMenu.transform);
        var entry = button.GetComponent<RadialMenuEntry>();
        entry.actionType = actionType;
        entry.icon.sprite = icon;
        entry.priority = priority;
        button.GetComponentInChildren<TextMeshProUGUI>().text = label;
        radialMenu.GetComponent<RadialMenu>().entries.Add(entry);

        // Display Alignment Icons on Spell Action Buttons.
        if (entry.actionType == RadialMenuEntry.ActionType.Spell)
        {
            entry.DisplayAlignmentIcon();
        }
        // Display Deity Tributes count on place Tributes buttons.
        if (entry.actionType == RadialMenuEntry.ActionType.Crystal)
        {
            entry.DisplayTributesCounterWrapper();
        }

        return button;
    }

    private void OnEnable()
    {
        TileController.OnClickedOnTile += RetrieveTileStatus;
        TileController.OnPointerAwayFromTile += CloseRadialMenu;
        MovePlayerAction.OnUnitMovedToTile += UpdateTilesVisualizer;
    }

    private void OnDisable()
    {
        TileController.OnClickedOnTile -= RetrieveTileStatus;
        TileController.OnPointerAwayFromTile -= CloseRadialMenu;
        MovePlayerAction.OnUnitMovedToTile -= UpdateTilesVisualizer;
    }

    private void Start()
    {
        state = CursorState.Basic;
    }

    private void Update()
    {
        if (GridManager.IsUnitMoving)
            return;
        if (Input.GetMouseButtonDown(0))
        {
            // Shoot Raycasts
            SortInteractedItem();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            SortInteractedItemExit();
        }
    }
    private void SortInteractedItemExit()
    {
        // Check UI interactions
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        bool actionFired = false;

        // Iterate over all UI results
        foreach (RaycastResult result in results)
        {
            // Check for ActionButton Tag
            if (result.gameObject.CompareTag("ActionButton"))
            {
                result.gameObject.GetComponent<RadialMenuEntry>().FireAction();
                Debug.Log("Action Button Pressed (Mouse Up)");
                actionFired = true;
                break;
            }
        }

        if (!actionFired && _isRadialMenuOpen)
        {
            CloseRadialMenu();
        }
    }

    public void SortInteractedItem()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray rayOrigin = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(rayOrigin, out hitInfo))
        {
            if (hitInfo.collider.gameObject.CompareTag("Tile"))
            {
                RetrieveTileStatus(hitInfo.collider.gameObject.GetComponent<TileController>());
            }
            else if (hitInfo.collider.gameObject.CompareTag("ActionButton"))
            {
                hitInfo.collider.gameObject.gameObject.GetComponent<RadialMenuEntry>().FireAction();
                Debug.Log($"Hit: {hitInfo.collider.name}");
            }
            else
            {
                CloseRadialMenu();
            }
        }
    }

    private void RetrieveTileStatus(TileController tileController)
    {
        _tileController = tileController;
        OpenRadialMenu();
    }

    public void OpenRadialMenu()
    {
        if (_isRadialMenuOpen
            || (_tileController.detectedUnit != null && (
                _tileController.detectedUnit.CompareTag("Player") ||
                _tileController.detectedUnit.CompareTag("ActivePlayerUnit")))
            || _turnController.currentTurn == TurnController.Turn.EnemyTurn)
            return;


        // Cache references
        var radialMenuComp = radialMenu.GetComponent<RadialMenu>();
        var activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit")?.GetComponent<Unit>();
        var gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();

        // Set menu position
        radialMenu.position = Camera.main.WorldToScreenPoint(_tileController.transform.position);

        if (_tileController.detectedUnit != null && _tileController.detectedUnit.CompareTag("Enemy"))
            _targetedUnit = _tileController.detectedUnit.GetComponent<Unit>();

        // Display Enemy Unit Info (where applicable).
        var unitSelection = FindAnyObjectByType<UnitSelectionController>();
        if (_targetedUnit != null)
        {
            unitSelection.SelectEnemy(_targetedUnit);
        }

        // Run button
        _runButtonPrefabInstance = CreateActionButton(
            RadialMenuEntry.ActionType.Run, _runIcon, "Escape", 3);

        // Move button
        if (activePlayerUnit != null && CheckDistance(activePlayerUnit.unitMovementLimit) && _tileController.detectedUnit == null)
            _moveButtonPrefabInstance = CreateActionButton(
                RadialMenuEntry.ActionType.Move, _moveIcon, "Move", 1);

        // Trap, Crystal, Summon
        bool canPlaceHazard = CheckDistance(_hazardsLimit) && _tileController.detectedUnit == null;
        // Band-aid fix to allow only Aliza to use Traps.
        if (canPlaceHazard)
        {
            var trapController = _tileController.GetComponentInChildren<TrapController>();
            bool isTileFree = _tileController.currentSingleTileCondition == SingleTileCondition.free;

            if (isTileFree && trapController != null && trapController.currentTrapActivationStatus != TrapController.TrapActivationStatus.active && activePlayerUnit.unitTemplate.unitName == "Aliza")
                _trapButtonPrefabInstance = CreateActionButton(
                    RadialMenuEntry.ActionType.Trap, _trapIcon, "Trap", 2);

            if (isTileFree && gameStatsManager.captureCrystalsCount > 0)
                CreateActionButton(RadialMenuEntry.ActionType.Crystal, _crystalIcon, "Crystal", 4);
        }

        // Melee/Magnet
        if (activePlayerUnit != null && activePlayerUnit.unitTemplate != null)
        {
            int _meleeRange = activePlayerUnit.unitTemplate.physicAttackBehavior.GetAttackRange();
            bool canMelee = CheckDistance(_meleeRange) && _tileController.detectedUnit != null;

            if (canMelee)
            {
                _meleeButtonPrefabInstance = CreateActionButton(
                    RadialMenuEntry.ActionType.Melee,
                    GetButtonIcon(activePlayerUnit),
                    GetButtonName(activePlayerUnit),
                    4);
                DisplayHelp();
            }
        }
        // Spell
        bool canSpell = CheckDistance(_spellRange) && _tileController.detectedUnit != null;
        if (canSpell)
            _spellButtonPrefabInstance = CreateActionButton(
                RadialMenuEntry.ActionType.Spell, _spellIcon, "Spell", 5);

        _isRadialMenuOpen = true;
        PopulateButtonsList();
    }

    public void CloseRadialMenu()
    {
        radialMenu.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        _moveButtonPrefabInstance = null;
        _meleeButtonPrefabInstance = null;
        _spellButtonPrefabInstance = null;
        _trapButtonPrefabInstance = null;
        DestroyButtons();
        _actionButtons.Clear();
        _isRadialMenuOpen = false;
        radialMenu.GetComponent<RadialMenu>().ClearButtonsList();
    }

    public void DisplayHelp()
    {
        Debug.Log($"Tip - Targeted Unit Name: {_targetedUnit}");
    }

    private void DestroyButtons()
    {
        foreach (var button in _actionButtons)
        {
            Destroy(button.gameObject);
        }
    }
    private Sprite GetButtonIcon(Unit activePlayerUnit)
    {
        if (activePlayerUnit.hasHookshot == true)
        {
            return _magnetIcon;
        }
        else
        {
            return _meleeIcon;
        }
    }

    private string GetButtonName(Unit activePlayerUnit)
    {
        if (activePlayerUnit.hasHookshot == true)
        {
            return "Magnet";
        }
        else
        {
            return "Melee";
        }
    }

    private void PopulateButtonsList()
    {
        Button[] buttons = radialMenu.GetComponentsInChildren<Button>();
        _actionButtons.AddRange(buttons);
        radialMenu.GetComponent<RadialMenu>().ArrangeButtons();
    }

    public void ChangeCursorMode(RadialMenuEntry.ActionType state)
    {
        bool skipDestroyPanels = false;

        switch (state)
        {
            case RadialMenuEntry.ActionType.Move:
                _tileController.currentPlayerAction = new MovePlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                break;
            case RadialMenuEntry.ActionType.Melee:
                _tileController.currentPlayerAction = new MeleePlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                skipDestroyPanels = true;
                break;
            case RadialMenuEntry.ActionType.Spell:
                _tileController.currentPlayerAction = new AOESpellPlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                skipDestroyPanels |= true;
                break;
            case RadialMenuEntry.ActionType.Trap:
                _tileController.currentPlayerAction = new TrapPlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                break;
            case RadialMenuEntry.ActionType.Crystal:
                _tileController.currentPlayerAction = new PlaceCrystalPlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                break;
            case RadialMenuEntry.ActionType.Summon:
                _tileController.currentPlayerAction = new SummonPlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                break;
            case RadialMenuEntry.ActionType.Run:
                TurnController.Instance.RunFromBattle();
                DestroyEnemyInfoPanels();
                break;
        }

        if (skipDestroyPanels == false)
            DestroyEnemyInfoPanels();

        if (_targetedUnit?.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
        {
            // Destroy Enemy Info Panels.
            DestroyEnemyInfoPanels();
        }
        else
        {
            // Update Enemy Info Panels.
            UpdateEnemyInfoPanels();
        }

        CloseRadialMenu();

        bool allNoOpportunities = _turnController.playerUnitsOnBattlefield
            .Where(obj => obj != null)
            .Select(obj => obj.GetComponent<Unit>())
            .Where(unit => unit != null)
            // Exclude dead Units. The End Turn Button will flash when alive Units only have 0 OPs.
            .Where(unit => unit.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
            .All(unit => unit.unitOpportunityPoints <= 0);

        if (allNoOpportunities)
        {
            //var movesCounter = FindAnyObjectByType<MovesCounter>();
            //movesCounter.HighlightEndTurnButton();
        }

        if (_targetedUnit == null)
            return;
    }

    public void UpdateTilesVisualizer(TileController targetTile)
    {
        var reachableTilesVisualizer = FindAnyObjectByType<ReachableTilesVisualizer>();
        reachableTilesVisualizer.ShowReachableTiles();
    }

    private bool CheckDistance(int limit)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit")?.GetComponent<Unit>();
        if (activePlayerUnit != null)
        {
            int distance = GridManager.Instance.gridMovementController.GetDistance(activePlayerUnit.ownedTile, _tileController);
            return distance <= limit;
        }
        else return false;
    }

    private void DestroyEnemyInfoPanels()
    {
        var enemyPanels = GameObject.FindGameObjectsWithTag("EnemyUnitProfile");

        foreach (var enemyPanel in enemyPanels)
        {
            Destroy(enemyPanel);
        }
    }

    private void UpdateEnemyInfoPanels()
    {
        var enemyPanels = GameObject.FindGameObjectsWithTag("EnemyUnitProfile");

        foreach (var enemyPanel in enemyPanels)
        {
            enemyPanel.GetComponent<UnitProfileController>().UpdateTargetedUnitProfile(_targetedUnit);
        }
    }
}