using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPartyController : MonoBehaviour
{
    public GameObject[] playerUnitsOnBattlefield;
    // Start is called before the first frame update
    void Start()
    {
        playerUnitsOnBattlefield = GameObject.FindGameObjectsWithTag("Player");
        foreach (var playerUnit in playerUnitsOnBattlefield)
        {
            if (playerUnit.GetComponent<Unit>() != null)
            {
                TileController playerUnitTileController = GridManager.Instance.GetTileControllerInstance(playerUnit.GetComponent<Unit>().startingXCoordinate, playerUnit.GetComponent<Unit>().startingYCoordinate);
                playerUnitTileController.detectedUnit = playerUnit;
            }
        }
    }
}
