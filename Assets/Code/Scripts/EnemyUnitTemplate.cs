using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Unit Template", menuName = "EnemyUnitTemplate")]
public class EnemyUnitTemplate : UnitTemplate
{
    public enum EnemyPersonality
    {
    aggressive,
    passive
    }

    public EnemyPersonality enemyPersonality;
}
