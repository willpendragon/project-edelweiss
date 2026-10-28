using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesDebug : MonoBehaviour
{
    [SerializeField] GameStatsManager _gameStatsManager;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ReplenishKey();
    }

    void ReplenishKey()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _gameStatsManager.unlockedPuzzleKeys = 100;
            Debug.Log("Replenished Keys");
        }
    }
}
