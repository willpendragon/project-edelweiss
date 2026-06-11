using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using ProjectEdelweiss.Utils;
using System;

public class RadialMenuEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Vector2Int knockbackDirection;
    public int knockbackStrength = 2;
    [SerializeField] private Color _originalTileColor;
    [SerializeField] private TileController _knockbackTile;
    [SerializeField] private AlignmentIconHelper _alignmentIconHelper;
    [SerializeField] private ItemCounterUIHelper _itemCounterUIHelper;

    public enum ActionType
    {
        Move,
        Melee,
        Spell,
        Trap,
        Pray,
        Summon,
        Crystal,
        Run
    }


    [SerializeField] private TextMeshProUGUI _actionLabel;
    public Image icon;

    public ActionType actionType;
    public int priority;

    public delegate void NoPointsNotification(string notification);
    public static event NoPointsNotification OnPointsDepleted;

    public void SetLabel(string labelText)
    {
        _actionLabel.text = labelText;
    }

    public void FireAction()
    {
        if (actionType == ActionType.Run)
        {
            FindAnyObjectByType<CursorController>().ChangeCursorMode(actionType);
            return;
        }
        var activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit).GetComponent<Unit>();
        if (activePlayerUnit.unitOpportunityPoints <= 0)
        {
            OnPointsDepleted("Not enough Energy...");
            var cursorController = GameObject.FindAnyObjectByType<CursorController>();
            cursorController.CloseRadialMenu();
        }
        else
        {
            FindAnyObjectByType<CursorController>().ChangeCursorMode(actionType);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(1.5f, 0.5f);
        TriggerAdditionalBehaviour(actionType);
        BattleSFXManager.PlaySound(SoundType.RADIALHOVERSWITCH, 1);
    }

    private void TriggerAdditionalBehaviour(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Melee:
                DisplayKnockbackPreview();
                return;
                //case ActionType.Spell:
                //    DisplayAlignmentIcon();
                //    return;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1f, 0.5f);

        if (_knockbackTile == null)
            return;
        // Reset Knockback Preview (where applicable).
        _knockbackTile.tileShaderController.SetTileColor(1f, _originalTileColor);
        _knockbackTile = null;
    }

    public void DisplayAlignmentIcon()
    {
        _alignmentIconHelper.DisplayAlignmentIcon();
    }

    private void DisplayKnockbackPreview()
    {
        var activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit);
        // Block the Knockback preview for Magnet-equipped units.
        if (activePlayerUnit.GetComponent<Unit>().hasHookshot == true)
            return;

        var cursor = FindAnyObjectByType<CursorController>();
        Debug.Log($"Melee Targeting: {cursor.TargetedUnit}");

        if (cursor.TargetedUnit == null)
            return;

        // Block the Knockback preview if the Tile belongs to a Deity conduit.
        if (cursor.TargetedUnit.unitType == Unit.UnitType.Deity)
            return;


        if (!IsKnockbackPossible(activePlayerUnit.GetComponent<Unit>(), cursor.TargetedUnit.ownedTile))
            return;

        // Calculate the Knockback direction.
        _knockbackTile = CalculateKnockback(activePlayerUnit.GetComponent<Unit>(), cursor.TargetedUnit);

        // Doesn't display the Knockback feedback if the target Tile is occupied.
        // Band-aid fix, the knockback calculation shouldn't return a tile at all in such cases.
        if (_knockbackTile == null)
            return;
        if (_knockbackTile.detectedUnit != null)
            return;

        // Retrieve the current Tile Color
        _originalTileColor = _knockbackTile.tileShaderController.RetrieveCurrentTileColor();
        // Highlight the possible knockback destination tile
        _knockbackTile.tileShaderController.SetTileColor(1f, Color.yellow);
    }

    // Should be moved into a separate class.
    private TileController CalculateKnockback(Unit attacker, Unit defender)
    {
        Vector2Int attackerPos = attacker.GetGridPosition();
        Vector2Int defenderPos = defender.GetGridPosition();

        int deltaX = attackerPos.x - defenderPos.x;
        int deltaY = attackerPos.y - defenderPos.y;

        knockbackDirection = Vector2Int.zero;
        if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            knockbackDirection.x = -(int)Mathf.Sign(deltaX);
        else
            knockbackDirection.y = -(int)Mathf.Sign(deltaY);

        knockbackStrength = Mathf.Clamp(knockbackStrength, 1, 3);

        Vector2Int previewGridPos = defenderPos + (knockbackDirection * knockbackStrength);
        previewGridPos = ClampGridPosition(previewGridPos);

        TileController previewTile = GridManager.Instance.GetTileControllerInstance(previewGridPos.x, previewGridPos.y);
        return previewTile;
    }
    // Should be moved into a separate class.

    private bool IsKnockbackPossible(Unit activePlayerUnit, TileController targetTile)
    {
        DistanceController distanceController = GridManager.Instance.GetComponentInChildren<DistanceController>();
        return distanceController.CheckDistance(activePlayerUnit.ownedTile, targetTile);
    }
    // Should be moved into a separate class.

    private Vector2Int ClampGridPosition(Vector2Int pos)
    {
        var grid = GridManager.Instance;
        pos.x = Mathf.Clamp(pos.x, 0, grid.gridHorizontalSize - 1);
        pos.y = Mathf.Clamp(pos.y, 0, grid.gridVerticalSize - 1);
        return pos;
    }

    public void DisplayTributesCounterWrapper()
    {
        if (_itemCounterUIHelper == null)
            return;
        _itemCounterUIHelper.DisplayTributesCounter();
    }
}
