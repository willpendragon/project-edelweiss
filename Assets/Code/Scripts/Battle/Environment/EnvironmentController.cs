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
        Instantiate(environment);
    }
}
