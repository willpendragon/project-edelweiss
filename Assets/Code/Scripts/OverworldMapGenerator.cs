using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverworldMapGenerator : MonoBehaviour
{
    public Level[] levelList;
    public GameObject mapNode;
    public Transform mapNodeTransform;
    // Start is called before the first frame update
    void Awake()
    {
        foreach (var level in levelList)
        {
            mapNode.GetComponent<EnemySelection>().EnemyTypeIds = level.EnemyTypeIds;
            mapNode.GetComponent<EnemySelection>().EnemyCoordinates = level.UnitCoordinates;
            mapNode.GetComponent<EnemySelection>().levelData = level;
            //Add Offset
            Vector3 mapNodePosition = mapNodeTransform.position;
            Instantiate(mapNode, mapNodePosition, Quaternion.identity);
        }
    }
}
