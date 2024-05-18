using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{

    public delegate void TrapAction();
    public static event TrapAction OnTrapAction;
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
        if (GetComponentInParent<TileController>().detectedUnit != null)
        {
            Unit detectedUnitOnTrapTile = GetComponentInParent<TileController>().detectedUnit.GetComponent<Unit>();
            if (detectedUnitOnTrapTile.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
                detectedUnitOnTrapTile.TakeDamage(10);

            OnTrapAction();
            //Beware of Magic Number

            Debug.Log("Applying Trap Effect to the Unit standing on the Trap Tile");
        }
    }
}
