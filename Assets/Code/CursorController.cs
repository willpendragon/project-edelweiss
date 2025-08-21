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
    [SerializeField] RectTransform radialMenu;
    [SerializeField] private TileController _tileController;
    [SerializeField] private int hazardsLimit = 1;
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

    void OpenRadialMenu()
    {
        if (_isRadialMenuOpen == true)
            return;
        if (_tileController.detectedUnit != null && _tileController.detectedUnit.CompareTag("Player"))
        {
            return;
        }

        if (_tileController.detectedUnit != null && _tileController.detectedUnit.CompareTag("ActivePlayerUnit"))
        {
            return;
        }

        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();

        if (CheckDistance(activePlayerUnit.unitMovementLimit) && _tileController.detectedUnit == null)
        {
            _moveButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
            _moveButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Move;
            TextMeshProUGUI moveButtonText = _moveButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>();
            moveButtonText.text = "Move";
            radialMenu.GetComponent<RadialMenu>().entries.Add(_moveButtonPrefabInstance.GetComponent<RadialMenuEntry>());
        }

        if (CheckDistance(hazardsLimit) && _tileController.detectedUnit == null)
        {

            TrapController trapController = _tileController.GetComponentInChildren<TrapController>();
            if (_tileController.currentSingleTileCondition == SingleTileCondition.free &&
                trapController != null &&
                trapController.currentTrapActivationStatus != TrapController.TrapActivationStatus.active)
            {
                _trapButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
                _trapButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Trap;
                TextMeshProUGUI trapButtonText = _trapButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>();
                trapButtonText.text = "Trap";
                radialMenu.GetComponent<RadialMenu>().entries.Add(_trapButtonPrefabInstance.GetComponent<RadialMenuEntry>());
            }

            if (_tileController.currentSingleTileCondition == SingleTileCondition.free &&
                gameStatsManager.captureCrystalsCount > 0)
            {
                GameObject crystalButton = Instantiate(actionButtonPrefab, radialMenu.transform);
                crystalButton.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Crystal;
                TextMeshProUGUI crystalButtonText = crystalButton.GetComponentInChildren<TextMeshProUGUI>();
                crystalButtonText.text = "Crystal";
                radialMenu.GetComponent<RadialMenu>().entries.Add(crystalButton.GetComponent<RadialMenuEntry>());
            }
        }
        else if (CheckDistance(activePlayerUnit.unitMovementLimit) && _tileController.detectedUnit != null)
        {
            _meleeButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
            _spellButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
            TextMeshProUGUI meleeButtonText = _meleeButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>();
            meleeButtonText.text = "Melee";
            TextMeshProUGUI spellButtonText = _spellButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>();
            spellButtonText.text = "Spell";
            _meleeButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Melee;
            _spellButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Spell;
            radialMenu.GetComponent<RadialMenu>().entries.Add(_meleeButtonPrefabInstance.GetComponent<RadialMenuEntry>());
            radialMenu.GetComponent<RadialMenu>().entries.Add(_spellButtonPrefabInstance.GetComponent<RadialMenuEntry>());
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
        }
    }

    private bool CheckDistance(int limit)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        int distance = GridManager.Instance.gridMovementController.GetDistance(activePlayerUnit.ownedTile, _tileController);
        return distance <= limit;
    }
}
