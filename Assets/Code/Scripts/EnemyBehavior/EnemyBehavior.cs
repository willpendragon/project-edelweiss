using UnityEngine;

public abstract class EnemyBehavior : ScriptableObject
{
    public abstract void ExecuteBehavior(EnemyAgent enemyAgent);
}