using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
        Run
    }

    [SerializeField] TileController tileController;
    [SerializeField] public CursorState state;
    [SerializeField] IPlayerAction actionType;
    [SerializeField] GameObject actionButtonPrefab;
    [SerializeField] private GameObject _moveButtonPrefabInstance;
    [SerializeField] private GameObject _meleeButtonPrefabInstance;
    [SerializeField] private GameObject _spellButtonPrefabInstance;
    [SerializeField] RectTransform radialMenu;
    [SerializeField] private TileController _tileController;
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
            // Debug Control
            // Add Edel as Active Player Unit
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
        {
            _tileController = tileController;
            OpenRadialMenu();
        }
    }
    void OpenRadialMenu()
    {
        if (_isRadialMenuOpen == true)
            return;

        if (CheckDistance() && _tileController.detectedUnit == null)// Knockback requires additional logic and will give a null ref at the moment
        {
            // Visualize Radial Menu near the Tile
            //radialMenu.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            // Instantiate Action Buttons inside the radial menu
            // The Action Button should hold the specific values for the corresponding move
            _moveButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
            _moveButtonPrefabInstance.GetComponent<RadialMenuEntry>().actionType = RadialMenuEntry.ActionType.Move;
            //_moveButtonPrefabInstance?.GetComponent<Button>().onClick.AddListener(() => ChangeTileToMove());
            TextMeshProUGUI moveButtonText = _moveButtonPrefabInstance.GetComponentInChildren<TextMeshProUGUI>();
            moveButtonText.text = "Move";
            radialMenu.GetComponent<RadialMenu>().entries.Add(_moveButtonPrefabInstance.GetComponent<RadialMenuEntry>());
        }
        else if (CheckDistance() && _tileController.detectedUnit != null)
        {
            //radialMenu.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            _meleeButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
            _spellButtonPrefabInstance = Instantiate(actionButtonPrefab, radialMenu.transform);
            // Remember that the logic is already expressed in the Action Interfaces, no need to change the move properties when spawning the button
            //_meleeButtonPrefabInstance?.GetComponent<Button>().onClick.AddListener(() => ChangeTileToMelee());
            //_spellButtonPrefabInstance?.GetComponent<Button>().onClick.AddListener(() => ChangeTileToSpell());
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
        // Arranges the button following a radial shape

        radialMenu.GetComponent<RadialMenu>().ArrangeButtons();
        PopulateButtonsList();
    }

    void CloseRadialMenu()
    {
        radialMenu.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        _moveButtonPrefabInstance = null;
        _meleeButtonPrefabInstance = null;
        _spellButtonPrefabInstance = null;
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

    //private void ChangeTileToMove()
    //{
    //    _tileController.currentPlayerAction = new MovePlayerAction();
    //    state = CursorState.Move;
    //    ChangeCursorMode(state);
    //}
    //public void ChangeTileToMelee()
    //{
    //    _tileController.currentPlayerAction = new MeleePlayerAction();
    //    state = CursorState.Melee;
    //    ChangeCursorMode(state);
    //}
    //private void ChangeTileToSpell()
    //{
    //    _tileController.currentPlayerAction = new AOESpellPlayerAction();
    //    state = CursorState.Spell;
    //    ChangeCursorMode(state);
    //}

    public void ChangeCursorMode(RadialMenuEntry.ActionType state)
    {
        switch (state)
        {
            case RadialMenuEntry.ActionType.Move:
                _tileController.currentPlayerAction = new MovePlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                Debug.Log($"Current Action is {state}");
                break;
        }
        switch (state)
        {
            case RadialMenuEntry.ActionType.Melee:
                _tileController.currentPlayerAction = new MeleePlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                Debug.Log($"Current Action is {state}");
                break;
        }
        switch (state)
        {
            case RadialMenuEntry.ActionType.Spell:
                _tileController.currentPlayerAction = new AOESpellPlayerAction();
                _tileController.currentPlayerAction.Execute(_tileController);
                Debug.Log($"Current Action is {state}");
                break;
        }
    }

    private bool CheckDistance()
    {
        // This method needs to check the distance and return the available actions.
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        int distance = GridManager.Instance.gridMovementController.GetDistance(activePlayerUnit.ownedTile, _tileController);
        if (distance <= 10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}