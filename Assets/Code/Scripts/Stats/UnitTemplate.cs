using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Template", menuName = "UnitTemplates/UnitTemplate")]
public class UnitTemplate : ScriptableObject

{
    [Header("Stats")]

    public string unitName;
    public int unitHealthPoints;
    public int unitMaxHealthPoints;
    public int unitManaPoints;
    public int unitMaxManaPoints;
    public int unitOpportunityPoints;
    public int unitFaithPoints;
    public int unitMovemementLimit;
    public int unitShieldPoints;
    public float meleeAttackPower;
    public float unitMagicPower;
    public int unitMaxFoodSlots;

    public float unitMeleeAttackBaseDamage;

    [Header("Gameplay")]

    public List<Spell> spellsList;
    public Vector2 coinsRewardRange;
    public float unitExperiencePointsReward;
    public PhysicalAttackBehavior physicAttackBehavior;

    [Header("Visuals")]

    public Sprite unitPortrait;
    public Sprite unitMiniPortrait;
    public GameObject unitCalloutPortrait;
    public Sprite unitBattlePortrait;

    [Header("Voices")]

    public GameObject unitCriticalHitVoice;

    public virtual float GetElementalModifier() => 1.0f; // Valore di default

    public virtual Sprite GetAlternateSprite() => null;
}