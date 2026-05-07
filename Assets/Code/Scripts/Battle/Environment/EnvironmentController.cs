using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    public void Start()
    {
        SpawnEnvironment();
    }

    public void SpawnEnvironment()
    {
        var environment = GridManager.Instance.currentMapData.environment;
        Vector3 spawnPoint = GridManager.Instance.currentMapData.environmentSpawnpoint;
        if (environment != null)
            return;
        Instantiate(environment, spawnPoint, Quaternion.identity);
    }
}