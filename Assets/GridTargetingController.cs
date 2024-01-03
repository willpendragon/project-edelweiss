using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridTargetingController : MonoBehaviour
{
    public GridManager gridManager;
    private void OnEnable()
    {
        TileController.OnTileClickedAttackMode += SetUnitAsTarget;
    }

    private void OnDisable()
    {
        TileController.OnTileClickedAttackMode -= SetUnitAsTarget;
    }
    public Unit SetUnitAsTarget(int tileXCoordinate, int tileYCoordinate)
    {
        Unit targetUnit = gridManager.GetTileControllerInstance(tileXCoordinate, tileYCoordinate).GetComponent<Unit>();
        return targetUnit;
    }
}
