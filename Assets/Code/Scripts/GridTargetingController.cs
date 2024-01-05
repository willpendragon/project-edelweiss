using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridTargetingController : MonoBehaviour
{
    public GridManager gridManager;
    public delegate void TargetedUnit(Unit targetedUnit);
    public static event TargetedUnit OnTargetedUnit;

    private void OnEnable()
    {
        TileController.OnTileClickedAttackMode += SetUnitAsTarget;
        SpellcastingController.OnCastingSpell += SwitchTilesToAttackSelectionMode;
    }

    private void OnDisable()
    {
        TileController.OnTileClickedAttackMode -= SetUnitAsTarget;
        SpellcastingController.OnCastingSpell -= SwitchTilesToAttackSelectionMode;
    }
    public void SwitchTilesToAttackSelectionMode()
    {
        GameObject[] battlefieldTiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (var battlefieldTile in battlefieldTiles)
        {
            //Switch all Battlefield on Tiles to Attack Selection Mode.
            battlefieldTile.GetComponent<TileController>().currentSingleTileStatus = SingleTileStatus.attackSelectionModeActive;
        }
    }
    public Unit SetUnitAsTarget(int tileXCoordinate, int tileYCoordinate)
    {
        Unit targetUnit = GridManager.Instance.GetTileControllerInstance(tileXCoordinate, tileYCoordinate).GetComponent<TileController>().detectedUnit.GetComponent<Unit>();
        OnTargetedUnit(targetUnit);
        return targetUnit;
    }
}
