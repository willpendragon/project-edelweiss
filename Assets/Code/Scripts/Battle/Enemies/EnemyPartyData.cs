using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    dummy1,
    dummy2,
    dummy3,
    Wildermann
}

[System.Serializable]
public class EnemyWeight
{
    public EnemyType enemyType;
    public int weight;
}

[CreateAssetMenu(fileName = "Enemy Party", menuName = "Enemy Party", order = 1)]
public class EnemyPartyData : ScriptableObject
{
    public int minEnemyPoolSize;
    public int maxEnemyPoolSize;
    public List<EnemyWeight> enemyWeights;
}
