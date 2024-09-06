using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [SerializeField] GridManager gridManager;

    List<TileController> obstacles = new List<TileController>();

    // Subscribe to Player Turn
    private void ActivateObstacles()
    {
        // I should prevent Player Action during this phase, deactivating the Player Input, sending a notification to the UI
        // and then restore the Player Input after a Coroutine countdown could be a simple yet good approach. 

        foreach (var target in GetObstaclesList())
        {
            if (target.gameObject.GetComponent<Unit>() != null)
            {
                int obstacleDamage = 20;
                target.GetComponent<Unit>().HealthPoints -= obstacleDamage;
            }
        }
    }
    private List<TileController> GetObstaclesList()
    {

        foreach (var tile in gridManager.gridTileControllers)
        {
            if (tile.tileType == TileType.Obstacle)
            {
                int obstacleHazardRange = 1;
                gridManager.gameObject.GetComponentInChildren<GridMovementController>().GetMultipleTiles(tile, obstacleHazardRange);
                obstacles.Add(tile);
            }
        }
        return obstacles;
    }
}
