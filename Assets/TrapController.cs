using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    public void OnEnable()
    {
        //TurnController.OnEnemyTurnSwap += ApplyTrapEffect;
    }
    public void OnDisable()
    {
        //TurnController.OnEnemyTurnSwap -= ApplyTrapEffect;
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
        Debug.Log("Applying Trap Effect to the Unit standing on the Trap Tile");
        if (GetComponentInParent<TileController>().detectedUnit != null)
        {
            Unit detectedUnitOnTrapTile = GetComponentInParent<TileController>().detectedUnit.GetComponent<Unit>();
            detectedUnitOnTrapTile.TakeDamage(10);
            //Beware of Magic Number
        }
    }
}
