using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Unit Template", menuName = "UnitTemplates/EnemyUnitTemplate")]
public class EnemyUnitTemplate : UnitTemplate
{
    public enum EnemyPersonality
    {
        aggressive,
        passive
    }

    public EnemyPersonality enemyPersonality;
    public Sprite alternateSprite;

    // Modifiers

    [SerializeField] private float _elementalModifier;

    public override float GetElementalModifier() => _elementalModifier;
    public override Sprite GetAlternateSprite() => alternateSprite;
}


