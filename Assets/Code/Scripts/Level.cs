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

[CreateAssetMenu(fileName = "New Level", menuName = "Level", order = 1)]
public class Level : ScriptableObject
{
    public int levelNumber;
    public int minEnemyPoolSize;
    public int maxEnemyPoolSize;
    public List<EnemyWeight> enemyWeights;
}
