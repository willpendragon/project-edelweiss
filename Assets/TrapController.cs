using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    public void OnEnable()
    {
        TurnController.OnEnemyTurnSwap += ApplyTrapEffect;
    }
    public void OnDisable()
    {
        TurnController.OnEnemyTurnSwap -= ApplyTrapEffect;
    }
    public enum TrapActivationStatus
    {
        active,
        inactive
    }

    public TrapActivationStatus currentTrapActivationStatus;

    private void Start()
    {
        currentTrapActivationStatus = TrapActivationStatus.inactive;
    }
    public void ApplyTrapEffect()
    {
        if (GetComponentInParent<TileController>().detectedUnit != null && currentTrapActivationStatus == TrapActivationStatus.active)
        {
            Unit detectedUnitOnTrapTile = GetComponentInParent<TileController>().detectedUnit.GetComponent<Unit>();
            detectedUnitOnTrapTile.HealthPoints -= 10;
            Debug.Log("Applying Trap Effect to the Unit standing on the Trap Tile");
        }
    }
}
