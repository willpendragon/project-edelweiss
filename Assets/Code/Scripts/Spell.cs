using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spell")]
public class Spell : ScriptableObject
{
    public string spellName;
    public int damage;
    public int manaPointsCost;
    public int opportunityPointsCost;
    // Add more spell properties as needed
}