using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] IPlayerAction actionType;
    [SerializeField] GameObject actionButtonPrefab;
    [SerializeField] private GameObject _moveButtonPrefabInstance;
    [SerializeField] private GameObject _meleeButtonPrefabInstance;
    [SerializeField] private GameObject _spellButtonPrefabInstance;
    [SerializeField] private GameObject _trapButtonPrefabInstance;
    [SerializeField] private GameObject _runButtonPrefabInstance;
    [SerializeField] RectTransform radialMenu;
    [SerializeField] private TileController _tileController;
    [SerializeField] private int _hazardsLimit = 1;
    [SerializeField] private int _meleeRange = 2; // Fallback value
    [SerializeField] private int _spellRange = 3; // Fallback value
    [SerializeField] private TurnController _turnController;
    [SerializeField] private Unit _targetedUnit;

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
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("ActionButton"))
            {
                result.gameObject.GetComponent<RadialMenuEntry>().FireAction();
                Debug.Log("Found Action Button");
                break;
            }
            else
            {
                CloseRadialMenu();
            }
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
        if (_isRadialMenuOpen)
            return;
        if (_tileController.detectedUnit != null && _tileController.detectedUnit.CompareTag("Player"))
            return;
        if (_tileController.detectedUnit != null && _tileController.detectedUnit.CompareTag("ActivePlayerUnit"))
            return;
        if (_turnController.currentTurn == TurnController.Turn.EnemyTurn)
            return;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_tileController.transform.position);
        radialMenu.position = screenPosition;

        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit")?.GetComponent<Unit>();

        if (_tileController.detectedUnit != null && _tileController.detectedUnit.CompareTag("Enemy"))
        {
            _targetedUnit = _tileController.detectedUnit.GetComponent<Unit>();
        }

        GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();
        _runButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
        _runButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Run;
        _runButtonPrefabInstance.GetComponent<RadialMenuEntry>().icon.sprite = _runIcon;
        _runButtonPrefabInstance.GetComponent<RadialMenuEntry>().priority = 3;
        _runButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>().text = "Escape from Battle";
        radialMenu.GetComponent<RadialMenu>().entries.Add(_runButtonPrefabInstance.GetComponent<RadialMenuEntry>());

        if (activePlayerUnit != null && CheckDistance(activePlayerUnit.unitMovementLimit) && _tileController.detectedUnit == null)
        {
            _moveButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
            _moveButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Move;
            _moveButtonPrefabInstance.GetComponent<RadialMenuEntry>().icon.sprite = _moveIcon;
            _moveButtonPrefabInstance.GetComponent<RadialMenuEntry>().priority = 1;
            _moveButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>().text = "Move";
            radialMenu.GetComponent<RadialMenu>().entries.Add(_moveButtonPrefabInstance.GetComponent<RadialMenuEntry>());
        }

        if (CheckDistance(_hazardsLimit) && _tileController.detectedUnit == null)
        {
            TrapController trapController = _tileController.GetComponentInChildren<TrapController>();
            if (_tileController.currentSingleTileCondition == SingleTileCondition.free &&
                trapController != null &&
                trapController.currentTrapActivationStatus != TrapController.TrapActivationStatus.active)
            {
                _trapButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
                _trapButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Trap;
                _trapButtonPrefabInstance.GetComponent<RadialMenuEntry>().icon.sprite = _trapIcon;
                _trapButtonPrefabInstance.GetComponent<RadialMenuEntry>().priority = 2;
                _trapButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>().text = "Trap";
                radialMenu.GetComponent<RadialMenu>().entries.Add(_trapButtonPrefabInstance.GetComponent<RadialMenuEntry>());
            }

            if (_tileController.currentSingleTileCondition == SingleTileCondition.free &&
                gameStatsManager.captureCrystalsCount > 0)
            {
                GameObject crystalButton = Instantiate(actionButtonPrefab, radialMenu.transform);
                crystalButton.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Crystal;
                crystalButton.GetComponent<RadialMenuEntry>().icon.sprite = _crystalIcon;
                crystalButton.GetComponentInChildren<TextMeshProUGUI>().text = "Crystal";
                radialMenu.GetComponent<RadialMenu>().entries.Add(crystalButton.GetComponent<RadialMenuEntry>());
            }

            if (_tileController.currentSingleTileCondition == SingleTileCondition.free &&
                activePlayerUnit.linkedDeity != null &&
                GridManager.Instance.gridMovementController.GetDistance(activePlayerUnit.ownedTile, _tileController) <= 3)
            {
                GameObject summonButton = Instantiate(actionButtonPrefab, radialMenu.transform);
                summonButton.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Summon;
                summonButton.GetComponent<RadialMenuEntry>().icon.sprite = _summonIcon;
                summonButton.GetComponentInChildren<TextMeshProUGUI>().text = "Summon";
                radialMenu.GetComponent<RadialMenu>().entries.Add(summonButton.GetComponent<RadialMenuEntry>());
            }
        }

        if (CheckDistance(_meleeRange) && _tileController.detectedUnit != null)

        {
            _meleeButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
            _meleeButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Melee;
            _meleeButtonPrefabInstance.GetComponent<RadialMenuEntry>().icon.sprite = GetButtonIcon(activePlayerUnit);
            _meleeButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>().text = GetButtonName(activePlayerUnit);
            _meleeButtonPrefabInstance.GetComponent<RadialMenuEntry>().priority = 4;
            radialMenu.GetComponent<RadialMenu>().entries.Add(_meleeButtonPrefabInstance.GetComponent<RadialMenuEntry>());

        }

        if (CheckDistance(_spellRange) && _tileController.detectedUnit != null)
        {
            _spellButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
            _spellButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Spell;
            _spellButtonPrefabInstance.GetComponent<RadialMenuEntry>().icon.sprite = _spellIcon;
            _spellButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>().text = "Spell";
            _spellButtonPrefabInstance.GetComponent<RadialMenuEntry>().priority = 5;
            radialMenu.GetComponent<RadialMenu>().entries.Add(_spellButtonPrefabInstance.GetComponent<RadialMenuEntry>());
        }

        if (_tileController.currentSingleTileCondition == SingleTileCondition.occupiedByDeity)
        {
            GameObject prayButton = Instantiate(actionButtonPrefab, radialMenu.transform);
            prayButton.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Pray;
            prayButton.GetComponent<RadialMenuEntry>().icon.sprite = _prayIcon;
            prayButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pray";
            radialMenu.GetComponent<RadialMenu>().entries.Add(prayButton.GetComponent<RadialMenuEntry>());
        }

        _isRadialMenuOpen = true;
        PopulateButtonsList();
    }

    void CloseRadialMenu()
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

        switch (state)
        {
            case RadialMenuEntry.ActionType.Move:
                _tileController.currentPlayerAction = new MovePlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                break;
            case RadialMenuEntry.ActionType.Melee:
                _tileController.currentPlayerAction = new MeleePlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                break;
            case RadialMenuEntry.ActionType.Spell:
                _tileController.currentPlayerAction = new AOESpellPlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
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
            case RadialMenuEntry.ActionType.Pray:
                _tileController.currentPlayerAction = new PrayPlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                break;
            case RadialMenuEntry.ActionType.Run:
                TurnController.Instance.RunFromBattle();
                break;
        }
        // Updating the Slider value
        var slider = _targetedUnit.transform.GetComponentInChildren<Slider>();
        slider.value = _targetedUnit.unitHealthPoints;
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
}
