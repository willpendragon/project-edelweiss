using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EnemyType
{
dummy1,
dummy2,
dummy3
}

[CreateAssetMenu(fileName = "New Level", menuName = "Level", order = 1)]
public class Level : ScriptableObject
{
    //public GameObject[] enemySelection;
    public List<EnemyType> EnemyTypeIds;
    public List<Vector2> UnitCoordinates;
    public int levelNumber;
}