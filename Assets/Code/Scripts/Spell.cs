using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellAlignment
{
    Fire,
    Water,
    Dark,
    Spark,
    Light
}
public enum SpellType
{
    SingleTarget,
    AOE,
}

public enum SpellSecundaryEffect
{
    NoEffect,
    Stun
}


[CreateAssetMenu(fileName = "New Spell", menuName = "Spell")]
public class Spell : ScriptableObject
{
    public string spellName;
    public int damage;
    public int manaPointsCost;
    public int opportunityPointsCost;
    public int spellRange;
    public SpellAlignment alignment;
    public SpellType spellType;
    public SpellSecundaryEffect spellSecundaryEffect;
    public GameObject spellVFX;
    public Vector3 spellVFXOffset;


    public float criticalHitChance;
}