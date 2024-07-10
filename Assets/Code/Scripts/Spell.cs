using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellAlignment
{
    fire,
    water,
    darkness,
    electricity,
    light
}
public enum SpellType
{
    SingleTarget,
    AOE,
}
[CreateAssetMenu(fileName = "New Spell", menuName = "Spell")]
public class Spell : ScriptableObject
{
    public string spellName;
    public int damage;
    public int manaPointsCost;
    public int opportunityPointsCost;
    public SpellAlignment alignment;
    public SpellType spellType;
    public GameObject spellVFX;
    public Vector3 spellVFXOffset;

    public float criticalHitChance;
}