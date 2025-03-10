using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "New Unit Template", menuName = "UnitTemplate")]
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

    public float unitMeleeAttackBaseDamage;

    [Header("Gameplay")]

    public List<Spell> spellsList;
    public Vector2 coinsRewardRange;
    public float unitExperiencePointsReward;

    [Header("Visuals")]

    public Sprite unitPortrait;
    public Sprite unitMiniPortrait;
    public GameObject unitCalloutPortrait;

    [Header("Voices")]

    public GameObject unitCriticalHitVoice;
}
