using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [SerializeField] GridManager gridManager;

    List<TileController> obstacles = new List<TileController>();

    // Subscribe to Player Turn

    private void Start()
    {
        foreach (var tile in gridManager?.gridTileControllers)
        {
            if (tile.tileType == TileType.Obstacle)
            {
                var spriteRenderer = tile.gameObject.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.red;
                    Debug.Log("Set up Obstacle Color");
                    OccupyObstacleTile(tile);
                }
                else
                {
                    Debug.Log("Unable to find SpriteRenderer Component in Obstacle Tile");
                }
            }
        }
    }

    private void OccupyObstacleTile(TileController obstacleTile)
    {
        obstacleTile.currentSingleTileCondition = SingleTileCondition.occupied;
        Debug.Log(obstacleTile + " set to" + obstacleTile.currentSingleTileCondition);
    }
    private List<TileController> GetObstaclesList()
    {

        foreach (var tile in gridManager?.gridTileControllers)
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
            else
            {
                Debug.Log("No targets found around the Obstacle");
            }
        }
    }
}
