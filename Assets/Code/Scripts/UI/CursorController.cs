using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

    private bool _isRadialMenuOpen;

    private List<Button> _actionButtons = new List<Button>();

    private void OnEnable()
    {
        TileController.OnDragCursorAcrossTile += RetrieveTileStatus;
        TileController.OnEndDragCursorAcrossTile += CloseRadialMenu;
    }

    private void OnDisable()
    {
        TileController.OnDragCursorAcrossTile -= RetrieveTileStatus;
        TileController.OnEndDragCursorAcrossTile -= CloseRadialMenu;
    }

    private void Start()
    {
        state = CursorState.Basic;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
            {
                playerUnit.gameObject.tag = "ActivePlayerUnit";
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            CloseRadialMenu();
        }
    }

    private void RetrieveTileStatus(TileController tileController)
    {
        _tileController = tileController;
        OpenRadialMenu();
    }

    public void OpenRadialMenu()
    {
        if (_isRadialMenuOpen) return;
        if (_tileController.detectedUnit != null && _tileController.detectedUnit.CompareTag("Player")) return;
        if (_tileController.detectedUnit != null && _tileController.detectedUnit.CompareTag("ActivePlayerUnit")) return;

        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();

        _runButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
        _runButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Run;
        _runButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>().text = "Run";
        radialMenu.GetComponent<RadialMenu>().entries.Add(_runButtonPrefabInstance.GetComponent<RadialMenuEntry>());

        if (CheckDistance(activePlayerUnit.unitMovementLimit) && _tileController.detectedUnit == null)
        {
            _moveButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
            _moveButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Move;
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
                _trapButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>().text = "Trap";
                radialMenu.GetComponent<RadialMenu>().entries.Add(_trapButtonPrefabInstance.GetComponent<RadialMenuEntry>());
            }

            if (_tileController.currentSingleTileCondition == SingleTileCondition.free &&
                gameStatsManager.captureCrystalsCount > 0)
            {
                GameObject crystalButton = Instantiate(actionButtonPrefab, radialMenu.transform);
                crystalButton.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Crystal;
                crystalButton.GetComponentInChildren<TextMeshProUGUI>().text = "Crystal";
                radialMenu.GetComponent<RadialMenu>().entries.Add(crystalButton.GetComponent<RadialMenuEntry>());
            }

            if (_tileController.currentSingleTileCondition == SingleTileCondition.free &&
                activePlayerUnit.linkedDeity != null &&
                GridManager.Instance.gridMovementController.GetDistance(activePlayerUnit.ownedTile, _tileController) <= 3)
            {
                GameObject summonButton = Instantiate(actionButtonPrefab, radialMenu.transform);
                summonButton.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Summon;
                summonButton.GetComponentInChildren<TextMeshProUGUI>().text = "Summon";
                radialMenu.GetComponent<RadialMenu>().entries.Add(summonButton.GetComponent<RadialMenuEntry>());
            }
        }

        else if (CheckDistance(_meleeRange) && _tileController.detectedUnit != null)

        {
            _meleeButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
            _meleeButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Melee;
            _meleeButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>().text = "Melee";
            radialMenu.GetComponent<RadialMenu>().entries.Add(_meleeButtonPrefabInstance.GetComponent<RadialMenuEntry>());
        }


        else if (CheckDistance(activePlayerUnit.unitMovementLimit) && _tileController.detectedUnit != null)
        {
            _spellButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
            _spellButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Spell;
            _spellButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>().text = "Spell";
            radialMenu.GetComponent<RadialMenu>().entries.Add(_spellButtonPrefabInstance.GetComponent<RadialMenuEntry>());
        }

        if (_tileController.currentSingleTileCondition == SingleTileCondition.occupiedByDeity)
        {
            GameObject prayButton = Instantiate(actionButtonPrefab, radialMenu.transform);
            prayButton.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Pray;
            prayButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pray";
            radialMenu.GetComponent<RadialMenu>().entries.Add(prayButton.GetComponent<RadialMenuEntry>());
        }

        _isRadialMenuOpen = true;
        radialMenu.GetComponent<RadialMenu>().ArrangeButtons();
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

    private void PopulateButtonsList()
    {
        Button[] buttons = radialMenu.GetComponentsInChildren<Button>();
        _actionButtons.AddRange(buttons);
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
    }

    private bool CheckDistance(int limit)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        int distance = GridManager.Instance.gridMovementController.GetDistance(activePlayerUnit.ownedTile, _tileController);
        return distance <= limit;
    }
}
