using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeController : MonoBehaviour
{
    public delegate void MeleeAttack();
    public static event MeleeAttack OnMeleeAttack;
    public Unit currentMeleeTargetedUnit;
    //Magic number, fix later 12022024
    public float attackPower = 2;
    public DistanceController distanceController;

    public GameObject targetUnit;

    public void OnEnable()
    {
        GridTargetingController.OnMeleeTargetedUnit += SetTargetedUnit;
        TileController.OnTileConfirmedMeleeMode += ExecuteMeleeAttack;
    }
    public void OnDisable()
    {
        GridTargetingController.OnMeleeTargetedUnit -= SetTargetedUnit;
    }

    public void StartMeleeAttack()
    {
        OnMeleeAttack();
    }

    public void SetTargetedUnit(Unit targetedUnit)
    {
        //Sets the currently Targeted Enemy.
        currentMeleeTargetedUnit = targetedUnit;
    }

    public void ExecuteMeleeAttack()
    {
        if (distanceController.CheckDistance(GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().ownedTile, currentMeleeTargetedUnit.ownedTile))
        {
            attackPower = attackPower * 2;
            Debug.Log("Applying distance multiplier");
        }
        currentMeleeTargetedUnit.HealthPoints -= attackPower;
        Debug.Log("Attacking Unit using Melee Move");
    }
}

