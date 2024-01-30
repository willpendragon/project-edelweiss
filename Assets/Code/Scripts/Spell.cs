using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellAlignment
{
    fire,
    water,
    darkness,
    electricity
}
public enum SpellType
{
    singleTarget,
    aoe,
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
    // Add more spell properties as needed
}