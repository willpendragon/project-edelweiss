using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTileController : MonoBehaviour
{
    public delegate void TrapTile();
    public static event TrapTile OnTrapTile;

    public TileController currentTrapTileTargetedUnit;

    public void StartTrapTile()
    {
        OnTrapTile();
    }

    public void SetTargetedTrapTile(TileController targetedTrapTile)
    {
        //Sets the currently Targeted Tile (where the trap will be active).
        currentTrapTileTargetedUnit = targetedTrapTile;

    }

    public void ExecuteTrapTileAction()
    {
        Debug.Log("Executing Trap Tile Action");
        TrapController trapController = currentTrapTileTargetedUnit.gameObject.GetComponentInChildren<TrapController>();
        trapController.currentTrapActivationStatus = TrapController.TrapActivationStatus.active;
        Debug.Log("Trap Tile is now Active");
    }
}
