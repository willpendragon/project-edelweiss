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
    public AnimationClip alternateIdle;
    public RuntimeAnimatorController alternateAnimator;

    // Modifiers

    [SerializeField] private float _elementalModifier;

    public override float GetElementalModifier() => _elementalModifier;
    public override Sprite GetAlternateSprite() => alternateSprite;
    public override AnimationClip GetAlternateIdle() => alternateIdle;
    public override RuntimeAnimatorController GetAlternateAnimator() => alternateAnimator;

}


