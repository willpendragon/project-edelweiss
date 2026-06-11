using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ObstacleController : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] GameObject _mirrorPrefab;

    List<TileController> obstacles = new List<TileController>();

    // Subscribe to Player Turn

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.E))
    //    {
    //        SpawnObstacles();
    //    }
    //}

    public void SpawnObstacles()
    {
        foreach (var tile in gridManager.gridMapDictionary.Values)
        {
            if (tile.tileType == TileType.Obstacle)
            {
                Debug.Log($"Found Obstacle on {tile.gameObject}");

                OccupyObstacleTile(tile);
                SpawnMirror(tile);
            }
            else
            {
                Debug.Log("Unable to find SpriteRenderer Component in Obstacle Tile");
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

    private void SpawnMirror(TileController tile)
    {
        int x = tile.tileXCoordinate;
        int y = tile.tileYCoordinate;

        var tileWorldPosition = gridManager.GetWorldPositionFromGridCoordinates(x, y);
        float yOffset = 1f;
        tileWorldPosition = new Vector3(tileWorldPosition.x, yOffset, tileWorldPosition.z);
        GameObject mirrorInstance = Instantiate(_mirrorPrefab, tileWorldPosition, Quaternion.identity);
        tile.detectedUnit = mirrorInstance;
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
