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
[CreateAssetMenu(fileName = "New Spell", menuName = "Spell")]
public class Spell : ScriptableObject
{
    public string spellName;
    public int damage;
    public int manaPointsCost;
    public int opportunityPointsCost;
    public SpellAlignment alignment;
    // Add more spell properties as needed
}