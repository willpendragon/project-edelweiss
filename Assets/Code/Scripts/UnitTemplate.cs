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

    public int unitShieldPoints;
    public float meleeAttackPower;
    public float unitMagicPower;

    [Header("Gameplay")]

    public List<Spell> spellsList;
    public float unitCoinsReward;
    public float unitExperiencePointsReward;


    [Header("Visuals")]

    public Sprite unitPortrait;
    public Sprite unitMiniPortrait;
    public GameObject unitCalloutPortrait;

    [Header("Voices")]

    public GameObject unitCriticalHitVoice;
}
