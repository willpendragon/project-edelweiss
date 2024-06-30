using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceController : MonoBehaviour
{
    public GridMovementController gridMovementController;

    public bool CheckDistance(TileController attackerTile, TileController defenderTile)
    {
        int distanceThreshold = 10;
        if (gridMovementController.GetDistance(attackerTile, defenderTile) <= distanceThreshold)
        {
            Debug.Log("Distance Check: Attacker is close to Defender. Attack Modifier will apply");
            return true;
        }
        else
        {
            Debug.Log("Distance Check: Attacker is distant from Defender. Attack Modifier will NOT apply");
            return false;
        }
    }

}
