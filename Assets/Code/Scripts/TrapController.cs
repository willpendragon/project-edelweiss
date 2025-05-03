using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{

    public delegate void TrapAction();
    public static event TrapAction OnTrapAction;
    float spikeDamage = 300;

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

            if (SpikeKillingPlayer(detectedUnitOnTrapTile, spikeDamage))
            {
                detectedUnitOnTrapTile.GetComponent<CrystalHandler>()?.TurnUnitIntoCrystal();
                detectedUnitOnTrapTile.TakeDamage(spikeDamage);
                Debug.Log("Targeted Unit became a Capture Crystal");
            }
            else if (detectedUnitOnTrapTile.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
            {
                OnTrapAction();
                detectedUnitOnTrapTile.TakeDamage(spikeDamage);

                Debug.Log("Applying Trap Effect to the Unit standing on the Trap Tile");
                TurnController.Instance.GameOverCheck();
            }
        }
    }

    public bool SpikeKillingPlayer(Unit detectedUnitOnTraptile, float spikeDamage)
    {
        float predictedHPOutcome = detectedUnitOnTraptile.unitHealthPoints - spikeDamage;
        if (predictedHPOutcome <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
