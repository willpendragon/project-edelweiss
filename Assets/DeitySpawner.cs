using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeitySpawner : MonoBehaviour
{
    [SerializeField] GameObject deity;
    [SerializeField] Transform deitySpawnPosition;
    // Start is called before the first frame update
    void Start()
    {
        var deityRoll = Random.Range(0, 7);
        if (deityRoll <= 6 && deityRoll >= 3)
        {
            Instantiate(deity, deitySpawnPosition);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
