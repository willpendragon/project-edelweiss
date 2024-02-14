using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridTargetingController : MonoBehaviour
{
    public GridManager gridManager;

    public delegate void MeleeTargetedUnit(Unit meleeTargetedUnit);
    public static event MeleeTargetedUnit OnMeleeTargetedUnit;

    public delegate void TargetedUnit(Unit targetedUnit);
    public static event TargetedUnit OnTargetedUnit;

    public delegate void UnitSetAsSpellEpicenter(Unit epicenterUnit);
    public static event UnitSetAsSpellEpicenter OnUnitSetAsSpellEpicenter;

    public delegate void TileSetAsTrap(TileController trapTile);
    public static event TileSetAsTrap OnTileSetAsTrap;

    private void OnEnable()
    {
        TileController.OnTileClickedMeleeMode += SetUnitAsMeleeTarget;
        TileController.OnTileClickedAttackMode += SetUnitAsTarget;
        TileController.OnTileClickedTrapTileMode += SetTileAsTrapTileTarget;
        TileController.OnTileClickedAOESpellMode += SetUnitAsSpellEpicenter;
        MeleeController.OnMeleeAttack += SwitchTilesToMeleeSelectionMode;
        SpellcastingController.OnCastingSpell += SwitchTilesToAttackSelectionMode;
        SpellcastingController.OnCastingSpellAOE += SwitchTilesToAOEAttackSelectionMode;
        TrapTileController.OnTrapTile += SwitchTilesToTrapTileSelectionMode;
        SummoningController.OnSummoningRitual += SwitchTilesToSummonZoneSelectionMode;
    }

    private void OnDisable()
    {
        TileController.OnTileClickedMeleeMode -= SetUnitAsMeleeTarget;
        TileController.OnTileClickedAttackMode -= SetUnitAsTarget;
        TileController.OnTileClickedTrapTileMode -= SetTileAsTrapTileTarget;
        TileController.OnTileClickedAOESpellMode -= SetUnitAsSpellEpicenter;
        MeleeController.OnMeleeAttack -= SwitchTilesToMeleeSelectionMode;
        SpellcastingController.OnCastingSpell -= SwitchTilesToAttackSelectionMode;
        SpellcastingController.OnCastingSpellAOE -= SwitchTilesToAOEAttackSelectionMode;
        TrapTileController.OnTrapTile -= SwitchTilesToTrapTileSelectionMode;
        SummoningController.OnSummoningRitual -= SwitchTilesToSummonZoneSelectionMode;
    }

    public void SwitchTilesToMeleeSelectionMode()
    {
        GameObject[] battlefieldTiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (var battlefieldTile in battlefieldTiles)
        {
            //Switch all Battlefield on Tiles to Attack Selection Mode.
            battlefieldTile.GetComponent<TileController>().currentSingleTileStatus = SingleTileStatus.meleeSelectionModeActive;
        }
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

    public void SwitchTilesToAOEAttackSelectionMode()
    {
        GameObject[] battlefieldTiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (var battlefieldTile in battlefieldTiles)
        {
            //Switch all Battlefield on Tiles to AOE Attack Selection Mode.
            battlefieldTile.GetComponent<TileController>().currentSingleTileStatus = SingleTileStatus.aoeAttackSelectionModeActive;
            Debug.Log("All Tiles on the Battlefield switched to AOE Attack Selection Mode");
        }
    }
    public void SwitchTilesToTrapTileSelectionMode()
    {
        GameObject[] battlefieldTiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (var battlefieldTile in battlefieldTiles)
        {
            //Switch all Battlefield on Tiles to AOE Attack Selection Mode.
            battlefieldTile.GetComponent<TileController>().currentSingleTileStatus = SingleTileStatus.trapTileSelectionModeActive;
            Debug.Log("All Tiles on the Battlefield switched to Trap Tile Selection Mode");
        }
    }

    public void SwitchTilesToSummonZoneSelectionMode()
    {
        GameObject[] battlefieldTiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (var battlefieldTile in battlefieldTiles)
        {
            //Switch all Battlefield on Tiles to AOE Attack Selection Mode.
            battlefieldTile.GetComponent<TileController>().currentSingleTileStatus = SingleTileStatus.summonAreaSelectionModeActive;
            Debug.Log("All Tiles on the Battlefield switched to Summon Zone Selection Mode");
        }
    }

    public Unit SetUnitAsMeleeTarget(int tileXCoordinate, int tileYCoordinate)
    {
        Unit targetUnit = GridManager.Instance.GetTileControllerInstance(tileXCoordinate, tileYCoordinate).GetComponent<TileController>().detectedUnit.GetComponent<Unit>();
        OnMeleeTargetedUnit(targetUnit);
        return targetUnit;
    }

    public Unit SetUnitAsTarget(int tileXCoordinate, int tileYCoordinate)
    {
        Unit targetUnit = GridManager.Instance.GetTileControllerInstance(tileXCoordinate, tileYCoordinate).GetComponent<TileController>().detectedUnit.GetComponent<Unit>();
        OnTargetedUnit(targetUnit);
        return targetUnit;
    }

    public TileController SetTileAsTrapTileTarget(int tileXCoordinate, int tileYCoordinate)
    {
        TileController trapTileTarget = GridManager.Instance.GetTileControllerInstance(tileXCoordinate, tileYCoordinate).GetComponent<TileController>();
        OnTileSetAsTrap(trapTileTarget);
        return trapTileTarget;
    }

    public Unit SetUnitAsSpellEpicenter(int tileXCoordinate, int tileYCoordinate)
    {
        Unit epicenterUnit = GridManager.Instance.GetTileControllerInstance(tileXCoordinate, tileYCoordinate).GetComponent<TileController>().detectedUnit.GetComponent<Unit>();
        OnUnitSetAsSpellEpicenter(epicenterUnit);
        return epicenterUnit;
    }
}
