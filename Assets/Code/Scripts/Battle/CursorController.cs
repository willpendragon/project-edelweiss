using Edelweiss.Core;
using System.Collections.Generic;
using System.Linq;
using Language.Lua;
using ProjectEdelweiss.Utils;
using TMPro;
using Unity.VisualScripting;
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
        Crystal,
        Attunement
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
    [SerializeField] private EscapeSettings _escapeSettings;
    [SerializeField] private Canvas _mainUICanvas;
    [SerializeField] private UnitSelectionController _unitSelectionController;

    private Coroutine _escapeCoroutine;
    private GameObject _escapeUIRoot;
    [SerializeField] private TMP_FontAsset escapeMenuFont;

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
    [SerializeField] private Sprite _attunementIcon;

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

        // Display Deity Tributes count on Attunement buttons.
        if (entry.actionType == RadialMenuEntry.ActionType.Attunement)
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

        if (_escapeCoroutine != null && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelEscape();
            return;
        }

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

        // Shoot through everything to find all potential targets under the cursor
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin).OrderBy(h => h.distance).ToArray();

        // PASS 1: Prioritize Action Buttons immediately
        foreach (var hitInfo in hits)
        {
            if (hitInfo.collider.gameObject.CompareTag("ActionButton"))
            {
                hitInfo.collider.gameObject.GetComponent<RadialMenuEntry>().FireAction();
                Debug.Log($"Hit: {hitInfo.collider.name}");
                return;
            }
        }

        // PASS 1.5: Environment Blocking!
        // If the very first thing the cursor touched (excluding UI) was a decorative wall/pillar, 
        // we stop right here. We do not want to click 'through' the pillar and select the tile/unit behind it.
        var firstPhysicalHit = hits.FirstOrDefault(h => !h.collider.gameObject.CompareTag("ActionButton"));
        if (firstPhysicalHit.collider != null)
        {
            // Assuming your decorations have a "Decorations" or "Environment" tag. 
            // If they are untagged, we check if they are attached to a Tile that is Environment.
            TileController hitTc = firstPhysicalHit.collider.GetComponentInParent<TileController>();

            // If we clearly hit an Environment tile mesh or a purely visual decoration prefab 
            // (You can also check for CompareTag("Obstacle") or whatever your prefab is tagged)
            if ((hitTc != null && hitTc.tileType == TileType.Environment) ||
                firstPhysicalHit.collider.gameObject.CompareTag("DecorationEnvironment"))
            {
                Debug.Log("Clicked directly on non-interactable Environment. Ignoring Raycast.");
                CloseRadialMenu();
                return;
            }
        }

        // PASS 2: Prioritize Enemies & Players
        foreach (var hitInfo in hits)
        {
            if (hitInfo.collider.gameObject.CompareTag("Enemy") || hitInfo.collider.gameObject.CompareTag("Player") ||
                hitInfo.collider.gameObject.CompareTag("ActivePlayerUnit") ||
                hitInfo.collider.gameObject.CompareTag("Deity") || hitInfo.collider.gameObject.CompareTag("DeityShard"))
            {
                Unit hitUnit = hitInfo.collider.gameObject.GetComponentInParent<Unit>();
                if (hitUnit != null && hitUnit.ownedTile != null)
                {
                    RetrieveTileStatus(hitUnit.ownedTile);
                    return;
                }
            }
        }

        // PASS 3: Fallback to the Grid Map Tiles (Only legitimate map tiles)
        foreach (var hitInfo in hits)
        {
            if (hitInfo.collider.gameObject.CompareTag("Tile"))
            {
                TileController tc = hitInfo.collider.gameObject.GetComponent<TileController>();

                if (tc != null)
                {
                    // Prevent clicking on purely visual decorations that act as fake tiles
                    if (!GridManager.Instance.gridMapDictionary.ContainsValue(tc))
                        continue;

                    RetrieveTileStatus(tc);
                    return;
                }
            }
        }

        CloseRadialMenu();
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

        if (_tileController.detectedUnit != null && (_tileController.detectedUnit.CompareTag("Enemy") ||
                                                     _tileController.detectedUnit.CompareTag("Deity") ||
                                                     _tileController.detectedUnit.CompareTag("DeityShard")))
            _targetedUnit = _tileController.detectedUnit.GetComponent<Unit>();

        // Display Enemy Unit Info (where applicable).

        if (_targetedUnit != null)
        {
            _unitSelectionController.SelectEnemy(_targetedUnit);
        }

        // Run button
        _runButtonPrefabInstance = CreateActionButton(
            RadialMenuEntry.ActionType.Run, _runIcon, "Escape", 3);

        // Move button
        if (activePlayerUnit != null && CheckDistance(activePlayerUnit.unitMovementLimit) &&
            _tileController.detectedUnit == null)
            _moveButtonPrefabInstance = CreateActionButton(
                RadialMenuEntry.ActionType.Move, _moveIcon, "Move", 1);

        // Trap, Crystal, Summon
        bool canPlaceHazard = CheckDistance(_hazardsLimit) && _tileController.detectedUnit == null;
        // Band-aid fix to allow only Aliza to use Traps.
        if (canPlaceHazard)
        {
            var trapController = _tileController.GetComponentInChildren<TrapController>();
            bool isTileFree = _tileController.currentSingleTileCondition == SingleTileCondition.free;

            if (isTileFree && trapController != null &&
                trapController.currentTrapActivationStatus != TrapController.TrapActivationStatus.active &&
                activePlayerUnit.unitTemplate.unitName == "Aliza")
                _trapButtonPrefabInstance = CreateActionButton(
                    RadialMenuEntry.ActionType.Trap, _trapIcon, "Trap", 2);

            if (isTileFree && gameStatsManager.captureCrystalsCount > 0)
                CreateActionButton(RadialMenuEntry.ActionType.Crystal, _crystalIcon, "Tribute", 4);
        }

        // Attunement button - shown when clicking deity altar tile while adjacent
        bool isAltarTile = IsDeityAltarTile(_tileController);
        bool isPlayerAdjacent = IsAdjacentToDeityAltar(activePlayerUnit);

        if (isAltarTile && isPlayerAdjacent)
        {
            Debug.Log("Created Attunement button.");
            CreateActionButton(RadialMenuEntry.ActionType.Attunement, _attunementIcon, "Attune", 6);
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
            case RadialMenuEntry.ActionType.Attunement:
                // TODO: Implement AttunementPlayerAction in Phase 2
                _tileController.currentPlayerAction = new AttunementPlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                break;
            // case RadialMenuEntry.ActionType.Summon:
            //     _tileController.currentPlayerAction = new SummonPlayerAction();
            //     _tileController.currentPlayerAction.Execute(_tileController);
            //     break;
            case RadialMenuEntry.ActionType.Run:
                if (_escapeCoroutine == null)
                {
                    if (PartyUtility.RetrieveActivePlayerUnit() == null)
                    {
                        BattleInterface.Instance.SetDeityNotification("Select a Player Unit first!");
                        CloseRadialMenu();
                    }
                    else
                    {
                        _escapeCoroutine = StartCoroutine(EscapeSequence());
                        DestroyEnemyInfoPanels();
                    }
                }

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
        if (activePlayerUnit == null || _tileController == null)
            return false;

        // Misuriamo manualmente i blocchi di "Distanza Manhattan" (Voxel Orthogonal Distance)
        Vector3Int pPos = activePlayerUnit.ownedTile.gridPosition;
        Vector3Int targetPos = _tileController.gridPosition;

        // Quanti "passi" di griglia in X e Z (profondit) abbiamo?
        int dstX = Mathf.Abs(pPos.x - targetPos.x);
        int dstZ = Mathf.Abs(pPos.z - targetPos.z);
        // Opzionale: int dstY = Mathf.Abs(pPos.y - targetPos.y); 
        // Aggiungi + dstY se vuoi che bersagliare uno pi in alto costi range extra.

        int blockDistance = dstX + dstZ;

        return blockDistance <= limit;
    }

    private bool IsDeityAltarTile(TileController tile)
    {
        if (tile == null)
        {
            Debug.LogWarning("[IsDeityAltarTile] Tile is null!");
            return false;
        }
                bool isDeityTileType = tile.tileType == TileType.DeityTile;
        bool isOccupiedByDeity = tile.currentSingleTileCondition == SingleTileCondition.occupiedByDeity;
        
        // Check if this tile is marked as a DeityTile or occupied by deity
        return isDeityTileType || isOccupiedByDeity;
    }

    private bool IsAdjacentToDeityAltar(Unit activePlayerUnit)
    {
        if (activePlayerUnit == null || activePlayerUnit.ownedTile == null)
        {
            return false;
        }

        var battleTypeController = BattleTypeController.Instance;
        if (battleTypeController == null || 
            battleTypeController.currentBattleType != BattleTypeController.BattleType.BattleWithDeity)
        {
            return false;
        }

        var deitySpawner = FindAnyObjectByType<DeitySpawner>();
        if (deitySpawner == null || deitySpawner.currentUnboundDeity == null)
        {
            return false;
        }

        var deityUnit = deitySpawner.currentUnboundDeity.GetComponent<Unit>();
        if (deityUnit == null || deityUnit.ownedTile == null)
        {
            return false;
        }

        Vector3Int playerPos = activePlayerUnit.ownedTile.gridPosition;
        Vector3Int deityPos = deityUnit.ownedTile.gridPosition;
        
        int distance = Mathf.Abs(playerPos.x - deityPos.x) + Mathf.Abs(playerPos.z - deityPos.z);
        
        Debug.Log($"[Adjacency] Player: {playerPos}, Deity: {deityPos}, Distance: {distance}");
        
        return distance == 1;
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

    private System.Collections.IEnumerator EscapeSequence()
    {
        var activePlayerUnitObj = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit);
        Unit activePlayerUnit = activePlayerUnitObj.GetComponent<Unit>();

        float timer = _escapeSettings != null ? _escapeSettings.gracePeriod : 1.5f;
        CreateEscapeUI();

        activePlayerUnit.unitOpportunityPoints--;
        BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateRemainingMoves(activePlayerUnit.unitTemplate
            .unitName);

        var timerText = _escapeUIRoot.GetComponentInChildren<TextMeshProUGUI>();

        while (timer > 0)
        {
            if (timerText != null)
                timerText.text = $"Escaping in {timer:F1}s...\n[ESC] TO CANCEL";

            timer -= Time.deltaTime;
            yield return null;
        }

        Destroy(_escapeUIRoot);
        _escapeCoroutine = null;

        // Execute the actual roll
        float roll = UnityEngine.Random.Range(0f, 100f);
        float chance = _escapeSettings != null ? _escapeSettings.escapeProbability : 100f;

        if (roll <= chance)
        {
            TurnController.Instance.RunFromBattle();
        }
        else
        {
            BattleInterface.Instance.SetDeityNotification("Escape failed!");
        }
    }

    private void CreateEscapeUI()
    {
        Canvas mainCanvas = _mainUICanvas;
        if (mainCanvas == null) return;

        _escapeUIRoot = new GameObject("EscapeCountdownUI");
        _escapeUIRoot.transform.SetParent(mainCanvas.transform, false);

        RectTransform rect = _escapeUIRoot.AddComponent<RectTransform>();
        // Set anchors to bottom-right
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);

        // Set pivot to bottom-right so it scales/positions from its own corner
        rect.pivot = new Vector2(1f, 0f);
        rect.sizeDelta = new Vector2(400, 100);

        // Add padding so it's not clipped by the screen edge (e.g., 20 pixels in)
        rect.anchoredPosition = new Vector2(-20f, 20f);

        var text = _escapeUIRoot.AddComponent<TextMeshProUGUI>();

        if (escapeMenuFont != null)
        {
            text.font = escapeMenuFont;
        }

        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24;
        text.color = Color.white;
        text.outlineWidth = 0.2f;
        text.outlineColor = Color.black;
    }

    private void CancelEscape()
    {
        if (_escapeCoroutine != null)
        {
            StopCoroutine(_escapeCoroutine);
            _escapeCoroutine = null;
        }

        if (_escapeUIRoot != null)
        {
            Destroy(_escapeUIRoot);
        }

        BattleInterface.Instance.SetDeityNotification("Escape Cancelled");
    }
}